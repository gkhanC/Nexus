# Nexus Prime Mimari Rehberi: NexusSwerveController (Swerve/Kaydırma Kontrolörü)

## 1. Giriş
`NexusSwerveController.cs`, özellikle mobil ve hyper-casual oyun türlerinde sıkça rastlanan "Yatay Kaydırma" (Swerve) mekaniğini yöneten bir uzman kontrolördür. Oyuncunun parmağını veya faresini sağa-sola sürüklemesiyle oluşan `NexusSwerveInput` verisini, nesnenin dünya üzerindeki pürüzsüz yatay hareketine dönüştürür.

Bu kontrolörün varlık sebebi; mobil platformlardaki hassas dokunma verilerini, oyun dünyasında pürüzsüz, sarsıntısız ve sınırları belirli bir harekete indirgemektir.

---

## 2. Teknik Analiz
Kaydırma mekaniği için şu teknikleri kullanır:

- **Input Integration**: `NexusSwerveInput` bileşeninden gelen `MoveFactorX` (parmağın son kareden beri ne kadar kaydığı) değerini temel girdi olarak alır.
- **Dynamic Clamping**: Nesnenin sahnede belirli bir yatay koridorun dışına çıkmasını `Mathf.Clamp` ile engeller (`MaxSwerveAmount`).
- **Speed Scaling**: Hareketin hızını `SwerveSpeed` ve `Time.deltaTime` ile çarparak, her türlü dokunmatik ekran hızında tutarlı bir kaydırma sağlar.
- **Local Space Movement**: Hareketi `localPosition` üzerinden yaparak, nesnenin bir üst hiyerarşiye (Örn: Sürekli ileri giden bir yol parent'ı) bağlı kalarak sadece sağa-sola gitmesini kolaylaştırır.

---

## 3. Mantıksal Akış
1.  **Girdi Okuma**: Parmağın (veya farenin) yatay değişim miktarı alınır.
2.  **Hesaplama**: Mevcut X pozisyonuna kaydırma miktarı eklenerek "Potansiyel Yeni X" bulunur.
3.  **Kısıtlama**: Potansiyel X, oyunun izin verdiği sınırlar içine (Örn: -2 ile +2 arası) zorlanır.
4.  **Uygulama**: Yeni pürüzsüz pozisyon nesneye atanır.

---

## 4. Kullanım Örneği
```csharp
// Runner tipi bir oyunda karakterin sağa sola kayması için:
// 1. Nesneye [NexusSwerveInput] ekle.
// 2. Nesneye [NexusSwerveController] ekle.
// 3. SwerveSpeed = 10, MaxSwerveAmount = 3.5 ayarlarını yap.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity.Controllers;

public class NexusSwerveController : MonoBehaviour
{
    public float SwerveSpeed = 5f;
    public float MaxSwerveAmount = 2f;
    [SerializeField] private NexusSwerveInput _input;

    private void Update() {
        float swerveAmount = Time.deltaTime * SwerveSpeed * _input.MoveFactorX;
        float targetX = Mathf.Clamp(transform.localPosition.x + swerveAmount, -MaxSwerveAmount, MaxSwerveAmount);
        transform.localPosition = new Vector3(targetX, transform.localPosition.y, transform.localPosition.z);
    }
}
```

---

## Nexus Optimization Tip: Input Smoothing
`NexusSwerveInput` içindeki ham veriyi doğrudan kullanmak yerine, `SwerveController` içinde bir `Lerp` veya `SmoothDamp` katmanından geçirin. Bu, **oyuncunun parmağını aniden çekmesi durumunda oluşabilecek sert duruşları engeller ve "Premium" bir hissiyat sağlar.**
