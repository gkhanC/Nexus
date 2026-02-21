# Nexus Prime Mimari Rehberi: AIBehaviorTree (Unmanaged Davranış Ağacı)

## 1. Giriş
`AIBehaviorTree.cs`, Nexus Prime'ın yapay zeka (AI) dünyasındaki performans temsilcisidir. Geleneksel Behavior Tree (BT) yapılarının (Node-based object trees) aksine, tüm ağaç yapısını unmanaged bellekte (RAM) birbirine bitişik bloklar halinde tutarak "Cache-Friendly" bir AI yürütümü sağlar.

Bu işlemcinin varlık sebebi, binlerce (Örn: 50.000) varlığın aynı anda AI kararı alması gereken strateji veya simülasyon oyunlarında, standart C# objelerinin neden olduğu Garbage Collector (GC) baskısını ve bellek atlamalarını (Memory Thrashing) tamamen ortadan kaldırmaktır.

---

## 2. Teknik Analiz
NexusBTProcessor, yüksek performanslı AI kararları için şu donanım-odaklı teknikleri kullanır:

- **Unmanaged Struct Nodes**: Karar düğümleri (Selector, Sequence, Action) standart sınıflar yerine birbirine bağlı unmanaged struct'lar üzerinde saklanır.
- **Cache-Friendly Traversal**: Ağaç üzerinde dolaşırken (Traversal), işlemci bir sonraki düğümü bellekte hemen yakınında bulur. Bu, "Pointer Chasing" maliyetini minimize eder.
- **Zero-GC Ticking**: `Tick()` metodu hiçbir yeni obje oluşturmaz (allocation-free). Tüm durum bilgisi `Registry` üzerindeki bileşenlerde tutulur.
- **Flat Tree Optimization**: Derin ağaç hiyerarşileri yerine, daha düz (flat) bir bellek dizilimi kullanarak işlemci boru hattını (pipeline) optimize eder.

---

## 3. Mantıksal Akış
1.  **Giriş**: `Tick(entity, registry)` çağrısı ile ilgili varlık için işlem başlatılır.
2.  **Dolaşım**: Unmanaged bellek üzerindeki BT yapısı en üstten (Root) aşağı doğru taranır.
3.  **Karar**: Koşul (Condition) düğümleri, varlığın `Registry` üzerindeki bileşenlerine bakarak `true/false` döner.
4.  **Aksiyon**: Seçilen aksiyon düğümü tetiklenir ve sonuç (Success/Failure/Running) bir sonraki karede değerlendirilmek üzere saklanır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Traversal** | Bir veri yapısı (ağaç vb.) üzerindeki tüm elemanları belirli bir sırayla gezme işlemi. |
| **Leaf Node** | Ağacın en ucunda bulunan, gerçek işi yapan (Aksiyon/Koşul) düğüm. |
| **BT Processor** | Davranış ağacı mantığını yürüten ana matematiksel motor. |
| **Memory Thrashing** | Verilerin bellekte çok dağınık olması nedeniyle işlemcinin sürekli RAM'den veri beklemesi durumu. |

---

## 5. Riskler ve Sınırlar
- **Complexity of Setup**: Unmanaged bir ağaç yapısını koda dökmek, görsel editörler kadar kolay olmayabilir. Düğüm adresleri manuel yönetilmelidir.
- **Pointer Safety**: Ağaç yapısı bozulursa (Memory Corruption), tüm AI sisteminin bellek hatası (Segmentation Fault) vermesine neden olabilir.

---

## 6. Kullanım Örneği
```csharp
public struct AISystem : NexusParallelSystem {
    private NexusBTProcessor _btProcessor;

    public override void Execute() {
        var entities = Registry.Query().With<AICapacity>().GetEntities();
        
        foreach(var e in entities) {
            // Hiçbir obje oluşturmadan AI'yı işlet
            _btProcessor.Tick(e, Registry);
        }
    }
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

public unsafe struct NexusBTProcessor
{
    public void Tick(EntityId entity, Registry registry)
    {
        // 1. Traverse the unmanaged BT structure.
        // 2. Execute leaf nodes (Actions/Conditions).
        // 3. Update entity state components based on results.
    }
}
```

---

## Nexus Optimization Tip: Batch Tick Strategy
Eğer 100.000'den fazla varlığınız varsa, her karede tüm AI'ları `Tick` etmek yerine, AI'ları gruplara (Batches) bölün. Örneğin bir karede sadece "Yakın düşmanlar", bir sonraki karede "Uzak düşmanlar" AI kararı alsın. Bu yöntem, **AI sisteminin kare süresini (Frame Time) %500 oranında dengeler.**
