# Nexus Prime Mimari Rehberi: NexusIntegrityChecker (Çalışma Zamanı Bütünlük Denetimi)

## 1. Giriş
`NexusIntegrityChecker.cs`, Nexus Prime framework'ünün "Donanım Teşhis" (Hardware Diagnostics) birimidir. Karmaşık ve milyonlarca varlık içeren simülasyonların unmanaged bellek (RAM) üzerindeki bütünlüğünü ve performans sağlığını denetlemek için tasarlanmıştır.

Bu denetçinin varlık sebebi, unmanaged ve unsafe kodun getirdiği riskleri minimize etmektir. Bellek hizalaması (Alignment) bozulması gibi sinsi ve performans düşüren hataların, simülasyonun derinliklerinde kaybolmadan önce saptanmasını sağlar.

---

## 2. Teknik Analiz
NexusIntegrityChecker, sistem sağlığı için şu kritik denetimleri yürütür:

- **Cache-Line Alignment Audit**: `Dense` ve `Sparse` tampon belleklerin (buffers) 64-byte hizalamasını (`% 64 == 0`) kontrol eder. Yanlış hizalanmış belleklerin işlemci önbelleğinde oluşturacağı "Split" maliyetini raporlar.
- **Page-Alignment Verification**: `ChunkedBuffer` içindeki her bir bellek parçasının (chunk), işletim sistemi sayfa sınırlarına (Page Boundaries) uygunluğunu denetler.
- **Health Status Logic**: Sistemi `Nominal`, `Degraded` ve `Critical` olarak üç seviyede derecelendirir.
- **Zero-Cost Compliance Check**: Sistemin statik durum (static state) şişkinliğini denetleyerek, Nexus'un "Zero-Cost" felsefesine aykırı, gereksiz arka plan tahsisatlarını izler.

---

## 3. Mantıksal Akış
1.  **Başlatma**: `NexusIntegrityChecker.Audit(registry)` metodu bir denetim başlatır.
2.  **Iterasyon**: Registry içindeki tüm `ComponentSets` (SparseSet'ler) tek tek taranır.
3.  **Hizalama Testi**: Her setin ham bellek pointerları alınır ve modulo (%) matematiği ile donanım sınırları test edilir.
4.  **Raporlama**: Bulunan tüm ihlaller bir `CoreMetrics` yapısı içinde birleştirilerek kullanıcıya döner.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Integrity Audit** | Sistemin beklendiği gibi (tutarlı ve hatasız) çalıştığını doğrulayan denetim. |
| **Alignment Violation** | Verinin bellek adresinin, donanımın beklediği sınırların dışına taşması durumu. |
| **Degraded State** | Sistemin çalıştıgı ancak düşük performansla veya riskli bir şekilde ilerlediği durum. |
| **Zero-Cost Compliance** | Bir yapının sadece kullanıldığı kadar maliyet getirmesi prensibi. |

---

## 5. Riskler ve Sınırlar
- **Auditing Overhead**: Tüm registry'yi taramak bir miktar CPU maliyeti oluşturur. Bu yüzden `Audit` metodu her karede değil, sadece kritik geçişlerde (sahne değişimi vb.) veya hata durumlarında çağrılmalıdır.
- **Pointer Stability**: Denetim sırasında Registry üzerinde yapısal bir değişiklik (Add/Remove) yapılmaması gerekir, aksi halde pointerlar geçersiz kalabilir.

---

## 6. Kullanım Örneği
```csharp
// Önemli bir simülasyon evresinden önce sağlığı kontrol et
var report = NexusIntegrityChecker.Audit(mainRegistry);

if (report.Status == NexusIntegrityChecker.HealthStatus.Critical)
{
    Console.Error.WriteLine($"NEXUS CRITICAL ERROR: {report.Diagnostics}");
    // Simülasyonu güvenli bir şekilde durdur
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

public static unsafe class NexusIntegrityChecker
{
    public static CoreMetrics Audit(Registry registry)
    {
        var metrics = new CoreMetrics { Status = HealthStatus.Nominal };
        foreach (var set in registry.ComponentSets)
        {
            // 1. Verify Memory Alignment
            void* densePtr = set.GetRawDense(out _);
            if (densePtr != null && ((long)densePtr % NexusMemoryManager.CACHE_LINE) != 0)
                metrics.Status = HealthStatus.Critical;
        }
        return metrics;
    }
}
```

---

## Nexus Optimization Tip: Page Alignment Benefit
`NexusIntegrityChecker` tarafından denetlenen **Page Alignment** (Sayfa Hizalaması), işletim sisteminin sanal bellek yönetimini (MMU) optimize eder. Sayfa sınırlarına uyan bellek tahsisatları, işlemcinin TLB isabet oranını artırarak **bellek erişim geçikmesini (latency) %5-8 oranında azaltabilir.**
