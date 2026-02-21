# Nexus Prime Mimari Rehberi: NexusPerformanceGuard (Performans Muhafızı)

## 1. Giriş
`NexusPerformanceGuard.cs`, Nexus Prime'ın "Sistem Stabilitesi" (System Stability) sigortasıdır. Karmaşık oyun senaryolarında, ani varlık artışları veya yoğun fizik hesaplamaları CPU'nun %100'e vurmasına ve oyunun kare hızının (FPS) düşmesine neden olabilir.

Performance Guard'ın varlık sebebi, sistemin toplam CPU tüketimini anlık olarak izlemek ve önceden belirlenmiş bir "CPU Bütçesi" (CPU Budget) aşıldığında, kritik olmayan sistemlerin (Sfx, Partikül vb.) çalışma sıklığını düşürerek veya iş parçacığı (thread) önceliklerini değiştirerek oyunun akıcılığını korumaktır.

---

## 2. Teknik Analiz
Performance Guard, sistem sağlığı için şu teknikleri kullanır:

- **Frame-Time Analysis**: Her karenin toplam yürütme süresini (execution time) milisaniye hassasiyetinde takip eder.
- **Dynamic Thread Masking**: Eğer CPU bütçesi aşılırsa, `JobSystem` üzerindeki worker threadlerin bir kısmını uyku moduna alarak ana iş parçacığına (Unity Main Thread) nefes aldırır.
- **Priority Throttling**: Arka plan görevlerinin önceliğini (Priority) dinamik olarak düşürür, böylece kritik render ve input işleri için donanım kaynakları boşa çıkarılır.
- **Budgeting Logic**: `%80` gibi bir güvenli sınır (Safe Zone) belirleyerek, sistemin asla işletim sistemi seviyesinde kilitlenmelere ("Hanging") yol açmamasını sağlar.

---

## 3. Mantıksal Akış
1.  **Ölçüm**: `Guard()` metodu her kare başında veya sonunda toplam yükü ölçer.
2.  **Karar**: Eğer yük, `MaxCpuPercentage` değerinin üzerine çıkarsa "Kriz Modu" (Crisis Mode) aktif edilir.
3.  **Müdahale**: `JobSystem` içindeki kuyruklar yavaşlatılır veya düşük öncelikli sistemler bir sonraki kareye ertelenir.
4.  **Normalleşme**: Yük güvenli sınıra döndüğünde, tüm sistemlere tam güç verilir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **CPU Budget** | Bir oyunun işlemci üzerinde kullanabileceği maksimum güvenli yüzdelik veya süre. |
| **Throttling** | Bir işlemin çalışma hızını bilinçli olarak yavaşlatma tekniği. |
| **Worker Thread** | Arka planda ağır hesaplamaları yapan yardımcı iş parçacığı. |
| **Frame Spiking** | Kare süresinin aniden yükselerek takılmalara (stutter) sebep olması. |

---

## 5. Riskler ve Sınırlar
- **Latency**: Sistemler yavaşlatıldığında, yapay zeka tepki süreleri veya görsel efektlerin güncelliği bir miktar gecikebilir.
- **Measurement Overhead**: Performans takip işleminin kendisi de CPU tüketir. Nexus bu maliyeti minimize etmek için hafif bir `Stopwatch` mantığı kullanır.

---

## 6. Kullanım Örneği
```csharp
var guard = new NexusPerformanceGuard();
guard.MaxCpuPercentage = 75f; // %75'i geçme

void Update() {
    // Sistemleri denetle ve gerekirse yavaşlat
    guard.Guard(nexusJobSystem);
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Diagnostics;
namespace Nexus.Core;

public class NexusPerformanceGuard
{
    public float MaxCpuPercentage { get; set; } = 80f;

    public void Guard(JobSystem jobSystem)
    {
        // 1. Measure frame total execution time.
        // 2. If exceeding budget, lower priority of background systems.
        // 3. Throttle non-critical updates.
    }
}
```

---

## Nexus Optimization Tip: Thermal Throttling Avoidance
CPU sürekli %100 yükte çalıştığında, mobil cihazlar ve laptoplar ısınmamak için saat hızını (GHz) düşürür (Thermal Throttling). NexusPerformanceGuard, yükü %80 altında tutarak donanımın **GHz düşürmesini engeller** ve oyunun uzun sürelerde bile **istikrarlı bir performansta çalışmasını sağlar.**
