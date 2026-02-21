# Nexus Prime Mimari Rehberi: NexusRigidbodyMove (Fizik Hareket Motoru)

## 1. Giriş
`NexusRigidbodyMove.cs`, Unity'nin `Rigidbody` bileşenini kullanarak nesnelerin fizik kurallarına uygun bir şekilde hareket etmesini sağlayan "Ağır Görev" (Heavy Duty) bir hareket motorudur. HypeFire mimarisinden rafine edilmiştir.

Bu kontrolörün varlık sebebi; nesnelerin sadece pozisyonlarını ışınlamak (teleport) yerine, çarpışma algılama ve sürtünme gibi fizik etkilerini koruyarak pürüzsüz bir seyahat gerçekleştirmesini sağlamaktır.

---

## 2. Teknik Analiz
Fiziksel hareket için şu mekanizmaları kullanır:

- **MovePosition Integration**: `Rigidbody.MovePosition` metodunu kullanarak, nesnenin fiziksel olarak bir noktadan diğerine "akmasını" sağlar. Bu, doğrudan `transform.position` değiştirmeye göre çok daha stabildir ve fizik titremelerini (jitter) engeller.
- **Velocity Control**: `SetVelocity` metodu ile nesneye doğrudan anlık hız verebilir. Bu, mermiler veya fırlatılan nesneler için idealdir.
- **FixedUpdate Dependency**: Fizik hesaplamaları Unity'nin `FixedUpdate` döngüsüne bağımlıdır, bu da tüm donanımlarda tutarlı bir hareket hızı sunar.
- **Required Component**: `[RequireComponent(typeof(Rigidbody))]` özniteliği ile nesnenin fizik kabiliyeti olmadan çalışmasını engelleyerek hata payını düşürür.

---

## 3. Mantıksal Akış
1.  **Hazırlık**: `Awake` anında nesne üzerindeki `Rigidbody` referansı önbelleğe (Cache) alınır.
2.  **Komut**: Dışarıdan bir `Move(direction)` veya `SetVelocity(vel)` komutu gelir.
3.  **Fizik Simülasyonu**: Komut, Unity'nin bir sonraki fizik adımında işlenmek üzere motora iletilir.
4.  **Sonuç**: Nesne, çarptığı engellerden sekerek veya onları iterek hedefe doğru ilerler.

---

## 4. Kullanım Örneği
```csharp
public class PlayerInput : MonoBehaviour {
    private NexusRigidbodyMove _mover;

    void Start() => _mover = GetComponent<NexusRigidbodyMove>();

    void FixedUpdate() {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _mover.Move(input);
    }
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity.Controllers;

[RequireComponent(typeof(Rigidbody))]
public class NexusRigidbodyMove : MonoBehaviour
{
    public float Speed = 5f;
    private Rigidbody _rb;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    public void Move(Vector3 direction) {
        _rb.MovePosition(transform.position + direction * Speed * Time.fixedDeltaTime);
    }

    public void SetVelocity(Vector3 velocity) {
        _rb.linearVelocity = velocity;
    }
}
```

---

## Nexus Optimization Tip: Interpolation Mode
Eğer kamera bu nesneyi takip ediyorsa, Rigidbody üzerindeki `Interpolation` ayarını "Interpolate" olarak işaretleyin. `NexusRigidbodyMove` ile birleşen bu ayar, **düşük kare hızlarında bile nesne hareketinin yağ gibi pürüzsüz görünmesini sağlar.**
