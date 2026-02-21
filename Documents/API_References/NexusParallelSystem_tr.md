# Nexus Prime Mimari Rehberi: NexusParallelSystem (Otonom Sistem Temeli)

## 1. Giriş
`NexusParallelSystem.cs`, Nexus Prime içindeki yüksek performanslı ve otonom iş mantıklarının ana taşıyıcısıdır. Geliştiricilerin karmaşık paralel programlama detaylarıyla (thread yönetimi, kilitlenmeler vb.) uğraşmadan, sadece iş mantığına odaklanmasını sağlayan bir soyutlama (abstraction) katmanıdır.

Bu sınıfın varlık sebebi, sistemlerin ihtiyaç duyduğu veri bağımlılıklarını çalışma zamanında otomatik olarak tespit etmek ve `JobSystem`'e bu sistemin hangi güvenlik önlemleriyle (paralel mi yoksa sıralı mı) çalıştırılması gerektiğini bildirmektir.

---

## 2. Teknik Analiz
NexusParallelSystem, sistem otomasyonu için şu altyapıları sunur:

- **Reflective Dependency Extraction**: `GetAccessInfo` metodu, `Reflection` kullanarak sınıf üzerindeki `[Read]` ve `[Write]` etiketli alanları tarar. Bu, geliştiricinin manuel olarak bağımlılık listesi tutma zorunluluğunu ortadan kaldırır.
- **Dependency Inversion**: `Registry` referansı, `[Inject]` özniteliği sayesinde sistem oluşturulduğunda otomatik olarak enjekte edilir.
- **Lifecycle Hooks**: `OnCreate` ve `OnDestroy` metodları, sistemin yaşam döngüsü boyunca (kaynak tahsisatı veya temizliği için) kontrol noktaları sağlar.
- **Abstract Logic Entry**: `Execute()` metodu zorunlu (abstract) kılınarak, her sistemin net bir yürütme noktasına sahip olması garanti edilir.

---

## 3. Mantıksal Akış
1.  **İlklendirme**: Sistem `JobSystem.AddSystem` ile kaydedildiğinde, `Inject` alanları doldurulur.
2.  **Sorgulama**: `JobSystem`, `GetAccessInfo()` çağrısı yaparak sistemin hangi bileşenlere hangi modda erişeceğini öğrenir.
3.  **Çalıştırma**: `Execute()` metodu, sistemin veri katmanındaki bağımlılıkları temiz olduğunda paralel thread havuzunda tetiklenir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Autonomous System** | Kendi bağımlılıklarını bilen ve dış müdahale olmadan çalışabilen mantık birimi. |
| **Reflection** | Bir programın çalışma zamanında kendi yapısını (alanlar, metodlar vb.) inceleyebilme yeteneği. |
| **Lifecycle Hook** | Belirli bir olay gerçekleştiğinde (oluşturma, silme) çalışan geri çağırma metodu. |
| **Abstraction Layer** | Karmaşık alt sistem detaylarını gizleyerek daha basit bir kullanım sunan yapı. |

---

## 5. Riskler ve Sınırlar
- **Reflection Overhead**: Bağımlılık taraması Reflection kullandığı için `AddSystem` anında ufak bir CPU maliyeti oluşturur. Ancak bu sadece bir kez yapıldığı için oyun içi performansa etkisi yoktur.
- **Virtual Method Call**: `GetAccessInfo` sanal bir metod olduğu için çok derin kalıtım hiyerarşilerinde mikro seviyede çağrı maliyeti oluşabilir.

---

## 6. Kullanım Örneği
```csharp
public class CombatSystem : NexusParallelSystem
{
    // Bağımlılıklar otomatik enjekte edilir ve taranır
    [Write] private Health* _health;
    [Read] private AttackPower* _power;

    public override void Execute()
    {
        // ... Savaş mantığı ...
    }

    public override void OnCreate()
    {
        // Başlangıç ayarları
    }
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Reflection;
namespace Nexus.Core;

public abstract class NexusParallelSystem : INexusSystem
{
    [Inject] protected Registry Registry;
    public string Name => GetType().Name;

    public virtual (HashSet<Type> Reads, HashSet<Type> Writes) GetAccessInfo()
    {
        var reads = new HashSet<Type>();
        var writes = new HashSet<Type>();
        var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<ReadAttribute>() != null) reads.Add(field.FieldType);
            if (field.GetCustomAttribute<WriteAttribute>() != null) writes.Add(field.FieldType);
        }
        return (reads, writes);
    }

    public abstract void Execute();
    public virtual void OnCreate() { }
    public virtual void OnDestroy() { }
}
```

---

## Nexus Optimization Tip: Static Mapping vs Reflection
Eğer sisteminiz binlerce kez silinip tekrar ekleniyorsa, `GetAccessInfo` metodunu `override` ederek Reflection maliyetinden kurtulabilirsiniz. `Reads.Add(typeof(T))` şeklinde manuel bir liste dönerek, **sistem kayıt süresini %95 oranında kısaltabilirsiniz.**
