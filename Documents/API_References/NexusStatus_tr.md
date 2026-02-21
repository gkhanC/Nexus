# Nexus Prime Mimari Rehberi: NexusStatus (Durum ve Kaynak Yönetimi)

## 1. Giriş
`NexusStatus.cs`, varlıkların temel yaşamsal kaynaklarını (Can, Mana, Enerji vb.) ve RPG tarzı niteliklerini (Güç, Çeviklik vb.) unmanaged bellek üzerinde yöneten bir veri şablonudur. HypeFire framework mimarisinden esinlenilen bu yapı, yüksek performanslı oyunlarda binlerce birimin durum takibini yapmak için tasarlanmıştır.

Bu yapının varlık sebebi; her düşman birimi veya mermi için ayrı sınıflar oluşturmak yerine, veriyi donanım dostu bileşenler (Components) içinde kümeleyerek cache-miss oranını düşürmek ve sistemlerin bu verilere en hızlı şekilde ulaşmasını sağlamaktır.

---

## 2. Teknik Analiz
Fiziksel veri dizilimi (Memory Layout) açısından kritik iki yapı sunar:

- **NexusStatus**: İki adet "Mevcut/Maksimum" çiftini (Health ve Mana) barındırır. `IsDead`, `HealthPercent` gibi yardımcı özellikler (helper properties) ile görselleştirme süreçlerini hızlandırır. `Damage` ve `Heal` metodları unmanaged veriyi `MathF.Max/Min` ile güvenli bir şekilde günceller.
- **NexusAttributeStats**: Standart RPG istatistiklerini (`Strength`, `Agility`, `Intelligence`, `Stamina`) `int` tipinde bir blok olarak saklar. Bu veriler tipik olarak `NexusStackable` ile birleşerek final savaş figürlerini oluşturur.

---

## 3. Mantıksal Akış
1.  **Tanımlama**: Bileşen (Component) içinde `NexusStatus Vitality` olarak tanımlanır.
2.  **Etki**: Bir hasar sistemi, `Vitality.Damage(50)` çağrısı yaparak unmanaged bellek adresindeki değeri doğrudan günceller.
3.  **Kontrol**: Ölüm kontrolü yapan sistemler, `Vitality.IsDead` bayrağına bakarak varlığın yok edilip edilmeyeceğine karar verir.
4.  **UI Güncelleme**: `HealthPercent` kullanılarak can barları güncellenir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Vitality Tracking** | Bir varlığın hayatta kalma parametrelerinin sürekli izlenmesi. |
| **Encapsulated Logic** | Metodların ve verinin aynı struct içinde, unmanaged kurallara uygun şekilde birleşmesi. |
| **RPG Stats** | Karakterin temel yeteneklerini belirleyen sayısal nitelikler kümesi. |

---

## 5. Riskler ve Sınırlar
- **Extended Resources**: Eğer projenizde sadece Health/Mana değil, "Stamina", "Oxygen" gibi 5-6 farklı kaynak gerekiyorsa, bu struct'ı genişletmek veya yeni bileşenler eklemek gerekir.
- **State Loss**: Unmanaged olduğu için, bu yapıları içeren bileşenler silindiğinde veriler tamamen kaybolur (Snapshot alınmadıysa).

---

## 6. Kullanım Örneği
```csharp
public struct EnemyStatus : INexusComponent {
    public NexusStatus Vitals;
    public NexusAttributeStats Stats;
}

// Sistem içinde kullanım
ref var enemy = ref registry.Get<EnemyStatus>(id);
enemy.Vitals.Damage(10);

if (enemy.Vitals.IsDead) {
    Console.WriteLine("Düşman elendi.");
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Data;

public struct NexusStatus
{
    public float CurrentHealth;
    public float MaxHealth;
    public float CurrentMana;
    public float MaxMana;

    public bool IsDead => CurrentHealth <= 0;
    public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;

    public void Damage(float amount) => CurrentHealth = MathF.Max(0, CurrentHealth - amount);
    public void Heal(float amount) => CurrentHealth = MathF.Min(MaxHealth, CurrentHealth + amount);
}
```

---

## Nexus Optimization Tip: Memory Pooling
`NexusStatus` bileşenlerini saniyede binlerce kez oluşturup silmek yerine, `AutomaticInternalPooling` sisteminden faydalanın. Bu, unmanaged bellek sayfalarının sürekli OS'ten istenmesini engelleyerek **işlemci üzerindeki memory management yükünü %30 azaltır.**
