# Nexus Prime Mimari Rehberi: NexusGameplayBases (Temel Oynanış Motorları)

## 1. Giriş
`NexusGameplayBases.cs`, Nexus Prime içindeki en yaygın kullanılan mikro-kontrolörlerin toplandığı bir "Kütüphane Dosyası"dır. Hareket (Move), Rotasyon (Rotate), Fizik Tahmini (Trajectory) ve Görsel Renk Efektleri (Hue Shift) gibi temel oyun mekaniklerini standart bir yapıda sunar.

Bu dosyanın varlık sebebi; her yeni nesne için sıfırdan basit hareket veya rotasyon kodu yazmak yerine, optimize edilmiş ve test edilmiş temel yapı taşlarını (Building Blocks) hazırda bulundurmaktır.

---

## 2. Teknik Analiz
Dosya içinde şu bağımsız motorlar yer alır:

- **NexusRotateController**: Nesneleri belirli bir hızda (`RotationSpeed`) sürekli döndüren görsel bir araçtır.
- **NexusRigidbodyMove**: Unity'nin fizik motorunu (`Rigidbody`) kullanarak, nesnelerin `fixedDeltaTime` hassasiyetinde hareket etmesini sağlar.
- **NexusTrajectorySimulator (Statik)**: Yerçekimi altındaki bir nesnenin gelecekteki konumunu `start + velocity * t + 0.5 * g * t^2` formülüyle matematiksel olarak tahmin eder.
- **NexusHueShifter**: Bir nesnenin rengini HSV spektrumu üzerinde sürekli kaydırarak dinamik görsel efektler oluşturur.

---

## 3. Mantıksal Akış
1.  **Bileşen Ekleme**: İlgili mekanik (Örn: Sürekli dönen bir para nesnesi) için nesneye `NexusRotateController` eklenir.
2.  **Yapılandırma**: Hız veya hedef değerler Inspector üzerinden girilir.
3.  **Yürütüm**: Bileşenler, unmanaged dünyadan bağımsız olarak veya unmanaged dünyadan gelen sinyallerle Unity'nin `Update/FixedUpdate` döngüsünde çalışır.

---

## 4. Kullanım Örneği
```csharp
// Bir nesneyi fiziksel olarak hareket ettir
var mover = GetComponent<NexusRigidbodyMove>();
mover.Move(Vector3.forward);

// Bir merminin 2 saniye sonra nerede olacağını tahmin et
Vector3 futurePos = NexusTrajectorySimulator.GetPointAtTime(pos, vel, 2f);
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity.Controllers;

public class NexusRotateController : MonoBehaviour {
    public Vector3 RotationSpeed;
    void Update() => transform.Rotate(RotationSpeed * Time.deltaTime);
}

public class NexusRigidbodyMove : MonoBehaviour {
    private Rigidbody _rb;
    public float Speed = 10f;
    void Awake() => _rb = GetComponent<Rigidbody>();
    public void Move(Vector3 dir) => _rb.MovePosition(transform.position + dir * Speed * Time.fixedDeltaTime);
}
```

---

## Nexus Optimization Tip: Component Sharing
`NexusGameplayBases` içindeki sınıflar oldukça hafiftir (Lightweight). Ancak binlerce nesnede `NexusHueShifter` gibi sürekli `material.color` değiştiren yapılar yerine, tek bir merkezi sistemden `MaterialPropertyBlock` güncellemeyi tercih etmek, **görsel render yükünü %20 azaltır.**
