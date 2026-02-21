# Nexus Prime Mimari Rehber: Development Cookbook (Geliştirici Yemek Kitabı)

## 1. Giriş: ECS Düşünce Yapısı
Nexus Prime ile uygulama geliştirirken "Nesne" (Object) odaklı değil, "Veri" (Data) odaklı düşünmelisiniz. Bir varlık (Entity) bir nesne değil, sadece bileşenlerin (Components) takıldığı bir kancadır.

Bu Cookbook, sisteminizi kurarken en sık karşılaşacağınız senaryolar için "Deep-Depth" standartlarında hazır çözümler sunar.

---

## 2. Tarif 1: Yüksek Yoğunluklu Hareket Sistemi
10,000 mermiyi veya 5,000 birimi aynı anda hareket ettirmek için en verimli yöntem.

### Veri Yapısı
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct Velocity : unmanaged { public float3 Value; }

[StructLayout(LayoutKind.Sequential)]
public struct Position : unmanaged { public float3 Value; }
```

### Sistem Mantığı
```csharp
public class MovementSystem : NexusParallelSystem
{
    [Read] private float _deltaTime;
    
    public override void Execute()
    {
        // Query ile filtrele ve paralel işle
        var query = registry.Query<Position, Velocity>();
        query.Execute((EntityId id, Position* pos, Velocity* vel) => {
            pos->Value += vel->Value * _deltaTime;
        });
    }
}
```

---

## 3. Tarif 2: State Snapshot & Rewind (Geri Sarma)
Oyunun durumunu kaydedip 5 saniye öncesine dönmek için.

```csharp
// 1. Snapshot Manager'ı başlat
var snapshotMgr = new SnapshotManager();

// 2. Her karede (veya belirli aralıkla) kaydet
void Update() {
    snapshotMgr.RecordFrame(registry, deltaOnly: true);
}

// 3. Geri sarma tetiklendiğinde
void OnRewindRequested() {
    var pastFrame = snapshotMgr.History.First.Value;
    snapshotMgr.LoadSnapshot(registry, pastFrame);
}
```

---

## 4. Tarif 3: Unity GameObject Senkronizasyonu
Nexus içindeki fiziksel veriyi Unity Gfx (Görsel) katmanına aktarma.

```csharp
// BridgeHub kaydı
bridgeHub.Register<Position>(
    push: (id, nexusPos) => {
        // Nexus -> Unity
        var transform = GetUnityTransform(id);
        transform.position = nexusPos->Value;
    },
    pull: (id, nexusPos) => {
        // Unity -> Nexus (Eğer Unity'den geliyorsa)
        var transform = GetUnityTransform(id);
        nexusPos->Value = transform.position;
    }
);
```

---

## 5. İleri Seviye İpucu: Memory Pooling
Bileşen ekleyip çıkarırken oluşacak maliyetleri önlemek için `EntityCommandBuffer` kullanın:

```csharp
public void OnProjectileHit(EntityId projectile, EntityId target) {
    // Hemen silme, kuyruğa al!
    ecb.DestroyEntity(projectile);
    ecb.AddComponent(target, new DamageEffect { Amount = 50 });
}
```

---

## 6. Sıkça Sorulan Sorular (Best Practices)
- **Q: Ne zaman Query kullanmalıyım?**
    - A: Bir grup bileşene sahip tüm varlıkları toplu işlemek istediğinizde.
- **Q: Ne zaman Direct Access (registry.Get) kullanmalıyım?**
    - A: Tekil bir varlığın verisine özel bir durumda (O(1) hızında) ulaşmak istediğinizde.
- **Q: String kullanabilir miyim?**
    - A: Sadece `NexusString32/64/128` gibi unmanaged alternatifleri kullanın.

---

**Nexus Prime Mühendislik Notu**: 
ECS dünyasında "Erken Optimizasyon" bir hata değil, bir zorunluluktur. Bu Cookbook'taki kalıpları takip ederek, projenizin temellerini 100 milyon saat döngüsünü (cycle) boşa harcamayacak şekilde inşa edebilirsiniz.
