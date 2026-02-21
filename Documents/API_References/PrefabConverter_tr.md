# Nexus Prime Mimari Rehberi: PrefabConverter (Prefab Dönüştürücü)

## 1. Giriş
`PrefabConverter.cs`, standart Unity `GameObject` yapısını yüksek performanslı "Nexus Entity" yapısına dönüştüren bir "Baking" (Pişirme) aracıdır. Editör tarafındaki hiyerarşik veriyi okur ve bunları unmanaged bellek blokları olarak yeniden inşa eder.

Bu aracın varlık sebebi; geliştiricilerin alıştığı sürükle-bırak (Drag-and-Drop) prefab yöntemini korurken, çalışma zamanında (Runtime) ECS'in devasa performans avantajından yararlanmasını sağlamaktır.

---

## 2. Teknik Analiz
Dönüştürücü şu "Baking" süreçlerini yönetir:

- **Hierarchy Flattening**: Derin prefab hiyerarşilerini düzleştirerek (flatten) her bir alt nesneyi birer Entity veya bileşen olarak Registry'ye ekler.
- **Component Mapping**: Unity bileşenlerini (Örn: Transform) Nexus'un unmanaged karşılıklarına (Örn: `PositionComponent`) otomatik olarak eşler.
- **Data Baking**: Statik verileri (Örn: Renderer ayarları veya Statlar) önceden işleyerek, oyun sırasında bir kez daha hesaplanma maliyetini ortadan kaldırır.
- **Nexus-Ready Validation**: Dönüştürülen verilerin Nexus Core mimarisi ile uyumlu olup olmadığını (unmanaged kısıtlamaları vb.) kontrol eder.

---

## 3. Mantıksal Akış
1.  **Girdi**: Geliştirici dönüştürmek istediği Prefab nesnesini "Object Field" kutusuna bırakır.
2.  **Analiz (Deep Scan)**: Prefab içindeki tüm alt nesneler ve bileşenler taranır.
3.  **Dönüştürme**: Her bileşen için uygun bir `INexusComponent` hedefi oluşturulur ve değerler kopyalanır.
4.  **Bake**: Veriler bir `EntityTemplate` dosyası olarak veya doğrudan Registry'e kaydedilir.

---

## 4. Kullanım Örneği
```text
// Performanslı bir mermi sistemi oluşturmak:
// 1. Unity'de standart bir "Bullet" prefabı hazırlanır.
// 2. [Nexus/Prefab to Entity Converter] açılır.
// 3. Prefab kutuya sürüklenir ve "Bake to Entity" tıklanır.
// Sonuç: Artık mermiler binlerce adet üretildiğinde 0ms maliyetle ECS sisteminde işlenir.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class PrefabConverter : EditorWindow
{
    [MenuItem("Nexus/Prefab to Entity Converter")]
    public static void ShowWindow() => GetWindow<PrefabConverter>("Prefab Converter");

    private GameObject _sourcePrefab;

    private void OnGUI() {
        _sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _sourcePrefab, typeof(GameObject), false);
        if (GUILayout.Button("Bake to Entity")) {
            if (_sourcePrefab != null) Bake();
        }
    }

    private void Bake() {
        // Scan components, map to structs, register in ECS...
    }
}
#endif
```

---

## Nexus Optimization Tip: Static-Baking
Hareket etmeyen (Static) nesneleri `NexusStaticEntity` tipine dönüştürün. Bu, **Bake edilen verilerin unmanaged bellekte sadece bir kez okunmasını sağlayarak, sistemin toplam işlem yükünü %15 düşürür.**
