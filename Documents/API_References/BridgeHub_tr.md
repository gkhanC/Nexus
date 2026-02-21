# Nexus Prime Mimari Rehberi: BridgeHub (Merkezi Köprü Merkezi)

## 1. Giriş
`BridgeHub.cs`, Nexus Prime'ın unmanaged simülasyon dünyası ile dış motorlar (Unity, Unreal vb.) arasındaki tüm veri trafiğini yöneten "Merkezi Santral"dir. Farklı bileşen tipleri için tanımlanmış senkronizasyon mantıklarını tek bir yerde toplar ve her karede (frame) bu mantıkların doğru sırada çalıştırılmasını orkestre eder.

Bu hub'ın varlık sebebi; her sistemin kendi başına senkronizasyon yapıp karmaşaya (Race Condition) yol açmasını engellemek ve veri akışını "Pull" (Motordan Nexus'a) ve "Push" (Nexus'tan Motora) şeklinde iki ana disipline ayırarak standartlaştırmaktır.

---

## 2. Teknik Analiz
BridgeHub, veri akışını şu iki ana kanal üzerinden yönetir:

- **Pull (Engine -> Nexus)**: Karenin başında (Start of Frame) çağrılır. Unity tarafındaki girdi veya fizik değişimlerini unmanaged `Registry`'ye çeker.
- **Push (Nexus -> Engine)**: Karenin sonunda (End of Frame) çağrılır. Simülasyon sonucunda değişen unmanaged verileri (Örn: AI sonucu pozisyonlar) Unity görsel nesnelerine iter.
- **Action Decoupling**: Senkronizasyon mantıklarını `Action` listelerinde saklayarak, Registry'nin iç yapısını bilmeden jenerik bir yürütüm sağlar.
- **Dirty Check Integration**: Kaydı yapılan her bileşen için opsiyonel "Kirli Kontrolü" (Dirty Check) desteği sunarak sadece değişen verilerin işlenmesini sağlar.

---

## 3. Mantıksal Akış
1.  **Kayıt (Register)**: Uygulama başlarken hangi bileşenlerin nasıl senkronize edileceği Hub'a bildirilir.
2.  **Girdi İşleme (PullAll)**: Unity dünyasındaki tüm güncellemeler tek bir pasla Nexus'a aktarılır.
3.  **Simülasyon**: Nexus unmanaged sistemleri veriyi işler.
4.  **Görselleştirme (PushAll)**: Değişen sonuçlar tekrar Unity dünyasına (Transform, VFX vb.) püskürtülür.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Push/Pull Discipline** | Veri akışının tek yönlü ve belirli zaman dilimlerinde (Phase) yapılmasını sağlayan kural seti. |
| **Sync Logic Registration** | Bir bileşen tipinin nasıl kopyalanacağının Hub'a önceden tanımlanması. |
| **Orchestration** | Birden çok bağımsız sistemin belirli bir uyum içinde sırayla çalıştırılması. |

---

## 5. Kullanım Örneği
```csharp
var hub = new BridgeHub(registry);

// Pozisyon verisini Nexus'tan Unity'ye itecek mantığı kaydet
hub.Register<Vector3>(
    push: (id, ptr) => NexusSyncManager.SyncEntity(registry, id, GetGO(id)),
    pull: (id, ptr) => *ptr = GetGO(id).transform.position
);

// Frame Loop
void Update() {
    hub.PullAll(); // Unity -> Nexus
    // ... Simülasyon ...
    hub.PushAll(); // Nexus -> Unity
}
```

---

## 6. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public class BridgeHub
{
    private readonly List<Action> _pushActions = new();
    private readonly List<Action> _pullActions = new();

    public void Register<T>(PushDelegate push, PullDelegate pull) where T : unmanaged {
        if (push != null) _pushActions.Add(() => NexusBridge<T>.Push(_registry, push));
        if (pull != null) _pullActions.Add(() => NexusBridge<T>.Pull(_registry, pull));
    }

    public void PullAll() { foreach (var a in _pullActions) a(); }
    public void PushAll() { foreach (var a in _pushActions) a(); }
}
```

---

## Nexus Optimization Tip: Batch Registration
Sık kullanılan bileşenleri (Transform, Health vb.) tek bir toplu kayıt sistemi (Batch) üzerinden Hub'a kaydedin. Bu, her karede yüzlerce `Action` delegesini tek tek çağırmak yerine, daha büyük bellek bloklarını tek seferde işlemek için gereken altyapıyı hazırlar.
