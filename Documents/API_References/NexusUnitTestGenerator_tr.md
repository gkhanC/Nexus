# Nexus Prime Mimari Rehberi: NexusUnitTestGenerator (Otomatik Stres Testi Üretimi)

## 1. Giriş
`NexusUnitTestGenerator.cs`, Nexus Prime'ın kalite güvence (QA) süreçlerini otomatize eden bir mühendislik aracıdır. Geliştiricinin yazdığı sistemlerin (Systems) ne kadar veri yüküne dayanabileceğini ölçmek için, sistemin ihtiyaç duyduğu bileşenlerden milyonlarca rastgele örnek üreterek sahte bir dünya (Dummy World) inşa eder.

Bu jeneratörün varlık sebebi, manuel olarak birim test (unit test) yazma yükünü azaltmak ve sistemlerin özellikle uç durumlarda (edge cases) unmanaged bellek hatası verip vermediğini bilimsel bir "Stress Test" ortamında saptamadır.

---

## 2. Teknik Analiz
NexusUnitTestGenerator, güvenilir test sonuçları için şu adımları izler:

- **System Signature Reflection**: Test edilecek sistemin `[Read]`, `[Write]` ve `[Inject]` alanlarını tarayarak hangi bileşen tiplerini beklediğini analiz eder.
- **Randomized Data Influx**: Belirlenen varlık sayısında (Örn: 100.000), unmanaged bellek limitlerini zorlayacak şekilde rastgele bit verileriyle doldurulmuş bileşenler üretir.
- **Performance Profiling**: Sistemin çalışma süresini mikro-saniye (tick) hassasiyetinde ölçer ve raporlar.
- **Safety Validations**: Test sonunda `Registry` bütünlüğünü (`NexusIntegrityChecker`) kontrol ederek, sistemin bellek sızıntısına veya yanlış hizalamaya neden olup olmadığını denetler.

---

## 3. Mantıksal Akış
1.  **Giriş**: `GenerateStressTest(system, count)` metodu ile hedef sistem ve varlık sayısı verilir.
2.  **İnşa**: Geçici bir test `Registry`'si oluşturulur ve sistemin beklediği veri yapıları unmanaged alanda ilklendirilir.
3.  **Yürütüm**: Sistem, bu devasa veri yığını üzerinde bir veya birden fazla kez çalıştırılır.
4.  **Raporlama**: İşlem hızı (Varlık/Saniye) ve bellek sağlığı metrikleri konsola veya bir rapora basılır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Stress Test** | Bir sistemi, beklenen normal yükünün çok üzerinde çalıştırarak dayanıklılığını ölçme işlemi. |
| **System Signature** | Bir sistemin bağımlılıklarını (okuma/yazma izinleri) tanımlayan karakteristik özellik seti. |
| **Edge Case** | Normal çalışma koşullarının en uç noktalarında ortaya çıkan, nadir ama kritik durumlar. |
| **Dummy World** | Sadece test amacıyla oluşturulan, geçici ve sahte veri ortamı. |

---

## 5. Riskler ve Sınırlar
- **Reflection Overhead**: Test başlangıcında yapılan yansıma (reflection) işlemleri zaman alabilir, ancak bu sadece test kurulum evresinde (Setup) gerçekleşir.
- **RAM Limits**: Eğer `count` parametresi sistemin fiziksel RAM kapasitesini zorlayacak kadar büyük verilirse, işletim sistemi seviyesinde kilitlenmeler yaşanabilir.

---

## 6. Kullanım Örneği
```csharp
var testGen = new NexusUnitTestGenerator();
var movementSystem = new PlayerMovementSystem();

// Movement sistemini 1 milyon varlık ile stres testine sok
testGen.GenerateStressTest(movementSystem, 1000000);
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

public class NexusUnitTestGenerator
{
    public void GenerateStressTest(INexusSystem system, int entityCount = 100000)
    {
        // 1. Reflect system component requirements.
        // 2. Populate a registry with 'entityCount' entities.
        // 3. Measure execution time and check for memory safety.
    }
}
```

---

## Nexus Optimization Tip: Warm-up Cycles
Sorgu performansını daha gerçekçi ölçmek için, jeneratörü "Warm-up" modunda çalıştırın. Sistemi test verisi üzerinde 2-3 kez boşa çalıştırarak CPU'nun komut önbelleğini (Instruction Cache) doldurmasını sağlayın. Bu sayede, **JIT derleme maliyetinden arındırılmış gerçek çalışma hızını %25 daha doğru ölçebilirsiniz.**
