# Nexus Prime Mimari Rehberi: NexusRandmonoizer (Varyasyon Motoru)

## 1. Giriş
`NexusRandmonoizer.cs`, sahnede seçili olan yüzlerce veya binlerce nesnenin transform (Pozisyon, Rotasyon, Ölçek) değerlerini hızlıca çeşitlendirmeyi sağlayan bir Editör yardımcı aracıdır. Doğal bir görünüm için gerekli olan "düzensizliği" (irregularity) saniyeler içinde oluşturur.

Bu aracın varlık sebebi; ağaçlar, kayalar veya mermiler gibi çok sayıda nesnenin aynı yöne ve aynı ölçeğe sahip olmasından kaynaklanan yapay (robotic) görünümü kırarak sahne kalitesini artırmaktır.

---

## 2. Teknik Analiz
Randomizer şu operasyonel yetenekleri sunar:

- **Multi-Axis Position Logic**: `AxisType` (X, Y, Z, XY, XZ, YZ, All) seçimi ile nesnelerin sadece istenen eksenlerde dağıtılmasını sağlar.
- **Unit Circle/Sphere Randomization**: Nesneleri bir daire veya küre hacmi içine rastgele dağıtarak daha organik (kümelenmiş) yerleşimler sağlar.
- **Rotation Variation**: Nesneleri 0-360 derece arası rastgele döndürerek simetriyi bozar.
- **Uniform Scale Randomization**: Nesnelerin X, Y ve Z ölçeklerini aynı oranda (proportional) değiştirerek form bozulmalarını önler.

---

## 3. Mantıksal Akış
1.  **Seçim Yakalama**: `Selection.gameObjects` üzerinden sahnede o an seçili olan nesneler listelenir.
2.  **Parametre Belirleme**: Geliştirici Min/Max aralıklarını ve eksen tipini seçer.
3.  **Döngüsel İşlem**: Listedeki her nesne için `Random.Range` ile yeni değerler hesaplanır.
4.  **Uygulama**: Hesaplanan yeni `localPosition`, `localRotation` veya `localScale` değerleri nesnelere atanır.

---

## 4. Kullanım Örneği
```text
// Orman oluştururken:
// 1. 100 tane ağaç prefab'ı seçilir.
// 2. Window üzerinden "Randomize Rotation" tıklanır.
// 3. "Randomize Scale" (Min: 0.8, Max: 1.2) tıklanır.
// Sonuç: Birbirinden farklı duran organik bir orman görünümü.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusRandmonoizer
{
    public static void RandomizePosition(float min, float max, AxisType axis = AxisType.All) {
        foreach (var go in Selection.gameObjects) {
            // Calculate and apply random pos based on axis...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Local vs World Space
Mümkünse her zaman `localPosition` ve `localRotation` üzerinden randomizasyon yapın. Bu, **ebeveyn (parent) nesnelerin düzenini bozmadan sadece alt objeleri çeşitlendirmenize olanak tanır.**
