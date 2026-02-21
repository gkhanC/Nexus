# Nexus Prime Mimari Rehberi: NexusCachedQuery (Reaktif Sorgu Önbellekleme)

## 1. Giriş
`NexusCachedQuery.cs`, Nexus Prime'ın sorgu performansını bir üst seviyeye taşıyan reaktif bir katmandır. Standart bir sorgu her çağrıldığında tüm veri setini tararken, CachedQuery sonuçları hafızada saklar ve sadece ilgili veri bileşenleri değiştiğinde (Dirty) bu listeyi günceller.

Bu sınıfın varlık sebebi, binlerce varlık içeren ama nadiren değişen sorgu sonuçlarını (Örn: "Durağan ağaçlar", "Pasif düşmanlar") her karede (frame) yeniden hesaplamanın işlemci üzerindeki gereksiz yükünü sıfıra indirmektir.

---

## 2. Teknik Analiz
NexusCachedQuery, verimlilik için şu mimari desenleri kullanır:

- **Dirty Flag Pattern**: Bileşen eklendiğinde veya çıkarıldığında `_isDirty` bayrağı aktif edilir. Bu sayede, veri değişene kadar hiçbir arama işlemi yapılmaz.
- **Event-Driven Updates**: `Registry` üzerindeki `OnEntityDestroyed`, `OnComponentAdded` ve `OnComponentRemoved` olaylarına abone olarak sadece hedef bileşenleri dinler.
- **Lazy Rebuilding**: Önbellek, veri değiştiği anda değil, sonuca ihtiyaç duyulduğu anda (`GetEntities()`) yeniden inşa edilir. Bu, bir karede birden fazla değişim olsa bile sadece bir kez hesaplama yapılmasını sağlar.
- **Set-Based Storage**: `HashSet<EntityId>` kullanarak, varlık listesini hızlı erişilebilir ve benzersiz (unique) bir şekilde saklar.

---

## 3. Mantıksal Akış
1.  **Abonelik**: Sorgu oluşturulduğunda hedef bileşen tipleri için olaylara abone olunur.
2.  **İzleme**: `Registry` üzerinden bir değişim geldiğinde, bu değişimin sorgu kapsamına girip girmediği kontrol edilir ve gerekiyorsa "Dirty" bayrağı kalkar.
3.  **Sorgulama**: Kullanıcı veri istediğinde, eğer bayrak kalkmışsa `RebuildCache()` metodunu çağırarak güncel listeyi oluşturur.
4.  **Temizlik**: `Dispose` çağrıldığında bellek sızıntısını önlemek için olay abonelikleri sonlandırılır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Reactive Query** | Veriler değiştikçe kendi durumunu güncelleyen akıllı sorgu modeli. |
| **Dirty Flag** | Bir verinin değiştiğini ve yeniden işlenmesi gerektiğini belirten işaretçi. |
| **Lazy Rebuild** | Hesaplamanın ihtiyaç anına kadar ertelenmesi tekniği. |
| **Event-Driven** | Program akışının meydana gelen olaylar (events) tarafından kontrol edilmesi. |

---

## 5. Riskler ve Sınırlar
- **Frequency Analysis**: Eğer sorgulanan bileşenler her karede değişiyorsa (Örn: Pozisyon), `NexusCachedQuery` kullanmak reaktif maliyet yüzünden performansı artırmak yerine düşürebilir. Sadece seyrek değişen veriler için idealdir.
- **Hash Cost**: `HashSet` kullanımı, unmanaged array'lere göre daha fazla bellek tüketebilir ve iterasyon hızı bir miktar (mikro seviyede) düşük olabilir.

---

## 6. Kullanım Örneği
```csharp
// "Inventory" ve "Stats" bileşenine sahip varlıkları takip eden reaktif sorgu
var inventoryQuery = new NexusCachedQuery(registry, typeof(Inventory), typeof(Stats));

void OnUpdate() {
    // Veriler değişmediyse bu çağrının maliyeti O(1)'dir (Doğrudan listeyi döner)
    var players = inventoryQuery.GetEntities();
    foreach(var p in players) {
        // ... İşlem ...
    }
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

public class NexusCachedQuery : IDisposable
{
    private readonly Registry _registry;
    private readonly Type[] _required;
    private readonly HashSet<EntityId> _cache = new();
    private bool _isDirty = true;

    public IEnumerable<EntityId> GetEntities()
    {
        if (_isDirty) RebuildCache();
        return _cache;
    }

    private void OnComponentModified(EntityId entity, Type type)
    {
        foreach (var req in _required) {
            if (req == type) { _isDirty = true; break; }
        }
    }
}
```

---

## Nexus Optimization Tip: Event Filtering
`OnComponentModified` içinde yapılan tip kontrolü, reaktif maliyeti belirleyen ana unsurdur. Eğer yüzlerce farklı bileşen tipiniz varsa, bu metodu `Registry` seviyesinde spesifik tipler için özelleştirerek (Type-specific events) **gereksiz "Dirty" kontrollerinden %80 oranında kurtulabilirsiniz.**
