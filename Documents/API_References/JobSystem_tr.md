# Nexus Prime Mimari Rehberi: JobSystem (İş Parçacıığı Orkestratörü)

## 1. Giriş
`JobSystem.cs`, Nexus Prime'ın "Çok Çekirdekli Motoru"dur (Multi-threaded Engine). Modern işlemciler 8, 16 veya daha fazla çekirdeğe sahip olsa da, bu çekirdeklerin verimli kullanımı "Veri Bağımlılığı" (Data Dependency) nedeniyle zordur. Aynı veriyi hem okuyan hem yazan iki sistem aynı anda çalışırsa, veri bozulması (race condition) kaçınılmazdır.

JobSystem'ın varlık sebebi, sistemler arasındaki bağımlılıkları analiz ederek onları güvenli **Katmanlara (Layers)** ayırmak ve her katman içindeki sistemleri işlemcinin tüm çekirdeklerinde paralel olarak koşturmaktır.

---

## 2. Teknik Analiz
JobSystem, paralel yürütme güvenliği için şu ileri teknikleri kullanır:

- **Dependency Graph Analysis (Kahn's Algorithm)**: Sistemler arasındaki Okuma/Yazma (Read/Write) çakışmalarını analiz eder. Eğer Sistem A, Pozisyon yazıyorsa ve Sistem B, Pozisyon okuyorsa; Sistem B ancak Sistem A bittikten sonra çalışabilir.
- **Layered Scheduling**: Birbirine bağımlılığı olmayan sistemler aynı katmana atanır. Bu katmanlar `Parallel.ForEach` ile eşzamanlı olarak yürütülür.
- **Concurrent Execution Monitoring**: `ConcurrentDictionary` kullanarak her sistemin çalışma süresini (metric) kaydeder.
- **Reflection-Based Dependency Extraction**: Sistemlerin üzerindeki `[Read]` ve `[Write]` özniteliklerini tarayarak çalışma zamanında otomatik bağımlılık haritası çıkarır.

---

## 3. Mantıksal Akış
1.  **Kayıt (`AddSystem`)**: Yeni bir sistem eklendiğinde, sistemin kullandığı bileşen tipleri (Reads/Writes) tespit edilir.
2.  **Katman İnşası (`RebuildLayers`)**: Kahn algoritması varyantı ile sistemler gruplanır.
    - Katman 1: Hiçbir bağımlılığı olmayan sistemler.
    - Katman 2: Sadece Katman 1'deki verilere bağımlı olanlar.
3.  **Yürütme (`Execute`)**: Katmanlar sırasıyla (Sequential) çalıştırılır. Her katman içindeki her bir sistem ise paralel (Parallel) olarak koşturulur.
4.  **Metrik Analizi**: Her sistemin kaç milisaniye harcadığı profiller için kaydedilir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Race Condition** | İki veya daha fazla iş parçacığının aynı veriye aynı anda erişmesiyle oluşan hata. |
| **Dependency Graph** | İşlerin birbirine olan bağımlılığını gösteren matematiksel düğüm yapısı. |
| **Kahn's Algorithm** | Bir grafikteki düğümleri doğrusal ve katmanlı bir sıraya dizme algoritması. |
| **Throughput** | Birim zamanda yapılan iş miktarı (İşlem kapasitesi). |

---

## 5. Riskler ve Sınırlar
- **Circular Dependency**: Eğer Sistem A, Sistem B'ye; Sistem B de Sistem A'ya bağımlıysa grafik kilitlenir. Nexus bu durumu tespit edip fallback mekanizmasıyla çözer.
- **Parallel Overhead**: Çok küçük işler için paralel thread oluşturmak, işin kendisinden daha uzun sürebilir. Bu durum "Grain Size" optimizasyonu gerektirir.

---

## 6. Kullanım Örneği
```csharp
var jobSystem = new JobSystem(registry);

// Sistemleri ekle (Bağımlılıklar otomatik çözülür)
jobSystem.AddSystem(new MovementSystem());
jobSystem.AddSystem(new CollisionSystem());

// Her karede çalıştır
void Update() {
    jobSystem.Execute();
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Collections.Concurrent;
using System.Threading.Tasks;
namespace Nexus.Core;

public class JobSystem : INexusJobSystem
{
    private readonly List<SystemNode> _nodes = new();
    private readonly List<List<SystemNode>> _layers = new();

    public void RebuildLayers()
    {
        _layers.Clear();
        // 1. Build Adjacency List based on Read/Write conflicts
        // 2. Group into layers using Kahn's algorithm
    }

    public void Execute()
    {
        foreach (var layer in _layers) {
            Parallel.ForEach(layer, node => {
                node.System.Execute();
            });
        }
    }
}
```

---

## Nexus Optimization Tip: Multi-Core Scalability
Standart bir `MonoBehaviour.Update()` döngüsü her zaman tek bir çekirdek (Main Thread) üzerine binerken; `Nexus JobSystem` iş yükünü **tüm çekirdeklere (8, 16, 32 vb.) homojen olarak dağıtır.** Bu, oyun mantığı kapasitenizi çekirdek sayınızla doğru orantılı olarak **4-8 kat artırmanızı** sağlar.
