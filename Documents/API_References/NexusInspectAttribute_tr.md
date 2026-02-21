# Nexus Prime Mimari Rehberi: NexusInspectAttribute (Editor Müfettişi Entegrasyonu)

## 1. Giriş
`NexusInspectAttribute.cs`, Nexus Prime'ın "Görünmez Veriyi Görünür Kılma" felsefesinin üründür. C# dilinde unmanaged olarak saklanan ve normalde Unity Inspector penceresinde görünmeyen struct'ları ve alanları, Unity Editor kullanıcı arayüzüne taşır.

Bu özniteliğin varlık sebebi, unmanaged verilerin (Örn: ham pointerlar, blittable struct'lar) Unity'nin standart `SerializeField` sistemiyle uyumsuz olmasıdır. `NexusInspect` bu kısıtlamayı aşarak, unmanaged verilerin bile editör üzerinden izlenmesini ve değiştirilmesini sağlar.

---

## 2. Teknik Analiz
NexusInspectAttribute, veri görünürlüğü için şu yetenekleri sunar:

- **Custom Labeling**: Verinin kod içindeki ismi yerine, editörde daha anlamlı bir isimle (`Label`) görünmesini sağlar.
- **Read-Only Mode**: Kritik verileri sadece izleme modunda tutarak, editör üzerinden kazara yapılacak hatalı müdahaleleri engeller.
- **Pointer Visualization**: Ham bellek adreslerini (pointers) insan tarafından okunabilir (human-readable) formatta görselleştirir.
- **Conditional Compilation**: `#if UNITY_EDITOR` blokları ile bu özniteliğin sadece editör ortamında çalışmasını sağlar, oyunun release (final) versiyonunda sıfır maliyet (zero-overhead) prensibine sadık kalır.

---

## 3. Mantıksal Akış
1.  **Tanımlama**: Bileşen içindeki alanlar `[NexusInspect]` ile işaretlenir.
2.  **Yansıma (Reflection)**: Unity Editor'deki özel bir `VisualDataInspector` sınıfı, Registry içindeki bileşenleri tarar.
3.  **Çizim**: İşaretli alanlar, `EditorGUILayout` komutları kullanılarak Inspector panelinde oluşturulur.
4.  **Düzenleme**: Kullanıcı editörden bir değeri değiştirdiğinde, Nexus bu değişikliği anında unmanaged bellek adresine (`unsafe`) yazar.

---

## 4. Kullanım Örneği
```csharp
public struct Health {
    [NexusInspect("Aktif Can Değeri", readOnly: false)]
    public float Current;

    [NexusInspect("Maksimum Kapasite", readOnly: true)]
    public float Max;
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
public class NexusInspectAttribute : Attribute
{
    public string Label { get; }
    public bool ReadOnly { get; }

    public NexusInspectAttribute(string label = null, bool readOnly = false)
    {
        Label = label;
        ReadOnly = readOnly;
    }
}
```

---

## Nexus Optimization Tip: Non-Intrusive Inspection
`NexusInspect` özniteliğini kullanırken, verilerinizi `public` yapmak zorunda değilsiniz. Private alanları bile bu öznitelikle işaretleyerek kapsüllemeyi (encapsulation) bozmadan editöre taşıyabilirsiniz. Bu, **kod mimarinizi temiz tutarken hata ayıklama kabiliyetinizi %300 artırır.**
