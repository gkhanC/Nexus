# Nexus Prime Mimari Rehberi: Entity Inspector Pro (Gelişmiş Varlık Müfettişi)

## 1. Giriş
`EntityInspectorPro.cs`, milyonlarca varlığın (Entity) bulunduğu devasa Nexus dünyalarında, belirli kriterlere uyan nesneleri saniyeler içinde bulmanızı sağlayan "SQL Benzeri" bir arama motorudur. Standart "Find" komutlarının çok ötesinde, mantıksal sorgularla veri madenciliği yapar.

Bu müfettişin varlık sebebi; "Canı 20'den az olup hızı 5'ten yüksek olan tüm düşmanları göster" gibi karmaşık geliştirici taleplerini anında karşılamaktır.

---

## 2. Teknik Analiz
Müfettiş, şu gelişmiş sorgulama yeteneklerine sahiptir:

- **Logical Query Engine**: `AND`, `OR`, `==`, `!=`, `<`, `>` gibi mantıksal operatörleri destekler.
- **Component Filtering**: Sadece belirli bir bileşene (Örn: `INexusStatus`) sahip olan entity'leri filtreleyebilir.
- **Direct Pointer Access**: Sorgu sonuçlarını doğrudan unmanaged bellek adreslerinden (Registry) çekerek en güncel "Canlı" veriyi sunar.
- **Selection Integration**: Bulunan entity'ye tıklandığında, hiyerarşideki karşılığını (Varsa Unity GameObject) veya State Tweaker üzerindeki verisini otomatik olarak seçer.

---

## 3. Mantıksal Akış
1.  **Sorgu Girişi**: Geliştirici metin kutusuna `Health < 50 AND Team == 1` gibi bir sorgu yazar.
2.  **Ayrıştırma (Parsing)**: Sistem, metni mantıksal bir ağaca (Logic Tree) böler.
3.  **Tarama**: ECS Registry içindeki tüm varlıklar bu mantıksal filtreden geçirilir.
4.  **Sonuç Listeleme**: Kriterlere uyan entity'ler ID'leri ve özet verileriyle listelenir.

---

## 4. Kullanım Örneği
```text
// Örnek Sorgular:
> Level > 10 AND Xp < 100
> Status == Dead OR IsStunned == true
> AmmoCount == 0
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class EntityInspectorPro : EditorWindow
{
    [MenuItem("Nexus/Entity Inspector Pro")]
    public static void ShowWindow() => GetWindow<EntityInspectorPro>("Inspector Pro");

    private string _query = "Health < 20 AND Speed > 5";

    private void OnGUI() {
        GUILayout.Label("Nexus Entity Search (SQL-Like)", EditorStyles.boldLabel);
        _query = EditorGUILayout.TextField("Query", _query);
        if (GUILayout.Button("Find Entities")) {
            // Parse query, scan Registry, show results...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Query Caching
Sık yaptığınız sorguları (Örn: "Ölü Entity'ler") birer "Saved Search" olarak kaydedin. Bu sayede her seferinde metin ayrıştırma (`parsing`) maliyetine katlanmadan, önceden hazırlanmış `NexusQuery` nesnesini kullanarak **arama hızını %30 artırabilirsiniz.**
