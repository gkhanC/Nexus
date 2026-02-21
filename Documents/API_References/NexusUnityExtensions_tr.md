# Nexus Prime Mimari Rehberi: NexusUnityExtensions (Unity Genişletmeleri)

## 1. Giriş
`NexusUnityExtensions.cs`, Unity'nin standart API'sini Nexus Prime'ın modern ve temiz kod yazım standartlarına yaklaştıran bir yardımcı (utility) kütüphanesidir. Unity nesneleriyle çalışırken oluşan boilerplate (basmakalıp) kodu azaltmak ve performansı artırmak için tasarlanmıştır.

Bu kütüphanenin varlık sebebi; Unity'nin yerleşik metodlarındaki performans darboğazlarını (Örn: Sık `null` kontrolleri veya hiyerarşi gezintileri) aşmak ve geliştiriciye daha akıcı, okunabilir bir API sunmaktır.

---

## 2. Teknik Analiz
Geliştirici verimliliği için şu kritik genişletmeleri sağlar:

- **Smart Null Checks**: Unity nesneleri için `IsNull()` ve `IsNotNull()` metodlarını sunar. Bu, Unity'nin `null` operatöründeki yerleşik performans maliyetinden kaçınmak için bir best-practice katmanıdır.
- **Transform Mastery**: `ResetLocal`, `SetX/Y/Z` gibi metodlarla Transform değerlerinin atomik ve hızlı güncellenmesini sağlar. Sadece tek bir ekseni değiştirmek için yeni `Vector3` oluşturma külfetini ortadan kaldırır.
- **Hierarchy Navigation**: `GetOrAddComponent` ve `ForEachChild` gibi metodlarla sahnede nesne arama ve yönetme işlemlerini optimize eder.
- **Functional Collections**: `IEnumerable` için `ForEach` desteği sunarak kodun daha deklaratif (fonksiyonel) yazılmasını sağlar.

---

## 3. Mantıksal Akış
1.  **Kapsam**: Genişletmeler tüm Unity temel sınıfları (Transform, GameObject, Vector3) üzerinde kullanılabilir durumdadır.
2.  **Yürütüm**: Metodlar, doğrudan tipin üzerine bir üye gibi çağrılır (Extension Method).
3.  **Performans**: Çoğu metod, Unity'nin derleyici dostu formlarını (`Vector3.zero` vb.) kullanarak gereksiz bellek tahsisatını (Allocation) önler.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Extension Method** | Bir tipi modifiye etmeden ona dışarıdan metod ekleme tekniği. |
| **Boilerplate Code** | Birden çok yerde tekrarlanan, az fonksiyonel ama yazılması zorunlu kod blokları. |
| **Atomic Updates** | Bir verinin sadece ilgili kısmının (Örn: sadece X ekseni) hızlıca güncellenmesi. |
| **Fluent API** | Metodların birbirine zincirlenebildiği, akıcı okunabilen kod yapısı. |

---

## 5. Kullanım Örneği
```csharp
// Standart Unity kodu
if (myObj != null) {
    var rb = myObj.GetComponent<Rigidbody>();
    if (rb == null) rb = myObj.AddComponent<Rigidbody>();
    myObj.transform.position = new Vector3(10, myObj.transform.position.y, myObj.transform.position.z);
}

// Nexus Extension kod yazımı
if (myObj.IsNotNull()) {
    myObj.GetOrAdd<Rigidbody>();
    myObj.transform.SetX(10f);
}
```

---

## 6. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

public static class NexusUnityExtensions
{
    public static bool IsNull(this UnityEngine.Object obj) => obj == null;
    
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component {
        T c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    public static void SetX(this Transform t, float x) => t.position = new Vector3(x, t.position.y, t.position.z);
}
```

---

## Nexus Optimization Tip: Avoid Native Null Checks
Unity nesnelerindeki standart `obj == null` kontrolü, C++ katmanına sorgu attığı için yavaştır. `NexusUnityExtensions.IsNull` gibi basit wrapperlar kullanmak, **binlerce nesnelik döngülerde milisaniye seviyesinde kazanç sağlayabilir.**
