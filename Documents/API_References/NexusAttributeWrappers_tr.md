# Nexus Prime Mimari Rehberi: NexusAttributeWrappers (Managed-Unmanaged Veri Köprüsü)

## 1. Giriş
`NexusAttributeWrappers.cs`, Nexus Prime'ın unmanaged veri yapılarını Unity'nin managed dünyasına ve özellikle de `Inspector` paneline bağlayan bir "Serileştirme Köprüsü"dür. Unmanaged struct'lar (Örn: `NexusAttribute`) Unity Inspector tarafından doğrudan düzenlenemez; bu kütüphane bu engeli ortadan kaldırır.

Bu sarmalayıcıların varlık sebebi; geliştiricinin unmanaged bellek yapısını elle kodlamak yerine, Unity Editörü'nün konforlu arayüzünü kullanarak başlangıç verilerini (Can, Menzil vb.) düzenleyebilmesini sağlamaktır.

---

## 2. Teknik Analiz
İki ana veri tipi için managed sarmalayıcılar sunar:

- **NexusAttributeWrapper**: `NexusAttribute` (`Current/Max`) yapısının managed halidir. `ToUnmanaged()` metodu ile Inspector'dan girilen veriyi unmanaged bellek adreslerine yazılabilecek saf struct formuna dönüştürür.
- **NexusMinMaxWrapper**: `NexusMinMax<float>` yapısını sarar. Tasarımcıların "Minimum" ve "Maksimum" sayısal aralıkları Unity arayüzünden kolayca girmesine olanak tanır.
- **Bi-Directional Sync**: `FromUnmanaged` metodları sayesinde, oyun sırasında değişen unmanaged verilerin tekrar Inspector üzerinde (Debug amaçlı) görünmesini sağlar.

---

## 3. Mantıksal Akış
1.  **Tanımlama**: Bir `MonoBehaviour` içinde `public NexusAttributeWrapper StartHealth;` şeklinde tanımlanır.
2.  **Tasarım**: Geliştirici Unity Inspector üzerinden değerleri (Örn: 100/100) girer.
3.  **Dönüştürme**: Nesne oluşturulduğunda `ToUnmanaged()` çağrılır ve veri Nexus Registry içindeki unmanaged alana kopyalanır.
4.  **Geribildirim**: Eğer unmanaged veri değişirse, `FromUnmanaged()` çağrılarak Editördeki görsel değerler güncellenebilir.

---

## 4. Kullanım Örneği
```csharp
public class CharacterConfig : MonoBehaviour {
    public NexusAttributeWrapper Health;

    public void ApplyToEntity(EntityId id, Registry registry) {
        // Inspector'dan gelen veriyi unmanaged dünyaya aktar
        registry.Set(id, Health.ToUnmanaged());
    }
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

[Serializable]
public class NexusAttributeWrapper
{
    public float CurrentValue;
    public float MaxValue;

    public NexusAttribute ToUnmanaged() => new NexusAttribute { Current = CurrentValue, Max = MaxValue };
    
    public void FromUnmanaged(NexusAttribute attr) {
        CurrentValue = attr.Current;
        MaxValue = attr.Max;
    }
}
```

---

## Nexus Optimization Tip: One-Way Initialization
Performans için, wrapper sınıfları sadece "Başlangıç Verisi" (Initial Data) yüklemede kullanın. Oyun sırasında her karede unmanaged veriyi wrapper'a geri yazmaya çalışmak (Sync-back), özellikle binlerce varlıkta **gereksiz CPU maliyeti ve Garbage (GC) oluşturabilir.** Sadece debug modunda çift yönlü senkronizasyonu aktif tutun.
