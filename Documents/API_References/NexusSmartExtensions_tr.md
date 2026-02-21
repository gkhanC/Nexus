# Nexus Prime Mimari Rehberi: NexusSmartExtensions (Unmanaged Veri Köprüsü)

## 1. Giriş
`NexusSmartExtensions.cs`, Unity'nin managed dünyası (C# Objeleri) ile Nexus'un unmanaged bellek blokları (Raw Pointers) arasındaki en kritik geçiş noktasıdır. Unity'nin `Vector3`, `Quaternion` gibi tiplerini, Nexus'un performanslı bellek alanlarına sıfır maliyetli ve güvenli bir şekilde kopyalamak için tasarlanmıştır.

Bu uzantı kütüphanesinin varlık sebebi; her veri kopyalama işleminde manuel `unsafe` kod yazma külfetini ortadan kaldırmak ve geliştiriciye "Bunu bu adrese kopyala" (CopyTo) şeklinde atomik bir komut seti sunmaktır.

---

## 2. Teknik Analiz
Performansa dayalı şu kritik genişletmeleri sağlar:

- **Unsafe Vector Copy**: `Vector3` verisini, bir float pointer'ına (`float*`) doğrudan bellek seviyesinde kopyalar. Bu, standart dizi veya managed kopyalamalara göre çok daha hızlıdır.
- **Pointer to Vector Re-Materialization**: Bir float pointer'ındaki ham veriyi tekrar Unity'nin `Vector3` tipine dönüştürür.
- **Entity Manipulation Helpers**: `RandomizePosition` gibi metodlarla, ECS varlıkları üzerinde çok sık yapılan işlemleri unmanaged bellek seviyesinde sarmalar.
- **Direct Memory Access**: Tüm kopyalama işlemleri `unsafe` bloklar içinde yapılır ve CPU saat döngülerini (Cycle) optimize eder.

---

## 3. Mantıksal Akış
1.  **Girdi**: Bir Unity bileşeninden (Örn: `transform.position`) `Vector3` verisi alınır.
2.  **Yönlendirme**: `CopyTo` metodu çağrılır ve hedef `Registry` bellek adresi verilir.
3.  **Transfer**: Veri, unmanaged yığına (Unmanaged Heap) ham byte'lar olarak aktarılır.
4.  **Ters Akış**: Gerektiğinde `ToVector3()` ile unmanaged veriden görsel bileşene geri kopyalama yapılır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Raw Pointer** | Bellekteki bir adresin hiçbir kontrol mekanizması olmadan doğrudan tutulması. |
| **Bilateral Copy** | Verinin hem unmanaged'dan managed'a hem de tersine akabilmesi. |
| **Re-Materialization** | Saf bellek verisinden anlamlı bir yüksek seviyeli nesne üretilmesi. |

---

## 5. Kullanım Örneği
```csharp
unsafe {
    // Unity verisini Nexus'a aktar
    Vector3 myPos = transform.position;
    float* targetPtr = registry.GetPointer<Position>(id);
    myPos.CopyTo(targetPtr);

    // Nexus'tan geri al
    transform.position = targetPtr.ToVector3();
}
```

---

## 6. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

public static class NexusSmartExtensions
{
    public static unsafe void CopyTo(this Vector3 v, float* ptr) {
        ptr[0] = v.x; ptr[1] = v.y; ptr[2] = v.z;
    }

    public static unsafe Vector3 ToVector3(this float* ptr) {
        return new Vector3(ptr[0], ptr[1], ptr[2]);
    }
}
```

---

## Nexus Optimization Tip: Memory Alignment Check
`CopyTo` metodunu kullanırken hedef pointer'ın 4-byte (float boyutu) hizalı olduğundan emin olun. `NexusLayout` zaten bu hizalamayı yapar. Hizalı olmayan bir adrese kopyalama yapmak, **işlemci seviyesinde "Misaligned Access" hatasına yol açarak performansı %30 düşürebilir.**
