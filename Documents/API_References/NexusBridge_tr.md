# Nexus Prime Mimari Rehberi: NexusBridge (Jenerik Senkronizasyon Arabirimi)

## 1. Giriş
`NexusBridge.cs`, Nexus Prime'ın herhangi bir oyun motoru (Unity, Unreal, Godot vb.) ile konuşmasını sağlayan en temel ve jenerik "Protokol Katmanı"dır. Unmanaged simülasyon verisinin dış dünyaya nasıl çıkacağını (`Push`) ve dış dünya verisinin simülasyona nasıl gireceğini (`Pull`) tanımlayan yüksek performanslı bir şablondur.

Bu yapının varlık sebebi; her bileşen tipi için ayrı senkronizasyon kodları yazmak yerine, tip-güvenli (type-safe) ve bellek-verimli (memory-efficient) bir standart sunarak motor entegrasyonu maliyetini minimize etmektir.

---

## 2. Teknik Analiz
Maksimum verimlilik için şu iki ana akış modelini yönetir:

- **Push (Nexus -> Engine)**: Simülasyon bittikten sonra unmanaged Registry'deki "Kirli" (Dirty) olarak işaretlenmiş verileri dış motorun görsel nesnelerine yansıtır. Sadece değişenleri işleyerek (Sparse Iteration) CPU yükünü optimize eder.
- **Pull (Engine -> Nexus)**: Simülasyon başlamadan önce dış dünyadaki değişimleri (Örn: Kullanıcı klavye girdisi veya Unity fizik motoru sonuçları) unmanaged belleğe çeker.
- **BridgeOrchestrator**: Senkronizasyon frekansını (Örn: 30 FPS veya 60 FPS) yöneten bir zamanlayıcıdır. Simülasyon hızı ne olursa olsun görsel aktarımın stabil kalmasını sağlar.
- **Zero-Allocation Sync**: Kopyalama işlemleri Raw Pointers (`T*`) üzerinden yapıldığı için hiçbir managed nesne oluşturulmaz (GC-Free).

---

## 3. Mantıksal Akış
1.  **Iterasyon**: İlgili bileşen setindeki tüm varlıklar hızlıca taranır.
2.  **Kirli Kontrolü (Dirty Check)**: Sadece `SetDirty` ile işaretlenmiş olanlar filtrelenir.
3.  **Veri Transferi**: Delegeler (Callback) aracılığıyla veriler bellek adresleri arasında kopyalanır.
4.  **Temizlik**: Senkronizasyon sonunda kirli bayraklar temizlenir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Pull Strategy** | Verinin motordan ECS'ye akış yönü. |
| **Push Strategy** | Verinin ECS'den motora (Görselleşmeye) akış yönü. |
| **Frequency Capping** | Gereksiz işlem yükünü önlemek için senkronizasyonun belirli bir hızda sınırlandırılması. |
| **Bi-Directional Sync** | Verinin her iki yöne de güvenli bir şekilde akabildiği yapı. |

---

## 5. Kullanım Örneği
```csharp
// Manuel bir senkronizasyon örneği
NexusBridge<Position>.Push(registry, (id, posPtr) => {
    // Veriyi Unity objesine uygula
    ApplyToTransform(id, posPtr->ToVector3());
});
```

---

## 6. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public unsafe class NexusBridge<T> where T : unmanaged
{
    public static void Push(Registry.Registry registry, PushDelegate pushCallback) {
        var set = registry.GetSet<T>();
        for (int i = 0; i < set.Count; i++) {
            if (set.IsDirty((uint)i)) {
                pushCallback(set.GetEntity(i), set.GetComponent(i));
                set.ClearDirty((uint)i);
            }
        }
    }

    public static void Pull(Registry.Registry registry, PullDelegate pullCallback) {
        var set = registry.GetSet<T>();
        for (int i = 0; i < set.Count; i++) {
            pullCallback(set.GetEntity(i), set.GetComponent(i));
            set.SetDirty((uint)i);
        }
    }
}
```

---

## Nexus Optimization Tip: Selective Dirty Clearing
`Push` işlemi bittikten sonra her zaman `ClearDirty` çağırmak yerine, veriyi hem video kayıt hem de ağ (Network) sistemine gönderiyorsanız, tüm sistemler okuyana kadar bayrağı saklayın. Bu, **aynı veriyi birden fazla sistem için gereksiz yere tekrar hesaplamayı önler.**
