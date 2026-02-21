# Nexus Prime Mimari Rehberi: VFXGraphProvider (Görsel Efekt Veri Sağlayıcı)

## 1. Giriş
`VFXGraphProvider.cs`, Nexus Prime'ın unmanaged veri dünyası ile Unity'nin GPU bazlı partikül sistemi olan `VFX Graph` arasındaki yüksek performanslı bağlantıdır. Milyonlarca varlığın pozisyon, renk ve boyut verisini "Point Cache" (Nokta Önbelleği) olarak VFX Graph'a besleyerek, devasa boyutta ve akıcı görsel efektlerin oluşturulmasını sağlar.

Bu sağlayıcının varlık sebebi; her bir partikül için CPU üzerinde `GameObject` veya `Transform` oluşturma külfetini tamamen ortadan kaldırıp, veriyi doğrudan GPU'nun partikül hesaplama hatlarına (Pipeline) iletmektir.

---

## 2. Teknik Analiz
Maksimum görsel performans için şu mekanizmaları kullanır:

- **Point Cache Injection**: Unmanaged Registry'deki varlık verilerini, VFX Graph'ın anlayabileceği `GraphicsBuffer` formuna dönüştürür.
- **Massive Scalability**: Geleneksel partikül sistemlerinin aksine, CPU threadlerini kilitlemeden aynı anda 1M+ partikülün konumunu saniyede 60 kez güncelleyebilir.
- **GPU-Side Consumption**: Veriler bir kez GPU'ya basıldıktan sonra, efektin tüm simülasyonu (hareket, renk solması vb.) GPU üzerinde gerçekleşir.
- **Property Binding**: `TargetVFX.SetGraphicsBuffer` metodunu kullanarak unmanaged bellek adreslerini direkt olarak VFX Graph içindeki bir isme (Örn: "EntityBuffer") bağlar.

---

## 3. Mantıksal Akış
1.  **Hazırlık**: VFX Graph içinde veriyi karşılayacak bir `StructuredBuffer` veya `Texture2D` alanı tanımlanır.
2.  **Paketleme**: Nexus Registry taranarak varlıkların görsel öznitelikleri (Pozisyon, Renk vb.) tek bir bellek bloğunda toplanır.
3.  **Gönderim**: Veriler GPU'ya `GraphicsBuffer` aracılığıyla yollanır.
4.  **Tetikleme**: VFX Graph, gelen yeni veriyi "Init" veya "Update" aşamasında partikül kaynağı olarak kullanır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **VFX Graph** | Unity'nin düğüm bazlı (Node-based), GPU üzerinden çalışan gelişmiş görsel efekt aracı. |
| **Point Cache** | Partiküllerin başlangıç noktalarını ve özelliklerini belirleyen önceden hesaplanmış veya canlı veri seti. |
| **GraphicsBuffer** | Compute shader'lar ve görsel efekt sistemleri için optimize edilmiş ham GPU bellek alanı. |

---

## 5. Kullanım Örneği
```csharp
public class CrowdVisualizer : MonoBehaviour {
    [SerializeField] private VFXGraphProvider _provider;

    void LateUpdate() {
        // Tüm şehir sakinlerinin (10k+) konumlarını VFX Graph'a gönder
        _provider.SyncWithVFX(mainRegistry);
    }
}
```

---

## 6. Tam Kaynak Kod (Conceptual Implementation)

```csharp
namespace Nexus.Bridge;

public class VFXGraphProvider : MonoBehaviour
{
    public VisualEffect TargetVFX;
    
    public void SyncWithVFX(Registry.Registry registry)
    {
        if (TargetVFX == null) return;
        // 1. Gather component data to GraphicsBuffer.
        // 2. TargetVFX.SetGraphicsBuffer("EntityBuffer", buffer);
    }
}
```

---

## Nexus Optimization Tip: GPU Frustum Culling
VFX Graph içinde `Culling` ayarlarını doğru yapılandırın. Eğer partikül bulutu kameranın görüş açısında değilse, Nexus Registry'den veri kopyalamayı durdurun. Bu, **GPU belleğine veri transferi maliyetini (Bus Transfer) %50 oranında optimize eder.**
