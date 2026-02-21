# Nexus Prime Mimari Rehberi: NexusCoreExtensions (Çekirdek Eklentiler)

## 1. Giriş
`NexusCoreExtensions.cs`, Unity'nin yerleşik `GameObject` ve `Object` sınıflarına eklenen düşük seviyeli yardımcı metodları (Extension Methods) içerir. Kodun okunabilirliğini artırmak ve hiyerarşi yönetimini basitleştirmek için tasarlanmıştır.

Bu eklentilerin varlık sebebi; her seferinde `foreach` döngüleri kurmak veya güvensiz null kontrolleri yapmak yerine, tek satırlık, güvenli ve performanslı arayüzler sunmaktır.

---

## 2. Teknik Analiz
Çekirdek işleyiş için şu araçları sunar:

- **NexusHierarchyExtensions**: Bir nesnenin altındaki tüm çocukları (`Transform`) hızlıca bir `List<GameObject>` olarak toplar. Sahnede toplu manipülasyon (Örn: Tüm çocukların katmanını değiştirmek) için idealdir.
- **NexusObjectExtensions (Smart Null-Check)**: Unity nesneleri için `obj == null` kontrolünün yanı sıra `obj.Equals(null)` kontrolünü de yaparak, Unity'nin "Fake Null" (Nesnenin C++ tarafı yok edilmiş ama C# tarafı hala hayatta) durumlarını güvenli bir şekilde saptar.

---

## 3. Mantıksal Akış
1.  **Hiyerarşi Tarama**: `parent.GetAllChildren()` çağrıldığında, parent'ın transformu üzerinden hızlı bir iterasyon yapılır.
2.  **Güvenli Kontrol**: Bir nesnenin gerçekten "yok olup olmadığını" anlamak için `IsNull()` metoduna başvurulur.

---

## 4. Kullanım Örneği
```csharp
// Bir nesnenin tüm çocuklarını al
List<GameObject> items = playerPrefab.GetAllChildren();

// Güvenli null kontrolü
if (target.IsNull()) {
    Debug.Log("Nesne yok edildi veya null.");
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

public static class NexusHierarchyExtensions
{
    public static List<GameObject> GetAllChildren(this GameObject parent) {
        var children = new List<GameObject>();
        foreach (Transform child in parent.transform) children.Add(child.gameObject);
        return children;
    }
}

public static class NexusObjectExtensions
{
    public static bool IsNull(this object obj) {
        return obj == null || obj.Equals(null);
    }
}
```

---

## Nexus Optimization Tip: List Pooling
`GetAllChildren` gibi metodlar her seferinde `new List<GameObject>()` oluşturarak heap bellek ayırması yapar. Çok sık kullanılan yerlerde bu listeyi bir havuzdan (Pool) çekmek, **Garbage Collector yükünü %15 azaltabilir.**
