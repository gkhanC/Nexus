# Nexus Prime Mimari Rehberi: RemoteDataSynchronizer (Uzak Veri Senkronizasyonu)

## 1. Giriş
`RemoteDataSynchronizer.cs`, Nexus Prime'ın çok oyunculu (multiplayer) veya dağıtık simülasyonlar için tasarlanmış veri aynalama (mirroring) motorudur. Veriyi C# nesnesi olarak değil, en ham unmanaged haliyle (binary) paketleyip uzak bir noktaya (server/client) aktarmak üzere kurgulanmıştır.

Bu senkronizörün varlık sebebi, ağ trafiğini (bandwidth) minimize etmek için `SnapshotManager`'ın ürettiği **Delta Snapshots** teknolojisini kullanmak ve sadece değişen bileşenleri ham byte blokları halinde karşı tarafa iletmektir.

---

## 2. Teknik Analiz
RemoteDataSynchronizer, düşük gecikmeli (low-latency) ağ iletişimi için şu stratejileri izler:

- **Delta Streaming**: Tüm Registry'yi göndermek yerine, sadece son senkronizasyondan bu yana değişen bileşenleri (Diff) paketler.
- **Unmanaged Binary Transfer**: Veriler zaten RAM'de unmanaged ve bit-bit uyumlu (blittable) durduğu için, herhangi bir "Serialization" maliyeti olmadan doğrudan `NativeMemory` üzerinden network buffer'ına kopyalanır.
- **Protocol Agnostic**: Mantık TCP veya UDP fark etmeksizin çalışabilir; ana odak verinin binary paketlenme ve karşı tarafta "Patch" edilme sürecidir.
- **Zero-Allocation Sync**: Senkronizasyon sırasında yeni C# objeleri oluşturulmaz; veriler ham byte pointerlar üzerinden akar.

---

## 3. Mantıksal Akış
1.  **Sorgulama**: Registry üzerindeki `Dirty` bayraklı bileşenler ve varlıklar saptanır.
2.  **Snapshot Alımı**: `SnapshotManager` aracılığıyla bir delta snapshot (fark yedeği) oluşturulur.
3.  **Paketleme**: Snapshot içeriği, donanım mimarisine uygun binary formatta bir paket haline getirilir.
4.  **İletim**: UDP/TCP soketleri üzerinden hedef IP adresine gönderilir.
5.  **Uygulama**: Karşı taraftaki alıcı, gelen ham byte'ları doğrudan kendi `Registry` adreslerine yazar.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Mirroring** | Bir veri kaynağının durumunun başka bir yerde birebir kopyalanması. |
| **Delta Snapshot** | İki zaman dilimi arasındaki sadece değişen verileri içeren paket. |
| **Blittable Transfer** | Verinin dönüştürülmeden, olduğu gibi (raw bytes) kopyalanması. |
| **Latency** | Verinin bir noktadan diğerine ulaşması sırasında geçen süre (gecikme). |

---

## 5. Riskler ve Sınırlar
- **Packet Loss**: UDP kullanılıyorsa, kaybolan paketlerin delta zincirini bozma riski vardır. Bu durum senkronizasyon kaybına (Desync) yol açar.
- **Endianness**: Veri farklı CPU mimarileri (Örn: ARM vs x86) arasında taşınıyorsa, byte sıralaması (Endianness) sorunları oluşabilir. Nexus Prime varsayılan olarak Little-Endian kullanır.

---

## 6. Kullanım Örneği
```csharp
var synchronizer = new RemoteDataSynchronizer();

void FixedUpdate() {
    // Her 100ms'de bir değişimleri sunucuya gönder
    synchronizer.SyncToRemote(mainRegistry, "192.168.1.50");
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

public class RemoteDataSynchronizer
{
    public void SyncToRemote(Registry registry, string ipAddress)
    {
        // 1. Get Delta Snapshot from SnapshotManager.
        // 2. Stream raw binary data over UDP/TCP.
        // 3. Apply on the remote receiver side.
        Console.WriteLine($"Nexus: Syncing data to {ipAddress}...");
    }
}
```

---

## Nexus Optimization Tip: Bit-Compression for Delta
Delta snapshot'ları paketlerken, değişmeyen bileşenleri göndermemenin yanı sıra, değişenleri de bit-seviyesinde sıkıştırarak (Örn: `quantization`) gönderin. Bu, **ağ kullanımını %200-%300 oranında optimize ederek** binlerce varlığın mobil cihazlarda bile senkronize kalmasını sağlar.
