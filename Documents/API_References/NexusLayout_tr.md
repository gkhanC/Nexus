# Nexus Prime Mimari Rehberi: NexusLayout (Bellek Hizalama ve Önbellek Optimizasyonu)

## 1. Giriş
`NexusLayout.cs`, Nexus Prime framework'ünün donanım seviyesindeki hassasiyetini belirleyen en temel yardımcı sınıftır. Modern işlemciler, bellekten veri okurken "Cache Line" denilen (genellikle 64 byte) bloklar halinde veri çekerler.

Bu sınıfın varlık sebebi, tüm unmanaged bellek tahsisatlarının (allocations) ve bileşen dizimlerinin bu 64-byte sınırlarına tam uyumlu (aligned) olmasını sağlamaktır. Bu sayede, tek bir veriyi okumak için işlemcinin gereksiz yere iki farklı önbellek satırını taramasının (Cache Line Split) önüne geçilir.

---

## 2. Teknik Analiz
NexusLayout, bellek performansı için şu donanım kurallarını uygular:

- **64-Byte Boundary Rule**: İşlemci MMU (Memory Management Unit) için en optimize adresleme olan 64-byte sınırlarını kullanır.
- **Aligned Allocation**: `NativeMemory.AlignedAlloc` kullanarak, işletim sisteminden alınan bellek adresinin tam olarak 64'ün katı olmasını garanti eder.
- **Size Padding**: Bir veri yapısının boyutu 64'ün tam katı değilse, `GetAlignedSize` fonksiyonu ile bir sonraki güvenli sınıra yuvarlanır.
- **MMU Efficiency**: Sayfa hizalı (Page Aligned) ve Önbellek hizalı (Cache Aligned) bellek kullanımı, işlemcinin TLB (Translation Lookaside Buffer) isabet oranını artırır.

---

## 3. Mantıksal Akış
1.  **Hesaplama**: İhtiyaç duyulan bellek miktarı alınır.
2.  **Hizalama**: `GetAlignedSize` ile en yakın 64-byte katına yuvarlama yapılır.
3.  **Tahsisat**: `NativeMemory` üzerinden donanım destekli hizalı tahsisat gerçekleştirilir.
4.  **Serbest Bırakma**: Hizalı bellek özel bir metodla (`AlignedFree`) iade edilir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Cache Line (64B)** | İşlemcinin RAM'den tek bir hamlede çektiği veri miktarı. |
| **Memory Alignment** | Verinin bellek adresinin belirli bir sayının katı olma zorunluluğu. |
| **Cache Line Split** | Bir verinin iki farklı önbellek satırına taşması sonucu oluşan performans kaybı. |
| **MMU** | Sanal bellek adreslerini fiziksel RAM adreslerine dönüştüren donanım birimi. |

---

## 5. Riskler ve Sınırlar
- **Memory Overhead**: Küçük verileri 64-byte hizalamak, bellekte "Padding" (boşluk) oluşmasına ve dolayısıyla RAM kullanımının bir miktar artmasına sebep olur. Performans için bellekten feragat edilir.
- **Unaligned Access**: Eğer `NexusLayout` dışında manuel bir tahsisat yapılırsa, işlemci "Misaligned Access" hatası verebilir veya performansı %50 düşürebilir.

---

## 6. Kullanım Örneği
```csharp
// 100 byte'lık bir alan için hizalı boyut hesapla
int rawSize = 100;
int alignedSize = NexusLayout.GetAlignedSize(rawSize); // 128 döner

// Hizalı bellek tahsis et
unsafe {
    void* ptr = NexusLayout.Alloc(alignedSize);
    // ... Kullanım ...
    NexusLayout.Free(ptr);
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

public static class NexusLayout
{
    public const int CACHE_LINE_SIZE = 64;

    public static int GetAlignedSize(int size)
    {
        return (size + CACHE_LINE_SIZE - 1) & ~(CACHE_LINE_SIZE - 1);
    }

    public static unsafe void* Alloc(int size)
    {
        return NativeMemory.AlignedAlloc((nuint)size, CACHE_LINE_SIZE);
    }

    public static unsafe void Free(void* ptr)
    {
        if (ptr != null) NativeMemory.AlignedFree(ptr);
    }
}
```

---

## Nexus Optimization Tip: Cache Boundary Safety
`NexusLayout` kullanımı, işlemcinin veriye ulaşma süresini **saat döngüsü (cycle) bazında minimize eder.** Hizalanmamış bellek, L1 önbelleğinde "Stall" oluşmasına neden olurken, `NexusLayout` ile tahsis edilen veriler işlemci boru hattından (pipeline) hiç takılmadan akar.
