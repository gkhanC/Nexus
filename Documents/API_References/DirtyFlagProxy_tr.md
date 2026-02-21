# Nexus Prime Mimari Rehberi: DirtyFlagProxy (Kirli Bayrak Senkronizasyonu)

## 1. Giriş
`DirtyFlagProxy.cs`, Nexus Prime'ın "Sadece Değişenleri Gönder" felsefesinin en teknik uygulama noktasıdır. Unmanaged bileşen setlerindeki binlerce varlığı her karede tarayıp Unity'ye kopyalamak yerine, sadece son kareden beri değer değişikliği yaşamış olanların belirlenip senkronize edilmesini sağlayan yüksek performanslı bir arayüzdür.

Bu proksinin varlık sebebi; devasa simülasyonlarda (Örn: 100,000 varlık) gereksiz veri transferi maliyetini (Overhead) sıfıra yakın bir seviyeye indirmek ve CPU önbelleğini (Cache) verimli kullanmaktır.

---

## 2. Teknik Analiz
DirtyFlagProxy, performans için şu ileri seviye teknikleri kullanır:

- **Raw Bitmask Access**: `SparseSet` içindeki 1-bit-per-entity (her varlık için 1 bit) saklayan kirli bayrak dizisine (`DirtyBits`) ham pointer üzerinden erişir.
- **32-Bit Block Skipping**: Kirli bayrak dizisini bit-bit gezmek yerine 32-bit'lik (`uint`) bloklar halinde tarar. Eğer koca bir blok 0 ise (yani 32 varlık da değişmediyse), tek bir işlemle 32 varlığı atlar.
- **Bitwise Evaluation**: Blok içinde kirli olan varlığı saptamak için `(mask & (1u << bit)) != 0` bitwise kontrolünü yapar. Bu, modern işlemcilerde naniosaniye seviyesinde bir işlemdir.
- **Generic Delegate Sync**: Değişmiş olan varlığı bulduğunda, geliştirici tarafından verilen `SyncDelegate` metodunu çağırarak verinin Unity hedefine (Renderer, Transform vb.) yamanmasını sağlar.

---

## 3. Mantıksal Akış
1.  **Hazırlık**: İlgili bileşen setinin kirli veri maskesi (Dirty Mask) RAM'den çekilir.
2.  **Blok Tarama**: Maske içinde 0'dan farklı bir uint değeri aranır.
3.  **Hassas Tespit**: 32'lik grup içinde hangi bitin 1 olduğu bulunur.
4.  **Tetikleme**: Sadece "Kirli" olan varlığın unmanaged pointer'ı ve ID'si senkronizasyon callback'ine gönderilir.
5.  **Sıfırlama**: İşlem bittikten sonra maske temizlenerek bir sonraki kareye hazırlanır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Dirty Bitmask** | Bir veri grubundaki her elemanın durumunu tek bir bit ile temsil eden bellek alanı. |
| **Bitwise AND** | İki sayısal değerin bit bazlı "VE" işlemine tabi tutularak ortak bitlerin bulunması. |
| **Sparse Enumeration** | Bir kümenin sadece belirli kurallara uyan (Örn: sadece değişenler) elemanlarının gezilmesi. |

---

## 5. Kullanım Örneği
```csharp
// Renderer renklerini sadece değiştiğinde güncelle
DirtyFlagProxy<ColorComponent>.Sync(registry, (id, colorPtr) => {
    if (NexusObjectMapping.TryGet(id.Index, out var renderer)) {
        ((Renderer)renderer).material.color = colorPtr->ToColor();
    }
});
```

---

## 6. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public unsafe class DirtyFlagProxy<T> where T : unmanaged
{
    public static void Sync(Registry.Registry registry, SyncDelegate syncCallback)
    {
        SparseSet<T> set = registry.GetSet<T>();
        uint* dirtyBits = (uint*)set.GetRawDirtyBits(out int bitCount);

        for (int i = 0; i < bitCount; i++) {
            uint mask = dirtyBits[i];
            if (mask == 0) continue; // 32 varlığı tek seferde atla

            for (int bit = 0; bit < 32; bit++) {
                if ((mask & (1u << bit)) != 0) {
                    syncCallback(set.GetEntity(i * 32 + bit), set.GetComponent(i * 32 + bit));
                }
            }
            dirtyBits[i] = 0; // Maskeyi temizle
        }
    }
}
```

---

## Nexus Optimization Tip: Early Out Block Skipping
`mask == 0` kontrolü, modern ECS motorlarında "Sparse Query" performansını artıran en büyük faktördür. Eğer simülasyonun sadece %1'i karesel olarak değişiyorsa, bu kontrol sayesinde taranması gereken veri miktarı **%99 oranında azalır.**
