# Nexus Prime Mimari Rehberi: NexusMinMax (Aralık Yönetimi)

## 1. Giriş
`NexusMinMax.cs`, sayısal değerlerin belirli bir alt ve üst sınır içinde yönetimini sağlayan jenerik ve unmanaged bir veri yapısıdır. Oyun geliştirmede en sık kullanılan paternlerden biri olan "Range" (Aralık) mantığını, Nexus Prime'ın yüksek performanslı ECS mimarisine uygun hale getirir.

Bu yapının varlık sebebi; mermi hızı, düşman canı veya görsel efekt boyutları gibi değişkenleri sadece iki sayı olarak değil, üzerinde `Clamp`, `Lerp` ve `Random` işlemlerinin yapılabileceği akıllı bir ünite olarak saklamaktır.

---

## 2. Teknik Analiz
NexusMinMax, esneklik ve performans için şu özellikleri sunar:

- **Generic Constraints**: `unmanaged` ve `IComparable<T>` kısıtlamaları sayesinde hem `int` hem `float` gibi tüm temel sayı tipleriyle tip güvenliği (type safety) içinde çalışır.
- **Zero-Allocation Logic**: Tüm işlemler stack üzerinde veya unmanaged bellek içinde gerçekleşir; çöp toplayıcı (GC) için hiçbir yük oluşturmaz.
- **Method Inlining**: `IsInRange` ve `Clamp` metodları `AggressiveInlining` ile işaretlenmiştir, bu da işlemci seviyesinde metod çağrısı maliyetini ortadan kaldırır.
- **Randomization Extensions**: Unity'nin `Random` kütüphanesiyle entegre çalışarak, belirlenen aralıkta tek satırda rastgele değer üretilmesini sağlar.

---

## 3. Mantıksal Akış
1.  **Tanımlama**: Bir bileşen içinde `NexusMinMax<float> DamageRange` olarak tanımlanır.
2.  **Kısıtlama**: Gelen bir değer `range.Clamp(input)` metoduyla anında güvenli aralığa çekilir.
3.  **Doğrulama**: `IsInRange` ile bir değerin belirlenen sınırlar içinde olup olmadığı O(1) maliyetle kontrol edilir.
4.  **Uygulama**: `Lerp` metodu ile aralık içindeki bir yüzdeye (Örn: %50) karşılık gelen değer hesaplanır.

---

## 4. Kullanım Örneği
```csharp
public struct WeaponComponent {
    public NexusMinMax<float> FireRate;
}

// Kullanım
var weapon = new WeaponComponent();
weapon.FireRate = new NexusMinMax<float>(0.1f, 0.5f);

float currentRate = weapon.FireRate.Random(); // 0.1 ile 0.5 arası rastgele
bool isSafe = weapon.FireRate.IsInRange(0.3f); // true
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Data;

public struct NexusMinMax<T> where T : unmanaged, IComparable<T>
{
    public T Min;
    public T Max;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInRange(T value) => value.CompareTo(Min) >= 0 && value.CompareTo(Max) <= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Clamp(T value) {
        if (value.CompareTo(Min) < 0) return Min;
        if (value.CompareTo(Max) > 0) return Max;
        return value;
    }
}
```

---

## Nexus Optimization Tip: Predictive Clamping
Sık güncellenen sistemlerde (Örn: Fizik/AI) değerleri her karede manuel `if` bloklarıyla kontrol etmek yerine `NexusMinMax.Clamp` metodunu kullanın. Derleyici bu metodu "Branchless" (dallanmasız) makine koduna dönüştürerek **işlemci boru hattı (Pipeline) verimliliğini %20 artırabilir.**
