# Nexus Prime Mimari Rehberi: BitPackedAttribute (Bit Seviyesinde Sıkıştırma)

## 1. Giriş
`BitPackedAttribute.cs`, Nexus Prime'ın veri yoğunluğunu (data density) artırmak ve ağ üzerindeki veri trafiğini minimize etmek için kullandığı bir "Sıkıştırma İşareti" (Compression Flag) katmanıdır. Bir bileşenin tüm byte'larını göndermek yerine, sadece belirlenen bit sayısı kadar alanı kapsayacak şekilde paketlenmesi gerektiğini belirtir.

Bu özniteliğin varlık sebebi, özellikle Boolean değerler, Enum'lar veya küçük sayısal değerler (Örn: 0-15 arası bir ID) için koca bir `int` (32 bit) veya `byte` (8 bit) harcamak yerine, bellekten tasarruf ederek **bandwidth kullanımını %80'e kadar azaltmaktır.**

---

## 2. Teknik Analiz
BitPackedAttribute, paketleme araçları için şu direktifleri sunar:

- **Bit Length Specification**: `Bits` parametresi ile verinin kaç bit içinde temsil edilebileceğini (Örn: 4 bit = 0-15 değer aralığı) beyan eder.
- **Source Generator Hook**: Bu öznitelik, Nexus'un Bit-Level Compression Tool'u tarafından taranır ve ilgili bileşen için otomatik olarak `Sıkıştır/Aç` (Pack/Unpack) metodlarına sahip wrapper sınıflar üretilir.
- **Network Optimization**: Delta serileştirme sırasında, bu özniteliğe sahip alanlar ham değerleri yerine bit-dizisi (bit-stream) olarak iletilir.

---

## 3. Mantıksal Akış
1.  **Tanımlama**: Geliştirici, küçük değer aralığına sahip bir struct bileşenini `[BitPacked(4)]` gibi bir değerle işaretler.
2.  **Analiz**: Nexus'un derleme zamanı araçları bu işareti görür.
3.  **Üretim**: Yazılım, bu veriyi bit-seviyesinde maskeleyen (Masking) ve kaydıran (Shifting) unsafe kodları otomatik üretir.
4.  **Yürütüm**: Veri kaydedilirken veya gönderilirken sadece hedeflenen bitler işlenir.

---

## 4. Kullanım Örneği
```csharp
using Nexus.Data;

[BitPacked(3)] // Sadece 3 bit yer kaplar (0-7 arası değerler için)
public struct TeamType {
    public int Value; 
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Data;

[AttributeUsage(AttributeTargets.Struct)]
public class BitPackedAttribute : Attribute
{
    public int Bits { get; }

    public BitPackedAttribute(int bitsCount)
    {
        Bits = bitsCount;
    }
}
```

---

## Nexus Optimization Tip: Precision Squeezing
Eğer bir bileşen sadece "Aktif/Pasif" veya "Takım ID" gibi çok kısıtlı değerler alıyorsa, mutlaka `[BitPacked]` kullanın. Bu, işlemcinin daha fazla veriyi tek bir "Cache Line" içine sığdırmasını sağlayarak **bellek bant genişliği verimliliğini (Memory Bandwidth Efficiency) logaritmik olarak artırır.**
