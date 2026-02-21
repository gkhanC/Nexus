# Nexus Prime Mimari Rehberi: NexusUIUtilities (Görsel Arayüz Yardımcıları)

## 1. Giriş
`NexusUIUtilities.cs`, Nexus Prime veri dünyasının kullanıcıya sunulduğu "Görsel Uç Nokta"ları yöneten bir koleksiyondur. Reaktif UI bağlama (`NexusUIBindings`), kamera-odaklı görselleştirme (`NexusBillboardUI`) ve proje düzenleme araçlarını tek bir dosyada birleştirir.

Bu yardımcıların varlık sebebi; unmanaged verilerin (Örn: Oyuncu adı, Can değeri) TextMeshPro gibi bileşenlere hızlıca basılmasını sağlamak ve sahadaki nesnelerin her zaman oyuncuya bakmasını garanti etmektir.

---

## 2. Teknik Analiz
Görsel sunum için şu modülleri sunar:

- **NexusUIBindings**: `TMP_Text` bileşenlerini hedef alarak, unmanaged verilerden gelen string değerlerini ekrana yansıtır. Veri değişimlerinde reaktif tetikleme yapısına uygundur.
- **NexusBillboardUI**: Nesnenin her zaman ana kameraya (`Camera.main`) dik bir açıyla bakmasını sağlar. Bunu yaparken `LateUpdate` kullanarak kameranın son konumuna göre kendini en güncel şekilde konumlandırır.
- **NexusFolderManager (Editor)**: Proje içindeki varlıkların (Script, Model, Texture vb.) unmanaged proje standartlarına göre otomatik olarak organize edilmesini sağlar.

---

## 3. Mantıksal Akış
1.  **Veri Güncelleme**: Nexus simülasyonu bir değeri günceller.
2.  **Yansıtma**: `UpdateValue` çağrılır ve UI metni yenilenir.
3.  **Hizalama**: Billboard bileşeni, her karede kameranın rotasyon matrisini kullanarak kendi rotasyonunu günceller.
4.  **Düzenleme**: Editör tarafında proje hiyerarşisi temiz tutulur.

---

## 4. Kullanım Örneği
```csharp
// Can değerini UI'da göster
var healthUI = GetComponent<NexusUIBindings>();
healthUI.UpdateValue("HP: 100/100");

// Bir nesneyi her zaman kameraya baktır (Örn: Karakter adı)
gameObject.AddComponent<NexusBillboardUI>();
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity.UI;

public class NexusUIBindings : MonoBehaviour
{
    public TMP_Text Label;
    public void UpdateValue(string value) => Label.text = value;
}

public class NexusBillboardUI : MonoBehaviour
{
    private Transform _camTransform;
    void Start() => _camTransform = Camera.main.transform;
    void LateUpdate() => transform.LookAt(transform.position + _camTransform.rotation * Vector3.forward, _camTransform.rotation * Vector3.up);
}
```

---

## Nexus Optimization Tip: Billboard Caching
`Camera.main` çağrısı Unity'de arka planda `GameObject.Find` kadar pahalı olabilir. `NexusBillboardUI` içinde yapıldığı gibi kamera referansını bir kez `Start` anında önbelleğe (Cache) almak, **her karede CPU çekim gücünü korumanıza yardımcı olur.**
