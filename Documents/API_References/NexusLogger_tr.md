# Nexus Prime Mimari Rehberi: NexusLogger (Zekice Günlükleme Sistemi)

## 1. Giriş
`NexusLogger.cs`, Nexus Prime framework'ünün "Kara Kutusu"dur. Sadece ekrana yazı basmakla kalmaz; unmanaged sistemlerden gelen verileri, fizik hatalarını ve ağ paketlerini thread-safe (iş parçacığı güvenli) bir şekilde toplar ve farklı kanallara (Konsol, Dosya, UI) dağıtır.

Bu log sisteminin varlık sebebi; Unity'nin standart `Debug.Log` sistemini daha güçlü, filtrelenebilir ve profesyonel bir hale getirmektir. Zengin metin (Rich Text) desteği ile logları görsel olarak ayrıştırır.

---

## 2. Teknik Analiz
Yüksek performanslı günlükleme için şu mimariyi sunar:

- **Multi-Sink Architecture**: `INexusLogSink` arayüzü sayesinde logları sadece konsola değil, aynı anda bir dosyaya, oyun içi bir debug paneline veya bir analytics servisine gönderebilir.
- **Thread-Safety**: `lock(_lock)` mekanizmasıyla donatılmıştır. Nexus'un paralel Job System'i içinden gelen logların birbirine karışmasını veya çökmesini (Race Condition) önler.
- **Visual Categorization (Rich Text)**: `LogLevel` (Success, Warning, Error vb.) değerine göre logları otomatik olarak renklendirir (`<b>[Nexus]</b>` ön ekiyle birlikte).
- **Unity Context Awareness**: Log basılırken geçilen `context` parametresi sayesinde, konsoldaki loga tıklandığında ilgili Unity nesnesine otomatik odaklanılmasını sağlar.

---

## 3. Mantıksal Akış
1.  **Girdi**: Herhangi bir sistem `NexusLogger.Log("Hata oluştu", LogLevel.Error)` çağrısını yapar.
2.  **Dağıtım**: Mevcut tüm `Sink` (Alıcı) havuzu taranır ve mesaj her birine iletilir.
3.  **Renklendirme**: Log seviyesine göre uygun HEX renk kodu seçilir.
4.  **Konsol Çıktısı**: Windows/Mac/Linux konsoluna renkli ve biçimlendirilmiş çıktı gönderilir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Log Sink** | Log mesajlarının nihai olarak ulaştığı hedef (Dosya, Ekran vb.). |
| **Rich Text** | Metinlerin HTML benzeri etiketlerle (color, b, i) biçimlendirilmesi. |
| **Contextual Logging** | Mesajın hangi nesneyle ilgili olduğunun bilgisini içeren kayıt. |
| **Fallback** | Birincil sistem çalışmadığında devreye giren yedekleme mekanizması. |

---

## 5. Kullanım Örneği
```csharp
// Başarı mesajı
NexusLogger.LogSuccess(this, "Simülasyon başarıyla yüklendi.");

// Hata mesajı (Kırmızı)
NexusLogger.LogError(this, "Unmanaged bellek taşması algılandı!");

// Özel bir alıcı (Sink) ekle
NexusLogger.AddSink(new MyFileStoreSink());
```

---

## 6. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Logging;

public static class NexusLogger
{
    private static readonly List<INexusLogSink> _sinks = new();
    private static readonly object _lock = new();

    public static void Log(object context, string message, LogLevel level) {
        lock (_lock) {
            foreach (var sink in _sinks) sink.Log(context, message, level);
        }
        // Unity Console output with rich text...
        Debug.Log($"<b>[Nexus]</b> <color=red>{message}</color>");
    }
}
```

---

## Nexus Optimization Tip: Conditional Tracing
`LogLevel.Trace` gibi çok yoğun veri üreten logları prodüksiyon (Final Build) aşamasında tamamen kapatmak için `#if UNITY_EDITOR` veya `[Conditional("DEBUG")]` özniteliğini kullanın. Bu, **oyun performansının loglar yüzünden düşmesini kesin olarak engeller.**
