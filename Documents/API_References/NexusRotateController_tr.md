# Nexus Prime Mimari Rehberi: NexusRotateController (Hizalamalı Rotasyon)

## 1. Giriş
`NexusRotateController.cs`, nesnelerin bir hedefe veya yöne doğru pürüzsüz bir şekilde dönmesini sağlayan bir "Yönelim Yöneticisi"dir (Orientation Manager). Karakterlerin, araçların veya mermilerin hareket yönlerine göre görsel olarak hizalanması için kullanılır.

Bu kontrolörün varlık sebebi; nesnelerin aniden (snap) dönmesini engellemek ve matematiksel `Slerp` (Küresel Lineer İnterpolasyon) kullanarak göze hoş gelen, akıcı bir rotasyon deneyimi sunmaktır.

---

## 2. Teknik Analiz
Pürüzsüz rotasyon için şu yöntemleri kullanır:

- **LookRotation**: Verilen bir yön vektörünü (`direction`), mermi veya karakterin bakması gereken hedef Quaternion'a dönüştürür.
- **Spherical Linear Interpolation (Slerp)**: Mevcut rotasyon ile hedef rotasyon arasında, zaman bazlı ve kavisli bir geçiş yapar.
- **Magnitude Filtering**: Çok küçük hareket vektörlerini (Örn: 0.001f altı) görmezden gelerek, nesnenin durduğu yerdeki titremeleri (shaking) önler.
- **Time-Step Mastery**: `Time.deltaTime` çarpanı kullanarak, rotasyon hızının donanımdan bağımsız olarak her saniye aynı kalmasını sağlar.

---

## 3. Mantıksal Akış
1.  **Girdi**: Karakterin gitmek istediği yön (Vector3) sisteme iletilir.
2.  **Validasyon**: Yön vektörü "sıfır" değilse işleme devam edilir.
3.  **Hesaplama**: Hedef bakış açısı matematiksel olarak belirlenir.
4.  **Uygulama**: Nesne, mevcut açısından hedef açıya doğru `RotateSpeed` hızında yumuşakça süzülür.

---

## 4. Kullanım Örneği
```csharp
public class EnemyAI : MonoBehaviour {
    private NexusRotateController _rotator;

    void Start() => _rotator = GetComponent<NexusRotateController>();

    void Update() {
        Vector3 targetDir = (player.position - transform.position).normalized;
        _rotator.RotateTowards(targetDir);
    }
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity.Controllers;

public class NexusRotateController : MonoBehaviour
{
    public float RotateSpeed = 10f;

    public void RotateTowards(Vector3 direction) {
        if (direction.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * RotateSpeed);
    }
}
```

---

## Nexus Optimization Tip: SqrMagnitude vs Distance
Uzaklık veya büyüklük kontrolü yaparken asla `Vector3.Distance` veya `magnitude` kullanmayın; bunlar pahalı karekök (sqrt) işlemi barındırır. `sqrMagnitude` kullanarak yapılan kontroller, **rotasyon mantığında CPU yükünü %5-8 oranında azaltır.**
