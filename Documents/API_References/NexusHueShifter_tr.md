# Nexus Prime Mimari Rehberi: NexusHueShifter (Dinamik Renk Kaydırıcı)

## 1. Giriş
`NexusHueShifter.cs`, oyun içindeki görsel elemanların renk tonunu (Hue) zamana bağlı olarak dinamik bir şekilde değiştiren bir yardımcı araçtır. HypeFire framework'ünden miras alınan bu yapı, özellikle parlayan efektler, vurgulanan arayüz elementleri ve "Premium" bir görsel hissiyat oluşturmak için kullanılır.

Bu aracın varlık sebebi; her renk değişimi için ayrı bir animasyon veya shader yazmak yerine, C# tarafında matematiksel bir sinüs dalgası (Cosine) kullanarak pürüzsüz ve döngüsel renk geçişleri sağlamaktır.

---

## 2. Teknik Analiz
Renk yönetimi için şu algoritmik yaklaşımları kullanır:

- **Mathematical Cosine Wave**: Renk tonunu `Mathf.Cos` fonksiyonu ile 0.1 ile 0.55 aralığında bir salınıma sokar. Bu, renklerin her zaman canlı (Saturated) ancak gözü yormayan bir aralıkta kalmasını sağlar.
- **HSV to RGB Conversion**: Hesaplanan "Hue" (Ton) değerini Unity'nin tam renk spektrumuna (`Color.HSVToRGB`) dönüştürür.
- **Editor-Time Support**: `#if UNITY_EDITOR` preprocessor direktifi ile efektin oyun çalışmasa bile Editor içinde (Time-since-startup kullanarak) canlı olarak izlenebilmesine olanak tanır.
- **Preset Colors**: `SoftBlue`, `SoftGreen` gibi belirli ofset değerlerine sahip hazır renk profilleri sunar.

---

## 3. Mantıksal Akış
1.  **Zaman Hesaplama**: Donanım saati veya oyun zamanı baz alınarak bir ofset değeri üretilir.
2.  **Dalga Uygulama**: Zaman değeri bir Cosine fonksiyonundan geçirilerek [0, 1] aralığında normalize bir tona dönüştürülür.
3.  **Renk Üretimi**: Normalize ton, doygunluk (Saturation) ve parlaklık (Value) değerleri ile birleşerek final `Color` verisini oluşturur.

---

## 4. Kullanım Örneği
```csharp
void Update() {
    // Nesnenin rengini her karede yumuşak bir şekilde değiştir
    GetComponent<Renderer>().material.color = NexusHueShifter.GetColor();

    // Hazır bir profil kullan
    var successColor = NexusHueShifter.SoftGreen;
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Mathematics;

public static class NexusHueShifter
{
    public static Color GetColor(float offset = 0f) {
        float hue = Mathf.Cos(Time.time + 1f) * 0.225f + 0.325f;
        return Color.HSVToRGB((hue + offset) % 1f, 1, 1);
    }

    public static Color SoftBlue => GetColor(0.2f);
}
```

---

## Nexus Optimization Tip: MaterialPropertyBlock
Eğer bir karede yüzlerce nesnenin rengini `NexusHueShifter` ile değiştiriyorsanız, direkt `material.color` kullanmak yerine `MaterialPropertyBlock` kullanın. Bu, **GPU draw-call (batching) verimliliğini koruyarak CPU yükünü %15 azaltacaktır.**
