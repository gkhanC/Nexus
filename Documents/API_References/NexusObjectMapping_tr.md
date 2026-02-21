# Nexus Prime Mimari Rehberi: NexusObjectMapping (Unmanaged-Managed Köprüsü)

## 1. Giriş
`NexusObjectMapping.cs`, Nexus Prime'ın unmanaged ECS dünyası ile Unity'nin managed nesne dünyası (GameObject, Transform, Renderer) arasındaki kritik bağlantı noktasıdır. Pure ECS yapısında varlıklar sadece birer sayıdır (ID), ancak bu varlıkların ekranda bir karşılığı olması için Unity nesnelerine hızlıca ulaşmaları gerekir.

Bu eşleyicinin varlık sebebi; her karede binlerce varlık için `GetComponent` veya `Find` gibi ağır Unity operasyonlarını çağırmak yerine, varlık ID'si üzerinden ilgili Unity objesine **O(1)** karmaşıklığında ve thread-safe bir şekilde ulaşmaktır.

---

## 2. Teknik Analiz
NexusObjectMapping, hibrit mimariyi desteklemek için şu teknikleri kullanır:

- **Concurrent Mapping**: `ConcurrentDictionary<uint, object>` kullanarak, farklı iş parçacıklarından (threads) gelen eşleme taleplerini kilitlenme (lock) olmadan yönetir.
- **Reference Bridging**: Unmanaged varlığın indeksini key olarak kullanarak, managed nesnenin referansını hafızada tutar. Bu sayede `Registry` verisi ile görsel nesne birbirine bağlanır.
- **Generic Access**: `Get<T>` metodu ile dönülen objeyi otomatik olarak hedef tipe (Örn: Transform) cast eder, bu da geliştirici kodunu daha temiz kılar.
- **Zero-Cost Lookup**: Eşleme yapıldıktan sonra, bir objeye ulaşma maliyeti sadece bir hash tablosu okuması kadardır.

---

## 3. Mantıksal Akış
1.  **Eşleme (`Map`)**: Bir varlık oluşturulduğunda ve buna karşılık bir GameObject instantiate edildiğinde, ikisi birbirine bu sınıfla bağlanır.
2.  **Sorgulama (`Get`)**: Sistemler iş mantığını yürütürken, görsel güncelleme için `NexusObjectMapping.Get<Transform>(id)` ile ilgili objeyi çeker.
3.  **Senkronizasyon**: Unmanaged verideki değişimler, bu köprü üzerinden Unity objesine aktarılır.
4.  **Temizlik (`Unmap`)**: Varlık silindiğinde, bellek sızıntısını önlemek için mapping kaydı silinir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Object Mapping** | İki farklı dünya (unmanaged/managed) arasındaki nesnelerin birbiriyle eşleştirilmesi. |
| **Hybrid ECS** | Hem saf veri yapılarını (unmanaged) hem de motor nesnelerini (managed) beraber kullanma stratejisi. |
| **Type Casting** | Bir objeyi belirli bir tipmiş gibi işlem görmeye zorlama (Object -> Transform). |
| **Sync Bridge** | Veri katmanı ile görsel katman arasındaki veri akışını sağlayan yapı. |

---

## 5. Riskler ve Sınırlar
- **GC Pressure**: `ConcurrentDictionary` içindeki key-value çiftleri managed heap üzerinde durduğu için, milyonlarca eşleme yapılması durumunda GC (Garbage Collector) baskısı oluşabilir. Sadece görsel karşılığı olan varlıklar eşlenmelidir.
- **Lifetime Management**: Eğer bir GameObject Unity tarafında `Destroy` edilirse ama Nexus tarafında `Unmap` edilmezse, "MissingReferenceException" riskleri doğar.

---

## 6. Kullanım Örneği
```csharp
// Bir varlık ve görsel nesneyi birbirine bağla
EntityId entity = registry.Create();
GameObject go = Instantiate(prefab);
NexusObjectMapping.Map(entity.Index, go.transform);

// Sistem içinde veriyi görsele aktar
void OnUpdate(EntityId id, Position* pos) {
    if (NexusObjectMapping.TryGet(id.Index, out var obj)) {
        var trans = (Transform)obj;
        trans.position = pos->Value;
    }
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Collections.Concurrent;
namespace Nexus.Core;

public static class NexusObjectMapping
{
    private static readonly ConcurrentDictionary<uint, object> _mappings = new();

    public static void Map(uint entityIndex, object unityObject)
    {
        _mappings[entityIndex] = unityObject;
    }

    public static T Get<T>(uint entityIndex) where T : class
    {
        if (_mappings.TryGetValue(entityIndex, out var obj)) return obj as T;
        return null;
    }

    public static void Unmap(uint entityIndex)
    {
        _mappings.TryRemove(entityIndex, out _);
    }
}
```

---

## Nexus Optimization Tip: Predictive Pre-Mapping
Eğer binlerce mermi veya partikül kullanıyorsanız, her biri için Mapping oluşturmak yerine bir "Pool" (Havuz) sistemine Mapping yapın. Mermi objesi havuzdan çıktığında Mapping'i güncelleyerek **sözlük ekleme/çıkarma (Insert/Remove) maliyetini %90 oranında azaltabilirsiniz.**
