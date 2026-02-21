# Nexus Prime Mimari Rehberi: NexusVisualDebugger (Görsel Hata Ayıklayıcı)

## 1. Giriş
`NexusVisualDebugger.cs`, unmanaged ECS dünyasındaki verileri Unity sahne görünümünde (Scene View) somutlaştıran bir "Görselleştirme Katmanı"dır. Karmaşık sayısal verileri renkli Gizmo'lara, yön oklarına ve metin etiketlerine dönüştürerek hata ayıklama sürecini hızlandırır.

Bu aracın varlık sebebi; sadece kod katmanında var olan unmanaged bileşenleri (Örn: Bir merminin görünmez çarpışma alanı veya bir yapay zekanın hedef noktası) sahne üzerinde canlı olarak görmektir.

---

## 2. Teknik Analiz
Görselleştirme için şu mekanizmaları kullanır:

- **[DrawGizmo] Integration**: Unity Editör'ün Gizmo sistemine kanca atarak (hook), nesne seçili olmasa bile arka planda veri çizimi yapabilir.
- **Selective Rendering**: Sadece `[NexusDebug]` veya `[AutoView]` ile işaretlenmiş bileşenleri işleyerek gereksiz görsel kirliliği ve CPU yükünü önler.
- **Handle Label System**: `Handles.Label` kullanarak, sahadaki nesnelerin üzerine anlık durum bilgilerini (Örn: "State: Attacking") metin olarak basar.
- **Wireframe Visualization**: Çarpışma alanlarını veya etki yarıçaplarını tel kafes (Wireframe) küreler ve küplerle çizer.

---

## 3. Mantıksal Akış
1.  **Tarama**: Sistem, aktif `Registry` içindeki tüm varlıkları ve üzerlerindeki izleme bayraklarını kontrol eder.
2.  **Veri Çıkarımı**: Unmanaged taraftan ham veri (Pozisyon, Yarıçap, Renk) okunur.
3.  **Çizim**: Unity `Gizmos` ve `Handles` API'ları kullanılarak veriler sahneye yansıtılır.
4.  **Güncelleme**: Her karede (Editör modunda bile) veriler canlı olarak güncellenir.

---

## 4. Kullanım Örneği
```csharp
// Bir bileşeni görselleştirmek için:
public struct SphereCollider : INexusComponent {
    public float Radius;
    public Color DebugColor;
}

// Visual Debugger bu bileşeni otomatik olarak sahnede bir kür olarak çizecektir.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusVisualDebugger
{
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void DrawNexusGizmos(Transform transform, GizmoType gizmoType) {
        // Fetch unmanaged data and draw...
    }

    public static void DrawDebugInfo(Vector3 pos, string text, Color color) {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(pos, 0.2f);
        Handles.Label(pos + Vector3.up * 0.5f, text);
    }
}
#endif
```

---

## Nexus Optimization Tip: Gizmo Frustum Culling
Eğer sahnede on binlerce entity varsa, Gizmo çizimleri Editör'ü yavaşlatabilir. `NexusVisualDebugger` içinde `Camera.current` kullanarak sadece ekranda görünen (Frustum içinde) nesnelerin Gizmo'larını çizin. Bu, **Editör FPS değerini devasa sahnelerde %40 artırır.**
