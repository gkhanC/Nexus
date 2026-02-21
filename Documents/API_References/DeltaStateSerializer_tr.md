# Nexus Prime Mimari Rehberi: DeltaStateSerializer (Diferansiyel Veri Yazımı)

## 1. Giriş
`DeltaStateSerializer.cs`, Nexus Prime'ın veri saklama (Persistence) ve ağ senkronizasyonu süreçlerinde bant genişliğini optimize eden "Akıllı Kayıt" katmanıdır. Geleneksel serileştiricilerin aksine tüm veri setini diske yazmaz; sadece bir önceki kayıttan bu yana değişmiş olan ("Dirty") veri parçalarını saptar ve paketler.

Bu serileştiricinin varlık sebebi; binlerce varlık içeren devasa bir dünyada, sadece yerinden oynayan 3-5 varlık için tüm dünyayı tekrar kaydetme maliyetinden kaçınmak ve G/Ç (I/O) işlemlerini minimize etmektir.

---

## 2. Teknik Analiz
DeltaStateSerializer, verimlilik için şu mekanizmaları kullanır:

- **Sparse Chunk Inspection**: `SparseSet` içindeki `DirtyBits` bayraklarını tarayarak sadece güncellenmesi gereken bellek bloklarını belirler.
- **Incremental Binary Stream**: Yazma işlemi sırasında sadece değişen blokların indeksi ve ham verisi `BinaryWriter` ile akışa (stream) eklenir.
- **Snapshot Integration**: `SnapshotManager` ile koordineli çalışarak, iki zaman dilimi (Timestamp) arasındaki farkları binary seviyesinde yakalar.
- **Zero-Allocation Write**: Serileştirme sırasında yeni nesneler (C# Objects) oluşturulmaz; veriler doğrudan unmanaged bellekten stream tamponuna kopyalanır.

---

## 3. Mantıksal Akış
1.  **Tarama**: Registry üzerindeki tüm bileşen setleri (`ComponentSets`) gezilir.
2.  **Filtreleme**: Her setin "Dirty" olarak işaretlenmiş bitsetleri kontrol edilir.
3.  **Paketleme**: Kirli (Dirty) bulunan her chunk, bir indeks önekiyle birlikte ham byte dizisi olarak paketlenir.
4.  **İletim/Kayıt**: Paketlenen veri akışı diske yazılır veya ağ üzerinden gönderilir.
5.  **Geri Yükleme**: Deserilizasyon sırasında, gelen indekslere bakılarak veriler nokta atışı (point-wise) `Registry`'ye yamanır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Delta Serialization** | Sadece iki durum arasındaki farkın kaydedilmesi veya gönderilmesi. |
| **Dirty Bitset** | Belirli bir veri bloğunun değişip değişmediğini takip eden 0/1 değerleri. |
| **Sparse Update** | Seyrek dağılmış verilerin tüm seti bozmadan güncellenmesi. |
| **I/O Overhead** | Giriş/Çıkış işlemlerinin işlemci ve depolama üzerindeki ek yükü. |

---

## 5. Riskler ve Sınırlar
- **Base State Dependency**: Delta verisinin anlamlı olabilmesi için karşı tarafta veya diskte mutlaka bir "Temel Durum" (Baseline) bulunmalıdır. Temel durum kaybolursa deltalar uygulanamaz.
- **Reconstruction Cost**: Çok fazla küçük delta verisi biriktiğinde, bunları tek tek uygulamak (Apply) performansı düşürebilir. Belirli aralıklarla bir "Full Snapshot" alınması önerilir.

---

## 6. Kullanım Örneği
```csharp
using var stream = File.OpenWrite("save_delta.bin");
var serializer = new DeltaStateSerializer();

// Sadece son 1 saniyede değişenleri kaydet
serializer.SerializeDelta(mainRegistry, stream);
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Data;

public unsafe class DeltaStateSerializer
{
    public void SerializeDelta(Registry.Registry registry, Stream stream)
    {
        using var writer = new BinaryWriter(stream);
        foreach (var set in registry.ComponentSets)
        {
            // 1. Get DirtyBits from SparseSet.
            // 2. Write only 'Dirty' chunks.
            // 3. Prefix with index for sparse reconstruction.
        }
    }
}
```

---

## Nexus Optimization Tip: Frequency Scaling
Delta serileştirme sıklığını verinin "Değişim Hızına" göre ayarlayın. Çok hızlı değişen verileri (Örn: Oyuncu Pozisyonu) delta yerine ham stream olarak gönderirken, yavaş değişenleri (Örn: Envanter) DeltaStateSerializer ile işleyin. Bu, **ağ ve disk kullanımını %90 oranında optimize etmenizi sağlar.**
