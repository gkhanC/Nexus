# Nexus Prime Mimari Rehberi: Bottleneck Visualizer (Darboğaz Görselleştirici)

## 1. Giriş
`BottleneckVisualizer.cs`, Nexus Prime'ın "Gerçek Zamanlı Profiler" verilerini Unity sahne görünümü (Scene View) üzerine yansıtan bir görsel uyarı sistemidir. Hangi sistemin veya hangi nesnenin işlemciyi (CPU) ne kadar yorduğunu renkli çubuklarla gösterir.

Bu aracın varlık sebebi; karmaşık performans tabloları arasında kaybolmak yerine, darboğazın (bottleneck) fiziksel olarak nerede olduğunu ("Şu düşman grubu çok ağır çalışıyor!") anında görmektir.

---

## 2. Teknik Analiz
Görselleştirme şu yöntemleri kullanır:

- **Handles.HighLevel UI**: Unity Editör'ün `Handles` sınıfını kullanarak 2D barlar ve metin kutularını 3D dünya üzerine çizer.
- **System Cost Mapping**: `NexusPerformanceGuard` tarafından sağlanan milisaniye (ms) bazlı verileri alır.
- **Color Coding**: 0-5ms arası **Yeşil**, 5-10ms **Sarı**, 10ms+ **Kırmızı** olarak sistemleri renklendirir.
- **Frustum Dependent Rendering**: Sadece kameranın gördüğü alanda çizim yaparak Editör performansını korur.

---

## 3. Mantıksal Akış
1.  **Ölçüm**: `NexusProfiler` ve alt sistemler her karenin maliyetini ölçer.
2.  **Dönüştürme**: Dünya koordinatlarındaki entity pozisyonları ekran koordinatlarına (Screen Space) izdüşürülür.
3.  **Çizim**: Her sistem/nesne başının üzerine ilgili maliyeti gösteren bir bar grafik çizilir.

---

## 4. Kullanım Örneği
```csharp
// Bir sistemin maliyetini sahnede göstermek:
BottleneckVisualizer.DrawSystemMetrics(screenPos, "PhysicsJob", 2.4f);

// Sonuç: Ekranda PhysicsJob yazan ve %25 dolu bir yeşil bar görünür.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class BottleneckVisualizer
{
    public static void DrawSystemMetrics(Vector2 screenPos, string name, float costMs) {
        // Use Handles UI to draw bar and label...
    }
}
#endif
```

---

## Nexus Optimization Tip: Cluster Visualization
Binlerce merminin her birine ayrı bar çizmek yerine, mermi grubunu bir "Cluster" (Küme) olarak görüp tek bir toplu maliyet barı çizin. Bu, **profilleme sırasında görsel kirliliği %90 azaltarak asıl soruna odaklanmanızı sağlar.**
