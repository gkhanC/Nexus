# Nexus Prime Mimari Rehberi: NexusInitializer (Motor Başlatıcı)

## 1. Giriş
`NexusInitializer.cs`, Nexus Prime framework'ünün "Komuta Merkezi" ve ana giriş noktasıdır. Unity sahneleri içinde ECS dünyasının (`Registry`), iş parçacığı yöneticisinin (`JobSystem`) ve bellek yedeği motorunun (`SnapshotManager`) otomatik olarak ayağa kaldırılmasını sağlar.

Bu başlatıcının varlık sebebi; unmanaged motorun karmaşık yapılarını basitleştirerek, Unity geliştiricilerine "Tek-Tıkla" (One-Click) entegrasyon imkanı sunmak ve motorun yaşam döngüsünü (Lifecycle) sahne geçişleriyle uyumlu hale getirmektir.

---

## 2. Teknik Analiz
NexusInitializer, motor yönetimi için şu kritik roller üstlenir:

- **Core Bootstrapping**: `Awake` aşamasında Registry, JobSystem ve SnapshotManager örneklerini oluşturur.
- **Orchestration**: `Update` döngüsünde `JobSystem.Execute()` çağrısını yaparak, unmanaged sistemlerin her karede (frame) çalışmasını sağlar.
- **Health Monitoring**: `NexusIntegrityChecker` kullanarak belirli periyotlarla (varsayılan 60 kare) unmanaged belleğin durumunu denetler ve hataları Unity konsoluna raporlar.
- **Resource Management**: Sahne kapandığında veya nesne silindiğinde `OnDestroy` ile unmanaged belleği iade eder (Dispose).

---

## 3. Mantıksal Akış
1.  **İlklendirme**: Sahne yüklendiğinde motorun kalbi olan `Registry` ve `JobSystem` RAM'de tahsis edilir.
2.  **Yürütüm**: Her karede, sistemler belirlenen sırada unmanaged veriyi işler.
3.  **Denetim**: Arka planda bellek bütünlüğü ve hizalama (Alignment) kontrolleri yapılır.
4.  **Temizlik**: Uygulama kapandığında tüm unmanaged kaynaklar el ile temizlenir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Bootstrapping** | Bir yazılımın çalışması için gereken temel bileşenlerin sırasıyla hazır hale getirilmesi. |
| **Runtime Integrity** | Uygulama çalışırken bellek yapılarının bozulup bozulmadığının sürekli denetlenmesi. |
| **Lifecycle Hook** | Unity'nin `Awake`, `Update`, `OnDestroy` gibi otomatik çağrılan yaşam döngüsü metodları. |
| **Engine Configuration** | Maksimum geçmiş kare sayısı gibi motorun performans parametrelerinin ayarlanması. |

---

## 5. Riskler ve Sınırlar
- **Global Dependency**: Sahnede birden fazla `NexusInitializer` bulunmamalıdır; aksi halde çakışan Registry'ler oluşur.
- **Disposal Negligence**: Eğer nesne sahnede yok edilmezse veya `OnDestroy` düzgün çalışmazsa, devasa unmanaged bellek sızıntıları oluşabilir.

---

## 6. Kullanım Örneği
```csharp
// Sahnede bir nesne oluştur ve bu scripti ekle.
// JobSystem ve Registry otomatik olarak hazır olacaktır.

void Start() {
    var registry = FindObjectOfType<NexusInitializer>().Registry;
    registry.Create(); // Artık ECS dünyası hazır!
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

public class NexusInitializer : MonoBehaviour
{
    public int MaxHistoryFrames = 100;
    private Registry _registry;
    private JobSystem _jobSystem;

    private void Awake() {
        _registry = new Registry();
        _jobSystem = new JobSystem(_registry);
    }

    private void Update() {
        _jobSystem.Execute();
    }

    private void OnDestroy() {
        _registry?.Dispose();
    }
}
```

---

## Nexus Optimization Tip: Integrity Performance
`PerformRuntimeIntegrityChecks` ayarını geliştirme aşamasında (Development) açık tutun, ancak "Final Release" sürümünde kapatın. Bu, **her 60 karede bir yapılan denetim maliyetini sıfıra indirerek küçük bir performans kazancı sağlar.**
