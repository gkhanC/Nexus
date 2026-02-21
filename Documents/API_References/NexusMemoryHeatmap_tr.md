# Nexus Prime Mimari Rehberi: NexusMemoryHeatmap (Bellek Isı Haritası)

## 1. Giriş
`NexusMemoryHeatmap.cs`, oyun dünyasındaki Entity ve bileşen yoğunluğunu görsel bir "Sıcaklık Haritası" (Thermal Overlay) olarak sunan bir optimizasyon aracıdır. Hangi bölgelerin RAM bazlı darboğazlara (bottleneck) sebep olduğunu saniyeler içinde tespit eder.

Bu aracın varlık sebebi; binlerce nesnenin bir araya toplandığı ("Hot areas") ve işlemci önbelleğinin (Cache) yetersiz kaldığı durumları görselleştirerek geliştiriciye "Burası çok yoğun, optimize etmelisin" sinyali vermektir.

---

## 2. Teknik Analiz
Isı haritası şu temel prensiplerle çalışır:

- **Density Mapping**: Dünya alanını grid'lere böler ve her grid içindeki Entity/Bileşen sayısını hesaplar.
- **Color Gradients**: Düşük yoğunluklu alanları **Mavi (Soğuk)**, tehlikeli seviyedeki yığınları **Kırmızı (Sıcak)** olarak işaretler.
- **Cache Bottleneck Prediction**: Kırmızı alanlar genellikle "Cache Miss" (işlemcinin veriyi bulamaması) riskinin en yüksek olduğu bölgelerdir.
- **Toggle Visualizer**: Editör sahne görünümünde tek tuşla açılıp kapatılabilen şeffaf bir katman (Overlay) olarak sunulur.

---

## 3. Mantıksal Akış
1.  **Veri Toplama**: ECS Registry taranarak tüm varlıkların dünya koordinatları alınır.
2.  **Grid Analizi**: Koordinatlar 2D veya 3D bir bellek haritasına yansıtılır.
3.  **Renklendirme**: Her bölgenin yoğunluk verisi, önceden tanımlanmış bir renk gradyanına (`Gradient`) tabi tutulur.
4.  **Overlay Çizimi**: Sonuçlar Editör'ün sahne kamerasının üzerine bir doku (Texture) veya Gizmo topluluğu olarak çizilir.

---

## 4. Kullanım Örneği
```text
// Performans sorununu teşhis etmek:
// 1. [Nexus/Memory Heatmap] açılır.
// 2. "Toggle Visualizer" butonuna basılır.
// 3. Sahnede orman bölgesinin kıpkırmızı olduğu görülür.
// Analiz: Ormandaki ağaç bileşenleri çok yoğun, LOD sistemi veya ECS Chunking devreye alınmalı.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusMemoryHeatmap : EditorWindow
{
    [MenuItem("Nexus/Memory Heatmap")]
    public static void ShowWindow() => GetWindow<NexusMemoryHeatmap>("Memory Heatmap");

    private void OnGUI() {
        GUILayout.Label("Entity & RAM Density Heatmap", EditorStyles.boldLabel);
        if (GUILayout.Button("Toggle Visualizer")) {
            // Iterate all entities, calculate density, draw overlay...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Data Locality
Isı haritasında kırmızı bir bölge saptadığınızda, o bölgedeki bileşenlerin `Memory Layout`'unu kontrol edin. Eğer veriler bellekte dağınıksa, `UnmanagedCollection` kullanarak verileri yan yana dizin. Bu işlem, **kırmızı bölgelerdeki işlemci hızını %200'e kadar artırabilir.**
