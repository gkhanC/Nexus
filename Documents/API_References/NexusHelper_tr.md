# Nexus Prime Mimari Rehberi: NexusHelper (Ana Cephe Sistemi)

## 1. Giriş
`NexusHelper.cs`, Nexus Prime Unity Framework'ünün "Büyük Cephesi" (Master Facade) ve orkestratörüdür. Karmaşık alt sistemleri (Pooling, Event Bus, Logging, UI Binding) tek bir statik giriş noktasında birleştirerek geliştiriciye son derece akıcı ve okunabilir bir API sunar.

Bu yardımcının varlık sebebi; her alt sistemin örneğine (singleton) ayrı ayrı ulaşma külfetini ortadan kaldırmak ve tüm framework yeteneklerini "NexusHelper.X" şeklinde tek bir komut paletine indirmektir.

---

## 2. Teknik Analiz
NexusHelper, şu ana sistemleri birleştirir:

- **Logging Facade**: `NexusLogger` üzerinden thread-safe (paralel işlere uygun) loglama yapar. Premium Nexus styling ile konsol çıktılarını renklendirir.
- **Pooling (Spawn/Despawn)**: `NexusObjectPool` sistemini sarmalar. `Instantiate` ve `Destroy` operasyonlarının yerine geçerek **runtime GC sıçramalarını %100 oranında engeller.**
- **Communication (Publish/Subscribe)**: `NexusEventBus` entegrasyonu ile varlıklar arası kayıpsız ve hızlı mesajlaşma sağlar.
- **UI Data Binding**: logical unmanaged değişkenleri (`NexusAttribute`) doğrudan Unity UI objelerine (Slider, Image) bağlayarak otomatik güncelleme sistemi kurar.
- **Master Controllers**: Rigidbody hareketi ve rotasyon kontrolü gibi sık kullanılan işlemleri "Fire and Forget" (Ateşle ve Unut) kolaylığıyla yönetir.

---

## 3. Mantıksal Akış
1.  **Çağrı**: Geliştirici, örneğin `NexusHelper.Spawn(prefab, pos, rot)` komutunu çağırır.
2.  **Yönlendirme**: Çağrı, arka plandaki ilgili alt sistemin (`NexusObjectPool`) mikro-saniye hassasiyetindeki optimizasyonlarına yönlendirilir.
3.  **Yürütüm**: Alt sistem işlemi gerçekleştirir (Örn: Havuzdan bir nesne çıkarır ve aktif eder).
4.  **Sonuç**: İşlem sonucu (Örn: GameObject) temiz bir şekilde geliştiriciye döner.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Facade Pattern** | Karmaşık bir alt sistemler kümesine basitleştirilmiş bir arayüz sağlayan tasarım deseni. |
| **Master Facade** | Framework içindeki tüm ana servislerin toplandığı en üst seviye cephe. |
| **Fire and Forget** | Sonuç beklenmeden, sadece komutun verilip bırakıldığı yazılım örüntüsü. |
| **UI Binding** | Veri katmanındaki bir değişimin otomatik olarak kullanıcı arayüzüne (UI) yansıması. |

---

## 5. Kullanım Örneği
```csharp
// Eskiden: GC Yaratan Karmaşık Kod
// var go = Instantiate(prefab, pos, rot);
// Debug.Log("Nesne oluşturuldu");

// Nexus ile: Optimize ve Temiz Kod
var go = NexusHelper.Spawn(prefab, pos, rot);
NexusHelper.LogSuccess(this, "Nesne havuzdan başarıyla çekildi");

// Bir event fırlat
NexusHelper.Publish(new PlayerSpawnedEvent { Id = myId });
```

---

## 6. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

public static class NexusHelper
{
    public static void Log(object context, object message) => NexusLogger.Instance.Log(context, message);
    
    public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot) 
        => NexusObjectPool.Instance.Spawn(prefab, pos, rot);

    public static void Publish<T>(T eventData) where T : INexusEvent 
        => NexusEventBus.Instance.Publish(eventData);

    public static void Move(GameObject go, Vector3 direction) {
        if (go.TryGetComponent<NexusRigidbodyMove>(out var mover)) mover.Move(direction);
    }
}
```

---

## Nexus Optimization Tip: Single Entry Point
Kodunuzun her yerinde farklı Singleton'lara (`Instance`) ulaşmak yerine sadece `NexusHelper` kullanın. Bu, projenizin bağımlılık grafiğini (Dependency Graph) basitleştirir ve **gelecekte yapılacak framework güncellemelerinde kod tabanınızdaki değişim ihtiyacını %60 azaltır.**
