# Nexus Prime Mimari Rehberi: NexusGraphLogicEditor (Grafik Mantık Editörü)

## 1. Giriş
`NexusGraphLogicEditor.cs`, Nexus Prime'ın "Düşük Seviyeli İşlem Hattı"nı (ECS Pipeline) görsel bir düğüm (Node) şemasına dönüştüren mimari bir yardımcıdır. Sistemler arasındaki veri bağımlılıklarını (Read/Write) çizgilerle göstererek, hangi sistemin hangi veriyi ürettiğini ve hangisinin tükettiğini netleştirir.

Bu editörün varlık sebebi; kod içinde kaybolan sistem hiyerarşisini kuş bakışı görerek, olası veri çakışmalarını (Race Condition) ve mantıksal döngüleri (Circular Dependency) engellemektir.

---

## 2. Teknik Analiz
Düğüm tabanlı editör şu yetenekleri sunar:

- **Node-Based Visualization**: Her sistem (System) bir kutu (Node) olarak temsil edilir.
- **Dependency Tracking**: Bir sistemin hangi bileşenleri okuduğu (`In: [Position]`) ve hangilerini yazdığı (`Out: [Velocity]`) kutu üzerinde listelenir.
- **Pipeline Flow**: Veri akışı soldan sağa veya hiyerarşik bir ağ şeklinde görselleştirilir.
- **GUI.Box Logic**: Unity'nin `GUILayout` ve `GUI.Box` yeteneklerini kullanarak, ek yazılıma ihtiyaç duymadan Editör içinde grafik arayüzü çizer.

---

## 3. Mantıksal Akış
1.  **Tarama**: Aktif Nexus Registry ve System Manager taranarak tüm "System" sınıfları listelenir.
2.  **İlişkilendirme**: Sistemlerin içindeki `[ReadOnly]` veya `[WriteOnly]` unmanaged veri erişim bayrakları analiz edilir.
3.  **Çizim**: Sistemler arası ilişkiler, veriyi yazan sistemden okuyan sisteme doğru çizgilerle (Bezier curves veya basit hatlar) bağlanır.
4.  **Hata Tespiti**: Eğer iki sistem aynı unmanaged veriye aynı anda yazmaya çalışıyorsa (Çakışma), ilgili düğümler kırmızıya boyanır.

---

## 4. Kullanım Örneği
```text
// Mimariyi analiz etmek:
// 1. [Nexus/Graph Logic Editor] açılır.
// 2. Tabloda "MovementSystem" düğümü bulunur.
// 3. Çıktı ucunun "Position" verisine gittiği ve "RenderSystem" tarafından okunduğu görülür.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusGraphLogicEditor : EditorWindow
{
    [MenuItem("Nexus/Graph Logic Editor")]
    public static void ShowWindow() => GetWindow<NexusGraphLogicEditor>("Graph Logic");

    private void OnGUI() {
        // Draw nodes for each system...
        DrawNode(new Rect(50, 50, 150, 100), "PhysicsSystem");
    }
}
#endif
```

---

## Nexus Optimization Tip: Auto-Layout
Düğümlerin el ile düzenlenmesi yerine, `Force-Directed Graph` algoritması kullanarak düğümleri otomatik olarak ekrana dağıtın. Bu, **çok büyük projelerde (100+ Sistem) karmaşayı saniyeler içinde çözerek mimariyi anlaşılır kılar.**
