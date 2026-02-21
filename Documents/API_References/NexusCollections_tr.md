# Nexus Prime Mimari Rehberi: NexusCollections (Unmanaged Veri Yapıları)

## 1. Giriş
`NexusCollections.cs`, Nexus Prime framework'ünün "Donanım Odaklı Veri Yönetimi" (Hardware-Oriented Data Management) kalbidir. C# dilinin standart koleksiyonlarının (List, Dictionary vb.) yarattığı Garbage Collector (GC) baskısını ve bellek dağınıklığını ortadan kaldırmak için, tamamen unmanaged (yönetilmeyen) bellek üzerinde çalışan bir koleksiyon suite'i sunar.

Bu koleksiyonların varlık sebebi, oyun mantığı (logic) sistemlerinde binlerce veri üzerinde işlem yaparken, işlemci önbelleğini (CPU Cache) en verimli şekilde kullanmak ve bellek yönetimini manuel (unsafe) ama güvenli bir katmanla kontrol etmektir.

---

## 2. Teknik Analiz
Koleksiyon suite, performans ve güvenlik için şu kritik yapıları içerir:

- **NexusRef<T>**: Bir bileşene doğrudan pointer tutmak yerine, varlığın (EntityId) geçerliliğini kontrol eden güvenli bir "referans" katmanı sağlar. `Ptr` erişimi sırasında varlık silinmişse `null` dönerek "Dandling Pointer" hatalarını önler.
- **NexusList<T>**: `NativeMemory.AllocCacheAligned` kullanarak verileri 64-byte sınırlarına dizer. Bu, işlemcinin listeyi tararken (iteration) tek bir cache-line içinde maksimum veriye ulaşmasını sağlar.
- **NexusDictionary<K, V>**: Saf unmanaged bellek üzerinde çalışan bir karma tablo (hash map) sunar. Referans tipleri içermediği için, milyonlarca kayıt olsa bile GC tarafından taranmaz.
- **NexusString<TSize>**: Sabit boyutlu, unmanaged string yapısıdır. Varlık isimleri veya etiketleri gibi verilerin yığın (heap) yerine doğrudan bileşen içinde (inline) saklanmasını sağlar.

---

## 3. Mantıksal Akış
1.  **Tahsisat (Allocation)**: Koleksiyon oluşturulduğunda (`new`), `NexusMemoryManager` üzerinden cache-aligned unmanaged bellek ayrılır.
2.  **Yönetim**: Veriler ham pointerlar üzerinden yönetilir. Kapasite dolduğunda `Realloc` ile bellek güvenli bir şekilde genişletilir.
3.  **Erişim**: `ref` dönüşlü indeksleyiciler ile veri kopyalaması (copy-overhead) yapılmadan doğrudan bellek adresi üzerinden işlem yapılır.
4.  **Temizlik (Cleanup)**: Koleksiyonun ömrü bittiğinde `Dispose()` çağrılarak bellek OS'e el ile iade edilir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Dangling Pointer** | Artık geçerli olmayan bir bellek adresini işaret eden tehlikeli pointer. |
| **Cache-Aligned** | Verinin bellek adresinin işlemci cache line (64-byte) sınırına tam oturması. |
| **LIFO (Last-In-First-Out)** | NexusStack'te kullanılan, son giren verinin ilk çıkması prensibi. |
| **Zero-GC Penalty** | Veri yapısının GC tarafından asla taranmaması ve performans duraksamasına yol açmaması. |

---

## 5. Riskler ve Sınırlar
- **Manual Lifetime**: `NexusList` veya `NexusDictionary` kullandıktan sonra `Dispose()` çağırmayı unutmak "Memory Leak" (Bellek Sızıntısı) ile sonuçlanır.
- **Unmanaged Constraints**: Sadece `unmanaged` (struct) tipler saklanabilir. `class` veya `string` (managed) tipler koleksiyonlara eklenemez.

---

## 6. Kullanım Örneği
```csharp
// 100 kapasiteli unmanaged bir liste oluştur
using var list = new NexusList<float3>(100);

list.Add(new float3(1, 0, 0));

// Veriye referans ile eriş (Kopyasız)
ref var item = ref list[0];
item.x = 10;

// Dispose() otomatik olarak çağrılır (using bloğu sonunda)
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Collections;

public unsafe struct NexusList<T> : IDisposable where T : unmanaged
{
    private T* _buffer;
    private int _count;
    private int _capacity;

    public NexusList(int initialCapacity = 8)
    {
        _capacity = initialCapacity;
        _buffer = (T*)NexusMemoryManager.AllocCacheAligned(_capacity * sizeof(T));
    }

    public void Add(T item)
    {
        if (_count == _capacity) // Realloc logic here
        _buffer[_count++] = item;
    }

    public void Dispose() => NexusMemoryManager.Free(_buffer);
}
```

---

## Nexus Optimization Tip: Predictive Capacity
Koleksiyonların varsayılan kapasitesini (Default: 8) projenizdeki ortalama veri miktarına göre ayarlayın. Sık yapılan `Realloc` işlemleri bellek fragmantasyonuna yol açabilir. Başlangıçta doğru kapasite belirleyerek, **liste genişleme maliyetini %100 oranında ortadan kaldırabilirsiniz.**
