# Nexus Prime Mimari Rehberi: NexusDebugAttribute (Görsel Hata Ayıklama)

## 1. Giriş
`NexusDebugAttribute.cs`, Nexus Prime'ın unmanaged veri dünyasını Unity Editor'ün görsel dünyasına bağlayan "Görsel Teşhis" (Visual Diagnostics) aracıdır. Kodun içinde sadece sayılar ve byte'lar olarak duran verilerin (Örn: Pozisyon, Hız, Hedef) Unity Scene view üzerinde şekil veya etiket olarak görünmesini sağlar.

Bu özniteliğin varlık sebebi, unmanaged struct'ların Unity Gizmos sistemine doğrudan erişememesidir. `NexusDebug` ile işaretlenen veriler, Nexus Editor Suite tarafından otomatik olarak yakalanır ve ekrana çizilir.

---

## 2. Teknik Analiz
NexusDebugAttribute, hata ayıklama süreçleri için şu parametreleri sunar:

- **DebugShape Enum**: Veriyi `Point`, `Arrow`, `Line` veya `Label` (Yazı) olarak görselleştirme seçeneği sunar.
- **Color Selection**: Onluk (Hex) renk kodları ile görselleştirmenin rengini belirlemeyi sağlar.
- **Scale Control**: Çizilen şekillerin boyutunu (`Size`) dinamik olarak ayarlar.
- **Reflective Discovery**: Editor araçları, sistemleri tararken bu özniteliği taşıyan alanları otomatik olarak "Draw List"e ekler.

---

## 3. Mantıksal Akış
1.  **İşaretleme**: Geliştirici, debug etmek istediği bir bileşeni `[NexusDebug]` ile işaretler.
2.  **Yakalama**: Unity Editor çalışırken, Nexus'un Gizmo yöneticisi bu özniteliği taşıyan aktif varlıkları bulur.
3.  **Dönüştürme**: Unmanaged veri (Örn: `Vector3`), seçilen şekle (Shape) uygun Unity Gizmos komutuna dönüştürülür.
4.  **Çizim**: Scene view üzerinde gerçek zamanlı olarak görselleştirilir.

---

## 4. Kullanım Örneği
```csharp
[NexusDebug(DebugShape.Arrow, "#FF0000", 2.0f)]
public struct Velocity {
    public float3 Value;
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Attributes;

public enum DebugShape { Point, Arrow, Line, Label }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
public class NexusDebugAttribute : Attribute
{
    public DebugShape Shape { get; }
    public string ColorHex { get; }
    public float Size { get; }

    public NexusDebugAttribute(DebugShape shape = DebugShape.Label, string colorHex = "#FFFFFF", float size = 1.0f)
    {
        Shape = shape;
        ColorHex = colorHex;
        Size = size;
    }
}
```

---

## Nexus Optimization Tip: Targeted Debugging
Tüm varlıkları debug etmek yerine sadece seçili (selected) varlıkları veya belirli bir yarıçaptaki varlıkları görselleştirmek için `NexusDebug` parametrelerini filtreleyin. Bu, **Scene view FPS düşüşünü engelleyerek binlerce varlık arasında rahatça çalışmanızı sağlar.**
