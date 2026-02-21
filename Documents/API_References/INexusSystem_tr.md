# Nexus Prime Mimari Rehberi: INexusSystem & Core Attributes (Mantık ve Bağımlılık Yönetimi)

## 1. Giriş
Nexus Prime ekosisteminde "Sistemler" (`INexusSystem`), veriden bağımsız, saf mantık konteynerleridir. Geleneksel OOP mimarisindeki "Manager" sınıflarının aksine, sistemler kendi içlerinde veri saklamazlar; bunun yerine `Registry` üzerindeki bileşen dizilerine hükmederler.

Bu arayüzün ve beraberindeki özniteliklerin (`Attributes`) varlık sebebi, çok çekirdekli (multi-threaded) çalışma ortamında hangi sistemin hangi veriye erişeceğini önceden belirlemek ve **Data Race** (Veri Yarışı) riskini donanım seviyesinde bertaraf etmektir.

---

## 2. Teknik Analiz
Sistem mimarisi, `JobSystem` orkestrasyonu için şu meta-veri etiketlerini kullanır:

- **[Read] Attribute**: Bir sistemin bir bileşen tipini sadece okuyacağını belirtir. Birden fazla sistem aynı anda aynı bileşeni OKUYABİLİR. Bu, CPU çekirdekleri arasında maksimum paralellik sağlar.
- **[Write] Attribute**: Bir sistemin veriyi değiştireceğini (exclusive access) belirtir. Bir bileşene YAZAN bir sistem varken, başka hiçbir sistem o bileşene dokunamaz.
- **[Inject] Attribute**: Bağımlılık Enjeksiyonu (DI) mekanizmasıdır. `JobSystem`, sistem ilklendirilirken `Registry` veya `EntityCommandBuffer` gibi merkezi araçları bu alana otomatik olarak enjekte eder.
- **Stateless Design**: `INexusSystem` uygulayan sınıflar durum saklamamalıdır. Tüm durum (state) bileşenlerde saklanır, bu da sistemlerin herhangi bir çekirdekte güvenle çalıştırılmasını sağlar.

---

## 3. Mantıksal Akış
1.  **Tanımlama**: Geliştirici, `INexusSystem` arayüzünü uygular ve ihtiyaç duyduğu bileşenleri `[Read]` veya `[Write]` ile işaretler.
2.  **Analiz**: `JobSystem.AddSystem` çağrıldığında, Reflection ile bu öznitelikler taranır.
3.  **Çizelgeleme**: Bağımlılık matrisi oluşturulur (Kahn Algoritması). Yazma çakışması olan sistemler farklı katmanlara (layers) ayrılır.
4.  **Yürütme**: `Execute()` metodu, işlemcinin uygun olan en boş çekirdeğinde tetiklenir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Stateless** | Önceki karelerden veri taşımayan, her çağrıda sıfırdan hesaplama yapan yapı. |
| **Dependency Injection** | Bir nesnenin ihtiyaç duyduğu referansların dışarıdan otomatik sağlanması. |
| **Exclusive Access** | Bir kaynağa aynı anda sadece tek bir işlemin erişebilmesi durumu. |
| **Metadata Tagging** | Kodun çalışma şeklini değiştiren yardımcı etiketler (Attributes). |

---

## 5. Riskler ve Sınırlar
- **Write Over-use**: Her şeyi `[Write]` olarak işaretlemek, sistemlerin paralel çalışmasını engeller ve performansı tek çekirdek (Single-thread) seviyesine düşürür.
- **Thread Safety**: Eğer sistem içinde yerel bir değişken (field) güncelleniyorsa, bu değişkenin thread-safe olduğundan emin olunmalıdır (Ancak ideal ECS'de bu yapılmamalıdır).

---

## 6. Kullanım Örneği
```csharp
public class GravitySystem : INexusSystem
{
    [Inject] private Registry _registry;
    [Read] private float _gravityForce = -9.81f;
    
    // Position bileşenine yazacağız, Velocity'yi okuyacağız
    [Write] private Position* _pos;
    [Read] private Velocity* _vel;

    public void Execute()
    {
        // ... Yerçekimi mantığı ...
    }
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

public interface INexusSystem
{
    void Execute();
}

[AttributeUsage(AttributeTargets.Field)]
public class ReadAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field)]
public class WriteAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field)]
public class InjectAttribute : Attribute { }
```

---

## Nexus Optimization Tip: Parallelism Maximization
Mümkün olan her durumda `[Write]` yerine **[Read]** kullanmalısınız. İşlemci, 100 farklı çekirdekte aynı anda "Okuma" yapabilir, ancak tek bir "Yazma" işlemi tüm boru hattını (pipeline) durdurabilir. Doğru öznitelik kullanımı, oyununuzun **paralellik verimliliğini %300-400 oranında artırabilir.**
