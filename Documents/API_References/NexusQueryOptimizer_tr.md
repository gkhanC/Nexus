# Nexus Prime Mimari Rehberi: NexusQueryOptimizer (Akıllı Sorgu Optimizasyonu)

## 1. Giriş
`NexusQueryOptimizer.cs`, Nexus Prime'ın sorgu yürütme stratejilerini çalışma zamanında belirleyen bir "Heuristik" (Sezgisel) karar mekanizmasıdır. Her işlemin mutlaka paralel çalışması gerektiği yanılgısını yıkarak, veri büyüklüğüne göre en doğru işlemci stratejisini seçer.

Bu optimize edicinin varlık sebebi, küçük veri setlerinde (Örn: 100 varlık) paralel iş parçacığı (Thread Pool) oluşturmanın getirdiği maliyetin (Overhead), işlemin kendisinden daha uzun sürmesi sorununu çözmektir.

---

## 2. Teknik Analiz
NexusQueryOptimizer, verimlilik için şu stratejik kararları uygular:

- **Threshold-Based Execution**: Varlık sayısı belirli bir eşiğin (Örn: 1000) altındaysa, işlemciyi yeni bir thread oluşturmaya zorlamaz; ana thread üzerinde sıralı (sequential) çalışır.
- **Load Balancing (Parallel.For)**: Veri seti büyükse, iş yükünü otomatik olarak tüm çekirdeklere dağıtır.
- **Zero-Allocation Logic**: Karar mekanizması tamamen statiktir ve çalışma zamanında ek bellek (GC) maliyeti oluşturmaz.
- **Smart Dispatch**: İşlemcinin çekirdek sayısını ve mevcut yükünü dikkate alarak "En Hızlı Yol" (Fast Path) seçimini yapar.

---

## 3. Mantıksal Akış
1.  **Girdi**: İşlenecek toplam varlık sayısı (`count`) ve yapılacak aksiyon (`action`) alınır.
2.  **Karar**: Eğer `count < 1000` ise, basit bir `for` döngüsü tetiklenir.
3.  **Paralelleştirme**: Eşik aşılmışsa, `Parallel.For` ile .NET çekirdek işleyicisi (Task Scheduler) devreye sokulur.
4.  **Tamamlama**: İşlem bittiğinde kontrol ana akışa geri döner.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Heuristic Decision** | Kesin kurallardan ziyade, genel deneyim ve ölçümlere (sezgilere) dayalı karar verme. |
| **Overhead** | Bir işlemin kendisinden ziyade, o işlemi başlatmak için gereken hazırlık maliyeti. |
| **Parallel.For** | .NET'in bir döngüyü otomatik olarak birden fazla işlemci çekirdeğine bölen aracı. |
| **Sequential** | Verilerin tek bir sıra halinde, birbirini bekleyerek işlenmesi. |

---

## 5. Riskler ve Sınırlar
- **Hardcoded Threshold**: 1000 varlık eşiği her donanımda (Mobil vs. PC) farklı sonuçlar verebilir. Modern sistemlerde bu eşik dinamik olarak ölçülmelidir (Auto-Calibration).
- **Callback Cost**: Aksiyon olarak gönderilen delege (Action), döngü içinde binlerce kez çağrıldığı için delege çağrı maliyetini beraberinde getirir.

---

## 6. Kullanım Örneği
```csharp
int entityCount = registry.EntityCount;

// Nexus, varlık sayısına göre for veya Parallel.For arasında karar verir
NexusQueryOptimizer.ExecuteSmartQuery(entityCount, i => {
    // Varlık üzerindeki işlem
    var id = registry.GetEntityByIndex(i);
    // ...
});
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Threading.Tasks;
namespace Nexus.Core;

public static class NexusQueryOptimizer
{
    public static void ExecuteSmartQuery(int count, Action<int> action)
    {
        if (count < 1000)
        {
            // Small batch: Single thread is faster
            for (int i = 0; i < count; i++) action(i);
        }
        else
        {
            // Large batch: Dispatch to all cores
            Parallel.For(0, count, action);
        }
    }
}
```

---

## Nexus Optimization Tip: Threshold Calibration
Benchmark'lar gösteriyor ki, mobil cihazlarda `Parallel.For` maliyeti daha yüksektir. Bu yüzden mobil platformlarda eşik değerini **2500-3000** civarına çekerek, küçük nesne gruplarında gereksiz thread geçişlerini (Context Switch) engelleyebilir ve **pil ömrünü iyileştirebilirsiniz.**
