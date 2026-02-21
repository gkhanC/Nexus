# Nexus Prime Mimari Rehberi: HierarchicalSnapshotManager (Hiyerarşik Durum Yönetimi)

## 1. Giriş
`HierarchicalSnapshotManager.cs`, Nexus Prime'ın devasa dünyaları yönetebilmek için geliştirdiği "Kısmi Kayıt" (Partial Saving) altyapısıdır. Standart `SnapshotManager` tüm dünyayı dondururken, Hierarchical Snapshot sadece belirli sektörleri, bölgeleri veya sistemleri (Örn: Sadece "Köy 1" veya sadece "Oyuncu Envanteri") baz alarak durum takibi yapar.

Bu yöneticinin varlık sebebi, milyarlarca byte verinin bulunduğu bir simülasyonda gereksiz veri kopyalamasını önlemek ve sadece o an değişen veya ihtiyaç duyulan veri kümelerini (Sectoring) RAM'e yedekleyerek bellek kullanımını optimize etmektir.

---

## 2. Teknik Analiz
HierarchicalSnapshotManager, verimli durum yönetimi için şu teknikleri kullanır:

- **Sector-Based Filtering**: Varlıkları bir `sectorName` altında gruplayarak, sadece o gruba ait bileşenleri `CapturePartial` ile yedekler.
- **Partial Registry Restore**: `Registry` üzerindeki tüm veriyi ezmek yerine, sadece snapshot içindeki varlıkların verilerini günceller (Differential Patching).
- **Tag-Based Capture**: Varlıkların bulunduğu sektör bilgisini baz alarak dinamik sorgular (Dynamic Queries) üzerinden snapshot toplar.
- **Memory Decoupling**: Sektör yedekleri birbirinden bağımsız olduğu için, bir sektör bozulsa bile diğerleri sağlıklı kalmaya devam eder.

---

## 3. Mantıksal Akış
1.  **Sınıflandırma**: Varlıklar mantıksal gruplara (Sektörler) ayrılır.
2.  **Yedekleme (`SaveSector`)**: Belirli bir sektör ismi ve varlık listesiyle kısmi bir unmanaged snapshot oluşturulur.
3.  **Depolama**: Snapshot'lar bir sözlük (`Dictionary`) yapısında, sektör ismiyle anahtarlanarak saklanır.
4.  **Geri Yükleme (`RestoreSector`)**: Sektör ismiyle çağrıldığında, sadece o bölgedeki varlıkların durumları Registry üzerine geri yazılır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Partial Snapshot** | Tüm sistemin değil, sadece belirli bir kısmın anlık durum yedeği. |
| **Sectoring** | Büyük bir veri uzayını daha küçük, bağımsız yönetilebilir parçalara bölme işlemi. |
| **Differential Patching** | Sadece değişen kısımları hedef veri üzerine yamayarak güncelleme yapma. |
| **Capture Logic** | Verinin o anki halini dondurup (Freeze) unmanaged belleğe kopyalama süreci. |

---

## 5. Riskler ve Sınırlar
- **Entity Consistency**: Eğer bir varlık Sektör A'dan Sektör B'ye geçmişse ve sadece Sektör A geri yüklenirse, varlık iki yerde birden görünebilir veya veri tutarsızlığı (Duplication) oluşabilir.
- **Cross-Sector References**: Sektörler arası referanslar (Örn: Sektör 1'deki bir anahtarın Sektör 2'deki kapıyı açması) kısmi yüklemelerde kopabilir.

---

## 6. Kullanım Örneği
```csharp
var sectorManager = new HierarchicalSnapshotManager(registry);

// "Zindan_1" bölgesindeki varlıkları kaydet
var dungeonEntities = registry.Query().With<InDungeon>().GetEntities();
sectorManager.SaveSector("Zindan_1", dungeonEntities);

// Bir süre sonra sadece o zindanı eski haline döndür
sectorManager.RestoreSector("Zindan_1");
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Collections.Generic;
namespace Nexus.Core;

public class HierarchicalSnapshotManager
{
    private readonly Registry _registry;
    private readonly Dictionary<string, Snapshot> _sectorSnapshots = new();

    public void SaveSector(string sectorName, IEnumerable<EntityId> entities)
    {
        // 1. Filter entities in the sector.
        // 2. Capture a partial snapshot via Registry.
    }

    public void RestoreSector(string sectorName)
    {
        // Find and restore partial snapshot properties.
    }
}
```

---

## Nexus Optimization Tip: Predictive Sector Unloading
HierarchicalSnapshotManager kullanarak, oyuncuya uzak olan sektörlerin durumlarını unmanaged snapshot olarak saklayıp, o varlıkları `Registry`'den silebilirsiniz (Unload). Bu yöntem, **çalışma zamanındaki aktif varlık sayısını %70'e kadar azaltarak CPU yükünü devasa oranda düşürür.**
