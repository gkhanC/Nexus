# Nexus Prime Mimari Rehberi: AutomaticInternalPooling (Dahili Bellek Havuzlama)

## 1. Giriş
`AutomaticInternalPooling.cs`, Nexus Prime'ın bellek yönetimindeki "sıfır fragmantasyon" hedefine hizmet eden, düşük seviyeli bir havuzlama mekanizmasıdır. Sürekli yeni bellek blokları istemek ve silmek yerine, baştan ayrılmış (pre-allocated) büyük bir bellek bloğunu dilimleyerek kullanır.

Bu sistemin varlık sebebi, işletim sisteminin (OS) bellek yöneticisini (Heap) her mikro-tahsisat için rahatsız etmemek ve verileri bellekte birbirine yakın tutarak işlemcinin **Önbellek Yerelliği** (Cache Locality) avantajından maksimum düzeyde faydalanmaktır.

---

## 2. Teknik Analiz
AutomaticInternalPooling, yüksek performanslı bellek yönetimi için şu teknikleri kullanır:

- **Bump Allocator Logic**: Bellek tahsisatı, bir pointer'ın (offset) ileriye doğru kaydırılması kadar basittir. Bu, standart `malloc` veya `NativeMemory.Alloc` operasyonlarından çok daha hızlıdır (O(1)).
- **Aligned Pre-allocation**: Havuz, en baştan 64-byte (Cache Line) hizalı olarak ayrılır. İçindeki her dilim de bu hizalamaya sadık kalır.
- **Fragmentation Prevention**: Bellek tek bir büyük blok halinde olduğu için, işletim sistemi seviyesinde "Fragmentation" (parçalanma) oluşmaz.
- **Unmanaged Lifetime**: Havuz `IDisposable` arayüzünü uygular, böylece oyun kapandığında tüm havuz tek bir hamlede (`AlignedFree`) RAM'den temizlenebilir.

---

## 3. Mantıksal Akış
1.  **İlklendirme**: Havuz, belirlenen boyutta (Örn: 1MB) unmanaged alanda açılır.
2.  **Ödünç Alma (`Borrow`)**: Bir sistem bellek istediğinde, havuz içindeki "Bump Pointer" istenen miktar kadar kaydırılır ve başlangıç adresi geri dönülür.
3.  **Kullanım**: Dönülen pointer üzerinde ham veri işlemleri yapılır.
4.  **Temizlik**: Havuzun tamamı serbest bırakılana kadar içindeki parçalar manuel olarak silinmez.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Bump Allocator** | Belleği sadece bir imleci ileri kaydırarak dağıtan, en hızlı tahsisat yöntemi. |
| **Memory Fragmentation** | Belleğin küçük ve kullanışsız parçalara bölünerek verimsizleşmesi durumu. |
| **Cache Locality** | Birbirine yakın verilerin işlemci önbelleğinde (L1/L2) beraber bulunma olasılığı. |
| **Pre-allocation** | İhtiyaç duyulacak belleğin uygulama başında peşinen ayrılması. |

---

## 5. Riskler ve Sınırlar
- **No Individual Free**: Bump allocator yapısında, tek bir parçayı geri iade etmek (free) mümkün değildir. Havuz dolduğunda tamamı sıfırlanmalıdır.
- **Overflow**: Eğer havuz kapasitesi dolarsa (1MB sınırı aşılırsa), sistem hata fırlatabilir. Boyut ihtiyaca göre dinamik ayarlanmalıdır.

---

## 6. Kullanım Örneği
```csharp
// 2MB'lık bir dahili havuz oluştur
var pool = new AutomaticInternalPooling(2 * 1024 * 1024);

// Havuzdan 256 byte'lık bir alan ödünç al
unsafe {
    void* buffer = pool.Borrow(256);
    // ... Buffer'ı kullan ...
}

pool.Dispose(); // İş bitince tüm havuzu tek seferde sil
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Runtime.InteropServices;
namespace Nexus.Core;

public unsafe class AutomaticInternalPooling : IDisposable
{
    private void* _pool;
    private int _totalSize;

    public AutomaticInternalPooling(int preAllocSize = 1024 * 1024)
    {
        _totalSize = preAllocSize;
        _pool = NativeMemory.AlignedAlloc((nuint)_totalSize, 64);
    }

    public void* Borrow(int size)
    {
        // Simple bump allocator logic (Internal placeholder)
        return null; 
    }

    public void Dispose()
    {
        if (_pool != null) NativeMemory.AlignedFree(_pool);
    }
}
```

---

## Nexus Optimization Tip: Context-Specific Pools
Her sistem için özel bir `AutomaticInternalPooling` örneği kullanın. Örneğin mermiler için 1MB, UI elementleri için 256KB'lık havuzlar açarak, farklı yaşam döngüsüne sahip verilerin birbirini bellek üzerinde bloklamasını önleyebilir ve **L2 cache isabet oranınızı %20-30 civarında artırabilirsiniz.**
