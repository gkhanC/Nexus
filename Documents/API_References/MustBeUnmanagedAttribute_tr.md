# Nexus Prime Mimari Rehberi: MustBeUnmanagedAttribute (Unmanaged Tip Zorunluluğu)

## 1. Giriş
`MustBeUnmanagedAttribute.cs`, Nexus Prime'ın bellek güvenliği (Memory Safety) mimarisinde bir "Mühür" (Seal) görevi görür. Bir struct veya sınıfın kesinlikle unmanaged (blittable) olması gerektiğini derleyiciye ve analyzer'lara bildirir.

Bu özniteliğin varlık sebebi, unmanaged bellek ile çalışan sistemlerde (Örn: `Registry`, `Snapshot`) yanlışlıkla referans tipi (managed object) içeren yapıların kullanılmasını derleme anında (Compile-time) engellemek ve çalışma zamanında oluşabilecek bellek bozulmalarını (Corruption) sıfıra indirmektir.

---

## 2. Teknik Analiz
MustBeUnmanagedAttribute, sistem bütünlüğü için şu rolleri üstlenir:

- **Constraint Enforcement**: `UnmanagedComponentAnalyzer` (Roslyn) tarafından taranır. Eğer bu özniteliğe sahip bir tip içinde `string`, `class` veya `list` gibi managed referanslar varsa "NX0001" hatası tetiklenir.
- **Documentation by Code**: Geliştiriciye bu tipin sadece ham bellek (raw memory) üzerinde yaşadığını ve `NativeMemory` operasyonlarına uygun olduğunu beyan eder.
- **Structural Integrity**: Tipin bellek diziliminin (Memory Layout) deterministik kalmasını garanti altına alır.

---

## 3. Mantıksal Akış
1.  **Tanımlama**: Geliştirici, bir bileşeni `[MustBeUnmanaged]` ile işaretler.
2.  **Analiz**: Roslyn tabanlı analyzer, kodu tararken bu etiketi görür.
3.  **Doğrulama**: Tipin içindeki tüm alanlar (fields) unmanaged mi diye kontrol edilir.
4.  **Sonuç**: Eğer kural ihlal edilmişse, visual studio/rider üzerinde kırmızı hata çizgisi belirir.

---

## 4. Kullanım Örneği
```csharp
using Nexus.Attributes;

[MustBeUnmanaged]
public struct PlayerStats {
    public int Level;
    public float Experience;
    // string Name; // HATA: Analyzer bu satır yüzünden derlemeyi durdurur!
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Attributes;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class MustBeUnmanagedAttribute : Attribute { }
```

---

## Nexus Optimization Tip: Early Verification
`[MustBeUnmanaged]` özniteliği, bellek hatalarını runtime'dan compile-time'a çeker (Shift-Left). Bu, debug sürelerini kısaltırken, uygulamanızın **bellek stabilitesini %100 oranında garanti altına almanıza yardımcı olur.**
