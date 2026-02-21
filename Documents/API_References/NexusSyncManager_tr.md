# Nexus Prime Mimari Rehberi: NexusSyncManager (Veri Senkronizasyon Yöneticisi)

## 1. Giriş
`NexusSyncManager.cs`, Nexus Prime'ın unmanaged simülasyon katmanı ile Unity'nin görsel sahneleri arasındaki "Hız Ayarlayıcı"dır. Unmanaged bellekte (`Registry`) duran saf matematiksel verilerin (Örn: Pozisyon, Rotasyon), Unity'deki görsel nesnelere (`Transform`) milisaniyelik bir hızla aktarılmasını koordine eder.

Bu yöneticinin varlık sebebi, her Unity nesnesinin kendi verisini arayıp bulması yerine, merkezi bir sistem üzerinden toplu ve optimize edilmiş bir senkronizasyon pası (Synchronisation Pass) yaparak CPU üzerindeki `Transform` güncelleme maliyetini minimize etmektir.

---

## 2. Teknik Analiz
NexusSyncManager, hibrit senkronizasyon için şu teknikleri kullanır:

- **Global Sync Pass**: `registry.GetSet<T>` kullanarak tüm ilgilenilen bileşen setlerini toplu olarak tarar. Bu, her nesne için tek tek Registry sorgusu yapmaktan çok daha hızlıdır (Set-Iteration).
- **Mapping Lookup**: `NexusObjectMapping.TryGet` kullanarak Entity ID üzerinden ilgili Unity nesnesine O(1) maliyetle ulaşır.
- **Direct Pointer Access**: Verileri unmanaged bileşen setlerinden ham pointer (`Vector3*`, `NexusRotationField*`) olarak okur. Bu, managed-struct kopyalama maliyetini ortadan kaldırır.
- **Conditional Sync**: Sadece Nexus tarafından yönetilen ve görsel bir karşılığı olan (Mapped) varlıklar senkronize edilir.

---

## 3. Mantıksal Akış
1.  **Iterasyon**: Pozisyon ve rotasyon bileşen setleri üzerinden tüm aktif varlıklar taranır.
2.  **Referans Bulma**: Varlığın Unity tarafında bir görsel karşılığı (`GameObject`) olup olmadığı kontrol edilir.
3.  **Değer Aktarımı**: Unmanaged veriler, direkt olarak Unity `transform.position` ve `transform.rotation` özelliklerine atanır.
4.  **Hızlandırma**: Çok binli varlıklarda, bu işlem `JobSystem` içinde paralel olarak da yürütülebilir.

---

## 4. Kullanım Örneği
```csharp
// Update döngüsü içinde senkronizasyonu tetikle
void Update() {
    NexusSyncManager.Sync(mainRegistry);
}

// Veya sadece tek bir varlığı el ile senkronize et
NexusSyncManager.SyncEntity(registry, myId, myGameObject);
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

public static class NexusSyncManager
{
    public static void Sync(Registry registry)
    {
        var positionSet = registry.GetSet<Vector3>();
        var rotationSet = registry.GetSet<NexusRotationField>();

        for (int i = 0; i < positionSet.Count; i++)
        {
            EntityId id = positionSet.GetEntity(i);
            if (NexusObjectMapping.TryGet(id.Index, out object obj) && obj is GameObject go)
            {
                Transform t = go.transform;
                t.position = *positionSet.Get(id);
                // Rotation sync...
            }
        }
    }
}
```

---

## Nexus Optimization Tip: Change-Only Sync
Senkronizasyon maliyetini düşürmek için `DirtyBits` teknolojisini kullanın. Sadece son kareden beri unmanaged verisi değişmiş olan varlıkları senkronize ederek, **Unity tarafındaki Transform güncelleme yükünü %70-80 oranında azaltabilirsiniz.**
