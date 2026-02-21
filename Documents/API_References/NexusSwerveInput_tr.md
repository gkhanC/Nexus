# Nexus Prime Mimari Rehberi: NexusSwerveInput (Swerve Girdi Okuyucu)

## 1. Giriş
`NexusSwerveInput.cs`, oyuncunun dokunmatik ekran veya fare üzerindeki "Sürükleme" (Drag) hareketlerini sayısal verilere dönüştüren düşük seviyeli bir girdi birimidir. HypeFire framework'ünden entegre edilen bu sistem, özellikle yatay hareketin ön planda olduğu oyun türlerinin temelini oluşturur.

Bu birimin varlık sebebi; ham koordinat verilerini ("Parmağım şu an (500, 300) noktasında") anlamlı bir hareket faktörüne ("Parmağım sola doğru 10 birim kaydı") dönüştürerek kontrolörlere temiz veri sağlamaktır.

---

## 2. Teknik Analiz
Girdi okuma performansı için şu araçları sunar:

- **Move Factor Processing**: Farenin veya parmağın son kareden (`_lastMouseX`) bu yana katettiği mesafeyi hesaplar ve `Sensitivity` (Hassasiyet) çarpanıyla ölçeklendirir.
- **Screen-to-World Parity**: `GetHorizontalWorldPosition` metodu ile ekrandaki dokunma noktasını, kameranın derinliğine göre oyun dünyasındaki yatay X koordinatına yansıtır.
- **Multi-Platform Consistency**: Hem `GetMouseButton` hem de mobil platformlardaki `Touch` sistemleriyle uyumlu bir mantık yürütür (Unity'nin legacy input sistemi üzerinden).

---

## 3. Mantıksal Akış
1.  **Düşüş (Down)**: Parmağın ekrana değdiği ilk koordinat kaydedilir.
2.  **Sürükleme (Drag)**: Her karede mevcut konum ile bir önceki konum arasındaki fark (`delta`) bulunur.
3.  **Sıfırlama (Up)**: Parmak ekran kalktığında hareket faktörü 0'a çekilir.
4.  **Koordinat Dönüşümü**: Gerektiğinde ekran üzerindeki piksel değeri, kameranın bakış açısına göre dünya koordinatına çevrilir.

---

## 4. Kullanım Örneği
```csharp
public class MySwerveMover : MonoBehaviour {
    [SerializeField] private NexusSwerveInput _inputReader;

    void Update() {
        // Parmağın dünyadaki tam X konumunu al
        float worldX = _inputReader.GetHorizontalWorldPosition();
        
        // Veya kaydırma hızını (delta) al
        float deltaX = _inputReader.MoveFactorX;
    }
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity.Inputs;

public class NexusSwerveInput : MonoBehaviour
{
    public float Sensitivity = 1f;
    private float _lastMouseX;
    private float _moveFactorX;

    public float MoveFactorX => _moveFactorX;

    private void Update() {
        if (Input.GetMouseButtonDown(0)) _lastMouseX = Input.mousePosition.x;
        else if (Input.GetMouseButton(0)) {
            _moveFactorX = (Input.mousePosition.x - _lastMouseX) * Sensitivity;
            _lastMouseX = Input.mousePosition.x;
        }
        else _moveFactorX = 0f;
    }
}
```

---

## Nexus Optimization Tip: Delta Smoothing
Hassas donanımlarda (Örn: 120Hz ekranlar) girdi verisi bazen çok keskin veya sarsıntılı (jitter) gelebilir. `MoveFactorX` değerini `Time.deltaTime` ile normalize etmek veya küçük bir `Lerp` ile yumuşatmak, **oyuncu kontrol hissiyatını (User Experience) önemli ölçüde iyileştirir.**
