# API Referansı: NexusAnalyzer (Statik Güvenlik Analizcisi)

## Giriş
`NexusAnalyzer.cs`, Nexus Prime geliştirme sürecinin "güvenlik görevlisidir". Roslyn tabanlı bu araç, kodun sadece doğru yazılmasını değil, aynı zamanda performanslı çalışmasını sağlayacak kurallara uymasını da denetler. Özellikle ECS bileşenlerinin `unmanaged` (yönetilmeyen) yapıda olması gibi kritik kuralları derleme (compile) aşamasında kontrol ederek, çalışma zamanında oluşabilecek bellek hatalarını ve GC gecikmelerini daha oluşmadan engeller.

---

## Teknik Analiz
Analizci şu kuralları ve teknikleri denetler:
- **Unmanaged Constraint (NX001)**: ECS bileşeni olarak kullanılan tüm struct'ların `unmanaged` olup olmadığını kontrol eder. Eğer struct içinde `string` veya `class` gibi managed bir veri bulunursa hata (Error) fırlatır.
- **Diagnostic Descriptor**: Hataları standart .NET `DiagnosticId` (Örn: NX001) formatında sunarak IDE (Visual Studio/Rider) entegrasyonu sağlar.
- **Concurrent Execution**: Analiz işlemini tüm işlemci çekirdeklerini kullanarak gerçekleştirir, böylece büyük projelerde IDE hızını yavaşlatmaz.
- **In-IDE Guidance**: Geliştiriciye hatanın nedenini ve nasıl çözüleceğini (Örn: "Struct must be unmanaged") anlık olarak açıklar.

---

## Mantıksal Akış
1. **Tetiklenme**: Geliştirici IDE'de bir dosya kaydettiğinde veya proje derlendiğinde analizci devreye girer.
2. **Semantik Analiz**: Kodun sözdizimi (syntax) ötesinde, veri tiplerinin gerçek doğası (unmanaged olup olmadığı) incelenir.
3. **Kural Kontrolü**: Belirlenen semboller (structlar) `Rule` listesindeki kriterlere göre test edilir.
4. **Raporlama**: Eğer bir ihlal varsa, kodun tam satırında kırmızı alt çizgi ve açıklayıcı metin belirir.

---

## Terminoloji Sözlüğü
- **Diagnostic Analyzer**: Kod üzerindeki hataları veya iyileştirme fırsatlarını tespit eden derleyici eklentisi.
- **Unmanaged Type**: Bellek yönetimi Garbage Collector tarafından yapılmayan, ham bellek üzerinde saklanan veri tipi.
- **Severity (DiagnosticSeverity)**: Bir analiz bulgusunun önem derecesi (Info, Warning, Error).
- **Semantics**: Kodun anlamı ve tip hiyerarşisi (sadece yazım kuralları değil).

---

## Riskler ve Sınırlar
- **False Positives**: Analizci bazen ECS bileşeni olmayan struct'ları da ECS bileşeni sanabilir. Bu durumları önlemek için namespace veya attribute filtreleri hassas ayarlanmalıdır.

---

## Kullanım Örneği
```csharp
// Hatalı Kod
public struct PlayerData {
    public string Name; // HATA: NX001 - Managed string kullanılamaz.
}

// Doğru Kod
public struct PlayerData {
    public NexusString32 Name; // DOĞRU: Unmanaged text.
}
```

---

## Nexus Optimization Tip: Zero-GC Policy
NexusAnalyzer hatalarını ASLA bastırmayın (ignore). Bu hatalar, **projenizde Garbage Collector'ın çalışmasını %100 engelleyerek oyununuzun akıcılığını (Fluidity) koruyan en büyük güvencenizdir.**

---

## Orijinal Kod
[NexusAnalyzer.cs Kaynak Kodu](file:///home/gokhanc/Development/Nexus/NexusGenerator/NexusAnalyzer.cs)
