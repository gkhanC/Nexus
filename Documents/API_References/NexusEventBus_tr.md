# Nexus Prime Mimari Rehberi: NexusEventBus (Mesajlaşma Omurgası)

## 1. Giriş
`NexusEventBus.cs`, Nexus Prime framework'ünün "Sinir Sistemi"dir. Bağımsız sistemler (Örn: ECS Simülasyonu, Unity UI, Network Katmanı) arasındaki iletişimi, birbirlerinden haberleri olmadan (Decoupled) sağlayan yüksek performanslı bir dağıtım merkezidir.

Bu otobüsün varlık sebebi; "A sistemi B sistemini tanımalıdır" (Tight Coupling) zorunluluğunu ortadan kaldırarak, veriye dayalı reaktif bir mimari sunmaktır. `INexusEvent` arayüzünü kullanan her türlü veri yapısı bu sistem üzerinden ışık hızında dağıtılabilir.

---

## 2. Teknik Analiz
Maksimum esneklik ve performans için şu çok modlu yapıları sunar:

- **Global Pub/Sub**: Her yerden dinlenebilen ve tetiklenebilen standart olaylar. `ConcurrentDictionary` ile thread-safe yönetim sağlar.
- **Local (Per-Entity) Events**: Sadece belirli bir varlığı (EntityId) ilgilendiren olaylar. Örn: "Bu düşman hasar aldı" bilgisini sadece o düşmana bağlı UI komponentine iletir.
- **Buffered Publishing**: Henüz abonesi olmayan olayları kuyruğa alır (Buffer) ve ilk abone geldiğinde ona püskürtür.
- **Debounced Publishing**: Belirli bir zaman aralığında aynı olayın binlerce kez tetiklenmesini engeller (Örn: UI update sinyalleri).
- **Unity-to-Entity Resolution**: Unity `GameObject` veya `Component` referanslarını otomatik olarak `EntityId`'ye dönüştürerek hibrit projelerde kodlamayı basitleştirir.

---

## 3. Mantıksal Akış
1.  **Abonelik (Subscribe)**: Bir sistem "Ben `PlayerSpawnEvent` ile ilgileniyorum" diyerek kendini kaydeder.
2.  **Yayın (Publish)**: Diğer bir sistem olayı fırlatır.
3.  **Filtreleme**: Olay lokalse (Local) sadece hedef varlığın abonelerine, globalse tüm ilgili sistemlere dağıtılır.
4.  **Yürütüm**: Callback metodları (Delegate) sırayla çağrılır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Decoupling** | Sistemlerin birbirinin iç yapısını bilmeden iletişim kurabilmesi. |
| **Reactive Logic** | Bir olay gerçekleştiğinde (Örn: Hasar alma) buna otomatik tepki verilmesi. |
| **Event Debouncing** | Kısa bir süre içindeki mükerrer olayların tek bir işleme indirilmesi. |
| **Unified Identity** | Unity nesneleri ile ECS varlıklarının aynı ID sistemi üzerinden tanınması. |

---

## 5. Kullanım Örneği
```csharp
// Global Abone
NexusEventBus.Subscribe<PlayerDiedEvent>(e => Debug.Log("Oyuncu öldü!"));

// Local Abone (Sadece bu nesneye gelen mesajları dinle)
NexusEventBus.SubscribeLocal<DamageEvent>(myEntityId, e => ShowDamageNumbers(e.Amount));

// Yayınlama
NexusEventBus.Publish(new PlayerDiedEvent { Time = DateTime.Now });
```

---

## 6. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Communication;

public static class NexusEventBus
{
    private static readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();

    public static void Subscribe<T>(Action<T> handler) where T : INexusEvent {
        // Safe lock & add logic...
    }

    public static void Publish<T>(T @event) where T : INexusEvent {
        // Broadcast to all subscribers...
    }

    public static void PublishLocal<T>(EntityId id, T @event) where T : INexusEvent {
        // Precise target delivery...
    }
}
```

---

## Nexus Optimization Tip: Handler Copying
`Publish` işlemi sırasında abone listesini kopyalayarak (`ToArray`) yürütün. Bu, bir event handler içindeyken yeni bir `Subscribe` veya `Unsubscribe` yapıldığında oluşabilecek "Collection Modified" hatalarını önler ve **thread-safe okuma performansını artırır.**
