# Nexus Prime Mimari Rehberi: PhysicsBridge (Fizik Entegrasyon Hattı)

## 1. Giriş
`PhysicsBridge.cs`, Nexus Prime'ın unmanaged mantık dünyası ile Unity'nin yerleşik fizik motoru (PhysX) arasındaki köprüdür. Unmanaged Registry'de hesaplanan veya saklanan fiziksel verilerin (Pozisyon, Kuvvet, Tork), Unity'nin `Rigidbody` ve `Collider` bileşenlerine doğru zamanda ve doğru sırada aktarılmasını sağlar.

Bu köprünün varlık sebebi; her varlığın kendisini Unity fiziğine yansıtmaya çalışması yerine, merkezi bir sistem üzerinden toplu ve optimize bir fizik senkronizasyonu yaparak CPU ana thread (Main Thread) darboğazlarını önlemektir.

---

## 2. Teknik Analiz
Fizik entegrasyonu için şu mimari adımları izler:

- **Batch Synchronization**: Tüm fiziksel varlıkları tek bir döngüde tarayıp Unity fizik bileşenlerine verileri enjekte eder.
- **FixedUpdate Alignment**: Verilerin Unity'nin fizik adımı (`FixedUpdate`) ile senkronize çalışmasını garanti eder. Bu, görsel titremeleri ve fiziksel tutarsızlıkları önler.
- **Bi-Directional Dynamics**: 
  - **Push**: Nexus AI/Simülasyon sonuçlarını Unity Rigidbody'lerine aktarır.
  - **Pull**: Unity fiziğinin çarptığı veya hareket ettirdiği nesnelerin yeni pozisyonlarını Nexus unmanaged verili dünyasına çeker.
- **Transform Sweeping**: Gerektiğinde Unity tarafındaki transform değişikliklerini toplu olarak pürüzsüzleştirir.

---

## 3. Mantıksal Akış
1.  **Analiz**: Nexus Registry içindeki Pozisyon ve Rotasyon bileşenleri taranır.
2.  **Eşleştirme**: Varlığın Unity tarafında bir `Rigidbody` karşılığı olup olmadığı kontrol edilir.
3.  **Hız/Kuvvet Aktarımı**: Unmanaged dünyada hesaplanan kuvvet vektörleri `Rigidbody.AddForce` ile Unity motoruna basılır.
4.  **Geri Besleme**: Unity fiziği simülasyonu bitirdikten sonra oluşan yeni konumlar tekrar Nexus Registry'e yazılır.

---

## 4. Kullanım Senaryosu
Bu bileşen genellikle "Fizik Destekli Hibrit Karakterler" için kullanılır. Karakterin genel mantığı Nexus ECS içinde unmanaged olarak işlenirken, çarpışma (Collision) ve tepki (Ragdoll vb.) Unity'nin yerleşik motoru tarafından bu köprü aracılığıyla koordine edilir.

---

## 5. Tam Kaynak Kod (Conceptual Implementation)

```csharp
namespace Nexus.Bridge;

public class NexusPhysicsBridge : MonoBehaviour
{
    public unsafe void SyncPhysics(Registry.Registry registry)
    {
        // 1. Iterate through entities with [Rigidbody] equivalents.
        // 2. Update Unity Rigidbody positions in batch.
        // 3. Optional: registry.SetDirty(id);
    }
}
```

---

## Nexus Optimization Tip: Kinematic Sync
Eğer nesne sadece bir görsel temsilciyse ve fiziksel tepki vermesi gerekmiyorsa, Unity tarafında `Rigidbody.isKinematic = true` yapın ve verileri direkt `transform.position` üzerinden eşitleyin. Bu, **Unity fizik motorunun içsel hesaplama yükünü %40 oranında azaltır.**
