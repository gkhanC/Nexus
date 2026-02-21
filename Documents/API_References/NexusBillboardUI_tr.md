# Nexus Prime Mimari Rehberi: NexusBillboardUI (Kameraya Bakan Arayüz)

## 1. Giriş
`NexusBillboardUI.cs`, 3D dünya içindeki nesnelerin (Örn: Can barları, isim etiketleri) her zaman aktif kameraya bakacak şekilde dönmesini sağlayan bir "Görsel Hizalama" (Visual Alignment) aracıdır. HypeFire mimarisinden entegre edilmiştir.

Bu bileşenin varlık sebebi; 3D uzaydaki 2D arayüz elemanlarının açısının kamera hareketlerinden dolayı bozulmasını engellemek ve oyuncunun bu bilgileri her zaman en net şekilde okuyabilmesini sağlamaktır.

---

## 2. Teknik Analiz
Bileşen, Unity'nin transform sistemini şu şekilde manipüle eder:

- **Camera Locking**: `Camera.main` referansını yakalar ve nesnenin rotasyonunu kameranın rotasyonuna eşitler.
- **LateUpdate Usage**: Rotasyon güncellemesi `LateUpdate` aşamasında yapılır. Bu, kameranın kendi hareketi bittikten sonra billboard işleminin gerçekleşmesini sağlayarak titremeleri (Jitter) önler.
- **Performance**: Sadece rotasyon eşitlemesi yaptığı için işlem maliyeti son derece düşüktür.

---

## 3. Mantıksal Akış
1.  **Başlangıç (Start)**: Sahnedeki ana kamera saptanır.
2.  **Güncelleme (LateUpdate)**: Eğer kamera aktifse, nesnenin `transform.rotation` değeri kameranınkiyle senkronize edilir.
3.  **Sonuç**: Nesne, kamera ne kadar dönerse dönsün her zaman düz bir şekilde oyuncuya bakar.

---

## 4. Kullanım Örneği
```csharp
// Bir World-Space UI Canvas'ına bu scripti eklemeniz yeterlidir.
// Can barı her zaman kameraya paralel kalacaktır.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

public class NexusBillboardUI : MonoBehaviour
{
    private Camera _mainCamera;

    private void Start() => _mainCamera = Camera.main;

    private void LateUpdate() {
        if (_mainCamera != null) transform.rotation = _mainCamera.transform.rotation;
    }
}
```

---

## Nexus Optimization Tip: Update Culling
Eğer ekranda çok fazla Billboard nesnesi varsa, bu işlemi her karede yapmak yerine kameradan uzaklığa göre (LOD) seyreltin. Çok uzaktaki nesneler için rotasyon güncellemeyi durdurmak, **büyük sahnelerde Transform işlem yükünü %10 oranında azaltabilir.**
