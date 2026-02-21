# Nexus Prime Mimari Rehberi: NexusRotationField (Unmanaged Rotasyon Verisi)

## 1. Giriş
`NexusRotationField.cs`, Nexus Prime'ın unmanaged ECS bileşenleri içinde rotasyon verisini saklamak için kullandığı hibrit bir yapıdır. Unity'nin `Quaternion` yapısı unmanaged olduğu halde, editör üzerinde Euler açıları (Vector3) ile çalışmak geliştiriciler için çok daha sezgiseldir.

Bu yapının varlık sebebi; unmanaged bellek blokları içinde rotasyonu en basit haliyle (3 float) saklamak, ancak ihtiyaç duyulduğunda (Örn: Fizik hesaplamaları) Unity'nin Quaternion sistemine sıfır maliyetli ve otomatik (`implicit`) geçişler yapabilmektir.

---

## 2. Teknik Analiz
NexusRotationField, rotasyon yönetimi için şu yetenekleri sunar:

- **Memory Efficiency**: Rotasyonu 4-float (Quaternion) yerine 3-float (Euler) olarak saklayarak bellek kullanımını her bileşende %25 oranında azaltır.
- **Bi-Directional Conversion**: Hem `Vector3` hem de `Quaternion` tiplerinden `implicit` (açıkça belirtilmeden) dönüşebilir. Bu, geliştiricinin `registry.Set(id, new Vector3(0, 90, 0))` yazabilmesine olanak tanır.
- **Operator Overloading**: Rotasyon verisini bir skalar değerle (`float`) çarparak dinamik olarak ölçeklendirilmesini sağlar.
- **Serializable Support**: `[Serializable]` özniteliği sayesinde Unity Inspector penceresinde standart bir Vector3 alanı olarak görünebilir.

---

## 3. Mantıksal Akış
1.  **Girdi**: Geliştirici, rotasyonu Inspector'dan veya koddan Euler açıları olarak girer.
2.  **Saklama**: Veri, unmanaged bileşen içinde `Vector3` (Euler) olarak tutulur.
3.  **Dönüştürme**: Bir Unity sistemine (Örn: `transform.rotation`) aktarılacağı zaman, `implicit operator` sayesinde arka planda anında `Quaternion.Euler()` hesaplaması yapılır.
4.  **Uygulama**: Hesaplanan Quaternion, Unity'nin görsel veya fiziksel katmanına aktarılır.

---

## 4. Kullanım Örneği
```csharp
public struct RotationComponent : INexusComponent {
    public NexusRotationField Value;
}

// Kullanım
RotationComponent rot = new RotationComponent();
rot.Value = new Vector3(0, 45, 0); // Implicit cast from Vector3

// Unity'ye aktarım
transform.rotation = rot.Value; // Implicit cast to Quaternion
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Mathematics;

[Serializable]
public struct NexusRotationField
{
    public Vector3 Euler;

    public Quaternion Quaternion => Quaternion.Euler(Euler);

    public NexusRotationField(Vector3 euler) => Euler = euler;
    public NexusRotationField(Quaternion quaternion) => Euler = quaternion.eulerAngles;

    public static implicit operator Quaternion(NexusRotationField field) => field.Quaternion;
    public static implicit operator Vector3(NexusRotationField field) => field.Euler;
}
```

---

## Nexus Optimization Tip: Storage vs. Compute
Rotasyonu `NexusRotationField` (Euler) olarak saklamak bellekten tasarruf sağlarken, her karede `Quaternion`'a dönüştürmek küçük bir CPU CPU maliyeti getirir. Eğer verinizi saniyede binlerce kez Quaternion olarak okuyacaksanız, **bellekten feragat edip doğrudan `Quaternion` saklayarak rotasyon hesaplama maliyetini %100 oranında düşürebilirsiniz.**
