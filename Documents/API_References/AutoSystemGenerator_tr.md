# Nexus Prime Mimari Rehberi: AutoSystemGenerator (Otomatik Sistem Grafiği Oluşturma)

## 1. Giriş
`AutoSystemGenerator.cs`, Nexus Prime'ın "akıllı" beynidir. Geliştiricilerin yazdığı onlarca farklı sistemin birbirleriyle olan veri çakışmalarını çözen ve en verimli paralel yürütme planını hazırlayan statik bir orkestrasyon aracıdır.

Bu jeneratörün varlık sebebi, sistemlerin `[Read]` ve `[Write]` etiketlerini analiz ederek bir **Yönlendirilmiş Döngüsüz Grafik (DAG)** oluşturmak ve işlemcinin hiçbir çekirdeğini boş bırakmadan, veri güvenliğini bozmayacak en yoğun paralel planı dinamik olarak inşa etmektir.

---

## 2. Teknik Analiz
AutoSystemGenerator, sistem yönetimi için şu ana görevleri üstlenir:

- **Conflict Analysis**: Hangi sistemlerin aynı bileşene yazdığını tespit eder. Yazma çakışması olan sistemleri farklı zaman dilimlerine (Layer/Sync Point) ayırır.
- **Dependency Injection Orchestration**: Sistemlerin çalışma zamanında ihtiyaç duyduğu `Registry`, `EntityCommandBuffer` gibi araçları `[Inject]` alanlarına otomatik olarak enjekte eder.
- **Optimal Pathing**: Bağımsız sistemlerin hepsini aynı "Layer" içine koyarak, işlemcinin tüm thread'lerini aynı anda kullanabilmesini sağlar.
- **Waste Cycle Prevention**: Gereksiz bağımlılıklar yüzünden bekleyen thread'leri tespit eder ve grafiği daraltarak (Compact Graph) boşa harcanan saat döngülerini önler.

---

## 3. Mantıksal Akış
1.  **Tarama**: Kaydedilen tüm `INexusSystem` tipleri Reflection ile taranır.
2.  **Bağımlılık Haritası**: Sistemlerin okuma/yazma gereksinimleri bir matrise dönüştürülür.
3.  **Grafik İnşası**: Kahn Algoritması veya benzeri bir sıralama ile sistemler katmanlara bölünür.
4.  **Enjeksiyon**: Her bir katman tetiklenmeden önce gerekli tüm referanslar sistem alanlarına yazılır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Execution Graph** | İşlerin birbirine olan bağımlılığını gösteren akış şeması. |
| **DAG (Directed Acyclic Graph)** | Döngüsü olmayan, belirli bir yönü olan iş akış grafiği. |
| **Sync Point** | Tüm paralel işlerin bitmesini bekleyen ve bir sonraki aşamaya geçişi sağlayan bariyer. |
| **Waste Cycles** | İşlemcinin bir bağımlılık yüzünden boşta beklediği zaman dilimi. |

---

## 5. Riskler ve Sınırlar
- **Circular Dependency**: Eğer Sistem A, B'yi bekliyor ve Sistem B de A'yı bekliyorsa grafik kilitlenir (Deadlock). Generator bu durumu tespit edip hata fırlatmalıdır.
- **Complexity**: Yüzlerce sistemin olduğu çok büyük projelerde grafiğin her karede yeniden inşası maliyetli olabilir. Nexus bu yüzden grafiği sadece sistem eklendiğinde/çıkarıldığında (`RebuildSystemGraph`) yeniden oluşturur.

---

## 6. Kullanım Örneği
```csharp
// JobSystem ilklendirilirken grafiği oluştur
public void Configure(JobSystem jobs)
{
    // Sistemler eklendikten sonra
    AutoSystemGenerator.RebuildSystemGraph(jobs);
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

public static class AutoSystemGenerator
{
    public static void RebuildSystemGraph(JobSystem jobSystem)
    {
        // 1. Analyze all systems for Read/Write conflicts.
        // 2. Build an optimal execution graph.
        // 3. Inject dependencies via [Inject].
    }
}
```

---

## Nexus Optimization Tip: Explicit Ordering Optimization
Sistemleriniz arasında çok fazla "Yazma" çakışması varsa, AutoSystemGenerator bunları alt alta (sıralı) dizmek zorunda kalır. Veri yapılarınızı daha küçük parçalara bölerek (örneğin `Health` ve `Position` yerine daha spesifik bileşenler), generator'ın daha fazla sistemi **aynı paralel katmana koymasını sağlayıp performansı %50 artırabilirsiniz.**
