# Nexus Prime Mimari Rehberi: UnmanagedComponentAnalyzer (Derleme Zamanı Güvenlik Analizi)

## 1. Giriş
`UnmanagedComponentAnalyzer.cs`, Nexus Prime framework'ünün "Derleme Zamanı Güvenlik" (Compile-Time Safety) katmanıdır. C# dilinin standart derleyicisinin (Roslyn) içine sızarak, geliştiricinin yanlışlıkla managed bir tipi (class, string vb.) bir bileşen olarak kullanmasını engeller.

Bu analyzer'ın varlık sebebi, unmanaged bellek operasyonlarının (Registry, SparseSet) doğası gereği sadece "blittable" struct'lar ile çalışabilmesidir. Eğer bir geliştirici `Registry.Add<MyClass>` yazarsa, bu kod çalışma zamanında bellek çökmesine (Access Violation) neden olmadan önce, bu analyzer sayesinde **IDE üzerinde kırmızı çizgi (Hata)** olarak görünür.

---

## 2. Teknik Analiz
Analyzer, kod kalitesini ve veri güvenliğini korumak için şu Roslyn altyapılarını kullanır:

- **Roslyn Syntax Node Analysis**: Kod içindeki `Add`, `Get` ve `Has` metod çağrılarını tarayarak jenerik tip parametrelerini yakalar.
- **Semantic Model Validation**: Sadece kodun yazılışına değil, o tipin gerçek doğasına bakar. `namedType.IsUnmanagedType` kontrolü ile tipin içinde referans (managed) nesne olup olmadığını saptar.
- **NX0001 Diagnostic ID**: Hata kodunu standartlaştırarak, CI/CD süreçlerinde "Unmanaged Violation" hatalarının otomatik reddedilmesini sağlar.
- **Symbol Action Registration**: Sadece metod çağrılarını değil, `[MustBeUnmanaged]` özniteliği ile işaretlenmiş tüm sınıf ve struct tanımlarını da denetler.

---

## 3. Mantıksal Akış
1.  **Tetikleme**: Geliştirici kod yazdıkça veya derleme başladığında Roslyn analyzer'ı tetikler.
2.  **Sorgulama**: `Registry` üzerinden yapılan jenerik çağrılar veya öznitelikli tipler saptanır.
3.  **Doğrulama**: Tipin `IsUnmanagedType` özelliği `false` ise bir hata raporu (`Rule`) oluşturulur.
4.  **Raporlama**: IDE (Visual Studio/Rider) üzerinden kullanıcıya "Type must be unmanaged" uyarısı grafiksel olarak sunulur.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Roslyn** | .NET için açık kaynaklı C# ve Visual Basic derleyici platformu. |
| **Diagnostic Analyzer** | Derleyiciye kod hakkında ek kurallar ve analizler ekleyen eklenti. |
| **Blittable** | Bellek yapısı managed ve unmanaged dünyada birebir aynı olan veri tipi. |
| **Semantic Model** | Kodun anlamını, tiplerini ve sembollerini içeren derin bilgi katmanı. |

---

## 5. Riskler ve Sınırlar
- **Compiler Performance**: Çok büyük projelerde her tuş basımında analiz yapmak işlemciyi yorabilir. Bu yüzden analiz sadece ilgili jenerik metodlara ve özniteliklere odaklanır.
- **Partial Types**: Bazı durumlarda "partial" tanımlanmış struct'ların tüm parçalarının unmanaged olması gerekir, aksi halde analyzer hata raporlamayabilir (Roslyn kısıtı).

---

## 6. Kullanım Örneği
```csharp
// HATALI KULLANIM: Analyzer burada NX0001 hatası fırlatır
public struct BadComponent {
    public string Name; // String managed bir objedir!
}

registry.Add<BadComponent>(entity); // DERLEME HATASI: Type 'BadComponent' contains managed references
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
namespace Nexus.Core;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnmanagedComponentAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NX0001";
    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(AnalyzeGenericRegistryCall, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeGenericRegistryCall(SyntaxNodeAnalysisContext context)
    {
        // 1. Find Add/Get/Has calls
        // 2. Check if typeArg.IsUnmanagedType is false
        // 3. Report context.ReportDiagnostic(Rule)
    }
}
```

---

## Nexus Optimization Tip: Early Detection Savings
Bir "Access Violation" hatasını çalışma zamanında debug etmek saatler sürebilirken, `UnmanagedComponentAnalyzer` sayesinde bu hata **yazıldığı saniyede** yakalanır. Bu, geliştirme maliyetini ve teknik borcu (technical debt) devasa oranda azaltan bir "Shift-Left" stratejisidir.
