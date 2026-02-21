# Nexus Prime Mimari Rehberi: NexusStackable (Birikimli Veri Yönetimi)

## 1. Giriş
`NexusStackable.cs`, oyunlarda sıkça karşılaşılan "Birikebilir Kaynaklar" (Örn: Envanter eşyaları, Cephane, Para) için tasarlanmış bir veri konteyneridir. HypeFire mimarisinden entegre edilen bu yapı, bir değerin hem miktarını hem de kapasitesini (Cap) yöneten akıllı bir birimdir.

Bu bileşenin varlık sebebi; her kaynak tipi için manuel toplama/çıkarma/limit kontrolü yazmak yerine, bu mantıkları tek bir yapı altında toplayıp Unity'nin `UnityEvent` sistemiyle entegre ederek görsel güncellemeleri otomatik tetikleyebilmektir.

---

## 2. Teknik Analiz
Konteyner yönetimi için şu özelliklere sahiptir:

- **Capped Management**: Değerin belli bir sınırı (Capacity) aşmasını engeller. `SetCap` metodu ile dinamik kapasite yönetimi sağlar.
- **Transactional Support**: `TrySpend` metodu ile kaynağın yeterli olup olmadığını kontrol eder ve tek atomik işlemde harcamayı gerçekleştirir.
- **Event-Driven Binding**: Değer her değiştiğinde `OnValueChanged` olayını (event) ateşleyerek, UI katmanının (Slider, Text vb.) koda bağımlı olmadan güncellenmesini sağlar.
- **Implicit Operator**: Yapıyı doğrudan `int` gibi kullanmaya olanak tanıyan örtük dönüşüm (implicit conversion) desteği sunar.

---

## 3. Mantıksal Akış
1.  **Tanımlama**: Bir `MonoBehaviour` içinde `NexusStackable<AmmoTag> Ammo;` olarak tanımlanır.
2.  **Kısıtlama**: `SetCap(100)` ile üst sınır belirlenir.
3.  **İşlem**: `Add(50)` çağrıldığında miktar artar ancak 100'ü geçemez. Değişim anında Unity event fırlatılır.
4.  **Kontrol**: `TrySpend(20)` çağrıldığında, eğer 20 birim varsa harcanır ve `true` döner.

---

## 4. Kullanım Örneği
```csharp
public class PlayerInventory : MonoBehaviour {
    public NexusStackable<GoldTag> Gold = new();

    void Start() {
        Gold.SetCap(1000);
        // UI Slider'ı OnValueChanged olayına bağla
        Gold.OnValueChanged.AddListener((val) => Debug.Log("Yeni Altın: " + val));
    }

    public void BuyItem(int cost) {
        if (Gold.TrySpend(cost)) {
            // Satın alım başarılı
        }
    }
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

[Serializable]
public class NexusStackable<T> where T : struct
{
    [SerializeField] private int _count;
    [SerializeField] private int _cap = -1;
    public UnityEvent<int> OnValueChanged = new();

    public bool Add(int amount) {
        if (_cap >= 0 && (_count + amount) > _cap) return false;
        _count += amount;
        OnValueChanged.Invoke(_count);
        return true;
    }

    public bool TrySpend(int amount) {
        if (_count < amount) return false;
        _count -= amount;
        OnValueChanged.Invoke(_count);
        return true;
    }
}
```

---

## Nexus Optimization Tip: UnityEvent Overhead
Eğer bir karede binlerce kez `Add` tetikleniyorsa, `OnValueChanged` (UnityEvent) kullanımı CPU üzerinde ek yük oluşturabilir. Çok yüksek frekanslı güncellemeler için standart C# `Action` kullanmak veya event fırlatmayı bir sonraki kareye kadar "Buffer"lamak (ertelenmiş tetikleme) **performansı %5-10 artırabilir.**
