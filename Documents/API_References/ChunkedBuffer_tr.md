# Nexus Prime Mimari Rehberi: ChunkedBuffer (Sayfalı Bellek Yönetimi)

## 1. Giriş
`ChunkedBuffer.cs`, Nexus Prime'ın "Bellek Kararlılığı" (Memory Stability) garantisidir. Standart unmanaged diziler (`T*`) büyütüldüğünde (realloc), tüm veriler yeni bir bellek adresine taşınır ve bu durum eski pointerların (geçersiz adres işaret edenler) sistemi çökertmesine yol açar.

ChunkedBuffer, verileri **16KB'lık sabit sayfalar (Chunks)** halinde tutarak bu sorunu çözer. Buffer büyüse dahi eski sayfaların adresi değişmez. Bu özellik, yüksek performanslı sistemlerin veriye doğrudan pointerlar üzerinden güvenle "ebediyen" (veya bileşen silinene kadar) bakmasını sağlar.

---

## 2. Teknik Analiz
ChunkedBuffer, bellek hiyerarşisinde şu ileri teknikleri uygular:

- **Segmented Allocation**: Bellek tek bir devasa blok yerine, 16KB'lık (işletim sistemi dostu) parçalar halinde yönetilir.
- **Pointer Stability**: `Expand()` işlemi sadece yeni bir sayfa ekler; mevcut sayfaların RAM üzerindeki fiziksel yerini değiştirmez.
- **O(1) Address Math**: Herhangi bir indeksteki verinin adresi, `Base + Header + (Index * Size)` formülüyle modüler aritmetik kullanılarak anında hesaplanır.
- **Cache-Line Padding**: Her chunk'ın başlangıcı 64 byte (ALIGNMENT) ile hizalanır. Bu, ilk elemanın işlemci önbelleğine (L1 Cache) kusursuz yerleşmesini garanti eder.

---

## 3. Mantıksal Akış
1.  **Hesaplama**: Tipin (`T`) boyutuna bakılarak bir 16KB (16384 byte) içine kaç eleman sığacağı (`_elementsPerChunk`) belirlenir.
2.  **Adresleme (`GetPointer`)**: `Index / ElementsPerChunk` ile hangi sayfada olduğu, `Index % ElementsPerChunk` ile sayfa içindeki ofseti bulunur.
3.  **Genişleme (`Expand`)**: Kapasite dolduğunda yeni bir 16KB'lık blok `NativeMemory.AlignedAlloc` ile tahsis edilir ve ana tabloya (`_chunks`) eklenir.
4.  **Temizlik**: `Dispose` çağrıldığında, ana tablodaki her bir sayfa pointerı tek tek serbest bırakılır (Hardware-safe cleanup).

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Pointer Stability** | Verinin bellek adresinin, yapı büyüse dahi değişmemesi durumu. |
| **Memory Paging** | Büyük verinin sabit boyutlu küçük sayfalara bölünmesi (OS seviyesi teknik). |
| **AlignedAlloc** | Belleğin belirli bir sayının (örn: 64) katı olan adreslerden başlatılarak tahsis edilmesi. |
| **Fragmentation** | Bellekteki kullanılmayan küçük, verimsiz boşluklar. ChunkedBuffer bunu kontrol altında tutar. |

---

## 5. Riskler ve Sınırlar
- **Internal Waste**: 16KB'lık blokların sonunda tip boyutuna göre küçük boşluklar kalabilir. (Nexus bu maliyeti pointer stabilitesi için kabul eder).
- **Manual Lifetime**: Unmanaged bir yapı olduğu için mutlaka `Dispose` edilmelidir, aksi halde RAM dolana kadar boşalmaz.

---

## 6. Kullanım Örneği
```csharp
var buffer = new ChunkedBuffer<Velocity>(1024);

// Pointerı al
Velocity* vPtr = (Velocity*)buffer.GetPointer(500);

// Buffer'ı büyüt (Pointer stabilitesini bozmaz!)
buffer.Count = 5000;

// vPtr hala geçerli bir adresi göstermeye devam eder!
vPtr->X = 10.0f;
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Runtime.InteropServices;
namespace Nexus.Core;

public unsafe class ChunkedBuffer<T> : IDisposable where T : unmanaged
{
    private const int CHUNK_SIZE = 16 * 1024; 
    private const int ALIGNMENT = 64; 
    private readonly int _elementsPerChunk;
    private void** _chunks; 
    private int _chunkCount;
    private int _count;

    public void* GetPointer(int index)
    {
        int chunkIdx = index / _elementsPerChunk;
        int offset = index % _elementsPerChunk;
        byte* chunkBase = (byte*)_chunks[chunkIdx];
        return chunkBase + ALIGNMENT + (offset * sizeof(T));
    }

    private void Expand()
    {
        void* newChunk = NativeMemory.AlignedAlloc(CHUNK_SIZE, ALIGNMENT);
        NativeMemory.Clear(newChunk, CHUNK_SIZE);
        _chunks[_chunkCount++] = newChunk;
    }
    
    // ... Disposal logic
}
```

---

## Nexus Optimization Tip: Addressing Arithmetic
ChunkedBuffer'ın adres hesaplama mantığı (`Shift` ve `AND` operasyonlarına dönüştürülebilen bölme/mod işlemleri), modern CPU'ların ALU (Aritmetik Lojik Birim) üzerinde sadece **1 veya 2 clock cycle** sürer. Geleneksel dinamik dizi yeniden tahsisatı (allocation) ise **binlerce cycle ve ağır bellek taşıma (memcpy)** maliyeti yükler.
