# API Referansı: NexusSystemGenerator (Otomatik Kod Jeneratörü)

## Giriş
`NexusSystemGenerator.cs`, Nexus Prime'ın "otomasyon motorudur". Roslyn (C# Compiler SDK) kullanarak, geliştiricinin yazdığı ham sistem kodlarını analiz eder ve performans kritik olan "boilerplate" (rutin) kodları çalışma zamanı öncesinde otomatik üretir. Bu sayede geliştirici, karmaşık SIMD döngüleri veya pointer yönetimiyle uğraşmak yerine sadece oyun mantığına odaklanabilir.

---

## Teknik Analiz
Jeneratör şu ileri seviye teknikleri uygular:
- **IIncrementalGenerator Integration**: Kod değiştikçe sadece etkilenen kısımları yeniden üreterek derleme (compile) sürelerini minimize eder.
- **SIMD Loop Injection**: `[Read]` veya `[Write]` öznitelikleriyle işaretlenmiş alanlar için otomatik AVX-optimize döngüler (Run metodu) kurgular.
- **Partial Class Extension**: Geliştiricinin yazdığı sınıfları `partial` anahtar kelimesiyle genişleterek orijinal koda müdahale etmeden yeni yetenekler ekler.
- **Syntax Provider Filtering**: Sadece `INexusSystem` arayüzünü uygulanan sınıfları işleyerek gereksiz işlem yükünü önler.

---

## Mantıksal Akış
1. **İzleme**: Roslyn, projedeki tüm sınıfları tarayarak `INexusSystem` olanları jeneratöre raporlar.
2. **Analiz**: Belirlenen sınıfların içindeki bileşen alanları ve öznitelikleri (`[Read]`, `[Write]`) incelenir.
3. **Üretim**: Bellek adreslerini alan, SIMD bloklarını kuran ve güvenli geri dönüş (fallback) döngülerini içeren `[ClassName]_Generated.g.cs` dosyası oluşturulur.
4. **Entegrasyon**: Üretilen kod, projenin derleme sürecine (build pipeline) dahil edilerek binary pakete eklenir.

---

## Terminoloji Sözlüğü
- **Source Generator**: Derleme sırasında kaynak kod üreten ve projeye dahil eden C# derleyici özelliği.
- **Roslyn**: .NET platformu için geliştirilen açık kaynaklı C# ve Visual Basic derleyici seti.
- **Partial Class**: Bir sınıfın tanımının birden fazla dosyaya bölünmesine izin veren yapı.
- **Compile-time Automation**: Yazılımın çalışma zamanında değil, derleme aşamasında otomatikleştirilmesi.

---

## Riskler ve Sınırlar
- **Syntax Errors**: Eğer jeneratör hatalı C# kodu üretirse, tüm projenin derlenmesi durabilir. Üretilen kodun `unsafe` ve `simd` kurallarına %100 uyması gerekir.

---

## Kullanım Örneği
```csharp
public partial class MovementSystem : INexusSystem {
    [Read] Position* pos;
    [Write] Velocity* vel;
    // Nexus, bu alanlar için Run() metodunu otomatik üretir.
}
```

---

## Nexus Optimization Tip: Explicit Attributes
Sistem alanlarını her zaman `[Read]` veya `[Write]` ile işaretleyin. Jeneratör, **yazma (Write) gerektirmeyen alanları sadece okuyarak işlemci önbelleğini (cache) daha verimli kullanır.**

---

## Orijinal Kod
[NexusSystemGenerator.cs Kaynak Kodu](https://github.com/gkhanC/Nexus/blob/master/NexusGenerator/NexusSystemGenerator.cs)
