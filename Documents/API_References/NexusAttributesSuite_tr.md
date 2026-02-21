# Nexus Prime Mimari Rehberi: NexusAttributesSuite (Gelişmiş Öznitelik Seti)

## 1. Giriş
`NexusAttributesSuite.cs`, Nexus Prime projelerinde geliştirici deneyimini (UX) ve performans ölçümünü en üst seviyeye taşıyan "Gelişmiş Yardımcılar" setidir. Kod tabanını temiz tutarken, Inspector arayüzünü profesyonel bir kontrol paneline dönüştürür.

Bu setin varlık sebebi; karmaşık nesne yapılarını gruplandırmak (`Foldout`), koşullu görünürlük sağlamak (`ConditionalField`) ve kritik metodların performansını anlık olarak ölçmektir (`Benchmark`).

---

## 2. Teknik Analiz
Suite paketi şu ileri seviye yetenekleri sunar:

- **UX Modülleri**:
  - **[Foldout]**: Devasa değişken listelerini başlıklar altında daraltılabilir (collapsible) bloklara böler.
  - **[ConditionalField]**: Bir alanın görünürlüğünü, başka bir değişkenin değerine bağlar (Örn: "HasAI" true ise AI ayarlarını göster).
  - **[Tag & Layer Selection]**: String veya int alanlarını Unity'nin Tag ve Layer listelerinden seçilebilir arayüzlere (Dropdown) dönüştürür.
- **Performans & Mühendislik Modülleri**:
  - **[Benchmark]**: Metodun çalışma süresini milisaniye cinsinden ölçer ve loglar. Optimizasyon süreçlerinde paha biçilemezdir.
  - **[NexusInlined]**: Source Generator'a bu metodun performans için satır içine (inline) alınması gerektiğini fısıldar.
  - **[LockInPlayMode]**: Oyun çalışırken kritik ayarların değiştirilip simülasyonun bozulmasını önler.

---

## 3. Mantıksal Akış
1.  **Dekorasyon**: Geliştirici ilgili alanı `[ConditionalField("UsePhysics")]` gibi bir etiketle işaretler.
2.  **Özel Çizim (Drawer)**: `OptionalValuesDrawer` gibi sınıflar, standart Unity çizimini override ederek (üzerine yazarak) daha akıllı UI elemanları sunar.
3.  **Mesajlaşma**: `OnValueChanged` etiketi ile bir değer değiştiğinde otomatik olarak bir metod tetiklenerek reaktif bir akış başlatılır.

---

## 4. Kullanım Örneği
```csharp
[Foldout("AI Settings")]
public bool LookAtTarget;

[ConditionalField("LookAtTarget")]
[ValidationRule(0, 100)]
public float RotationSpeed;

[Benchmark]
public void CalculatePath() { ... }
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Attributes;

public class FoldoutAttribute : PropertyAttribute {
    public string Name { get; }
    public FoldoutAttribute(string name) => Name = name;
}

public class BenchmarkAttribute : Attribute { }

public class OnValueChangedAttribute : PropertyAttribute {
    public string MethodName { get; }
    public OnValueChangedAttribute(string m) => MethodName = m;
}
```

---

## Nexus Optimization Tip: Searchable Lists
`[Searchable]` özniteliği, binlerce elemanlı listelerde (Örn: Item Database) arama barı sunar. Bu, sadece Editör hızını artırmakla kalmaz, aynı zamanda devasa dizilerin Inspector'da çizilme yükünü (Draw Calls) sadece görünür elemanlara indirgeyerek **Editör performansını %30 artırabilir.**
