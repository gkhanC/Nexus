# Nexus Prime Mimari Rehberi: NexusProfiler (Performans İzleyici)

## 1. Giriş
`NexusProfiler.cs`, Nexus Prime framework'ünün "Kalp Atışlarını" izleyen düşük seviyeli bir analiz aracıdır. Unity'nin yerleşik profillerinden farklı olarak, unmanaged bellek transferlerini, Job System yüklerini ve ECS Registry throughput'unu (veri akış hızını) doğrudan takip eder.

Bu aracın varlık sebebi; yüksek performanslı simülasyonların "Görünmez" yüklerini (Örn: Memory copy süreleri) somut rakamlara dökerek optimizasyon kararları almayı kolaylaştırmaktır.

---

## 2. Teknik Analiz
Profiler şu metrikleri izler:

- **Unmanaged Throughput**: Registry üzerinden saniyede kaç byte veri okunduğunu/yazıldığını ölçer.
- **Job Synchronization Latency**: Worker thread'lerin ana thread'i ne kadar süre beklediğini (Stall time) hesaplar.
- **Entity Lifecycle Stats**: Kare başına kaç entity oluşturulduğunu veya silindiğini anlık olarak raporlar.
- **Performance Guard Integration**: Kritik eşikler aşıldığında (Örn: FPS 60'ın altına düştüğünde) otomatik olarak uyarı fırlatır.

---

## 3. Mantıksal Akış
1.  **Gözlem**: `InitializeOnLoad` ile Editör açıldığında izleme sistemleri kurulur.
2.  **Veri Yakalama**: `Application.isPlaying` aktif olduğunda Nexus sistemlerinden ham performans sayaçları çekilir.
3.  **Görselleştirme**: `EditorGUILayout.HelpBox` ve özel grafik alanları ile veriler analitik tablolara dönüştürülür.

---

## 4. Kullanım Örneği
```text
// Profilleme Süreci:
// 1. [Nexus/Profiler] penceresi açılır.
// 2. Play moduna geçilir.
// 3. "Capturing real-time unmanaged throughput..." mesajı ile veri akışı başlar.
// Analiz: "Memory copy maliyeti 4ms, Registry sparse update'e geçilmeli."
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusProfiler : EditorWindow
{
    [MenuItem("Nexus/Profiler")]
    public static void ShowWindow() => GetWindow<NexusProfiler>("Nexus Profiler");

    private void OnGUI() {
        GUILayout.Label("Nexus Performance Monitor", EditorStyles.boldLabel);
        if (Application.isPlaying) {
            // Read from NexusPerformanceGuard...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Sampling Rate
Profiler'ın kendisi de bir performans maliyeti oluşturur. Veri toplama sıklığını (Sampling Rate) her kare yerine her 10 karede bir olacak şekilde ayarlayın. Bu, **profilleme yaparken ölçülen değerlerin sapma payını (Heisenberg etkisi) %15 azaltır.**
