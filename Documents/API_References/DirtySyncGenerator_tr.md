# Nexus Prime Mimari Rehberi: DirtySyncGenerator (Otomatik Senkronizasyon İşçisi)

## 1. Giriş
`DirtySyncGenerator.cs`, Nexus Prime'ın unmanaged verilerini Unity'nin `Transform` bileşenlerine en yüksek paralellikte aktarmak için tasarlanmış bir "İş Üreticisi" (Job Generator) taslağıdır. Standart `Update` döngüsü yerine Unity'nin `C# Job System` ve `TransformAccessArray` mimarisini kullanarak senkronizasyon maliyetini birden fazla CPU çekirdeğine dağıtır.

Bu üreticinin varlık sebebi; binlerce varlığın pozisyonunu senkronize ederken ana thread'i (Main Thread) kilitlememek ve Unity'nin "Internal Physics/Transform" sistemine en hızlı yolu kullanarak veri basmaktır.

---

## 2. Teknik Analiz
Senkronizasyonun hızlanması için şu stratejileri öngörür:

- **Dirty Bit-Sweep**: `SparseSet` üzerindeki kirli bayrakları (Dirty Flags) bir süpürme (sweep) mantığıyla tarayarak sadece değişen pozisyon verilerini saptar.
- **Parallel Execution**: Değişim verisi saptandıktan sonra, Unity'nin `IJobParallelForTransform` arabirimini kullanarak verileri paralel iş parçacıklarında Transform'lara yazar.
- **Burst Compatibility**: Kod yapısı Unity Burst derleyicisiyle tam uyumlu olacak şekilde (blittable veri kullanımı) optimize edilir.
- **Selective Dispatch**: Her karede her şeyi senkronize etmek yerine, sadece görsel olarak o an değişmesi gereken varlıkları "Dispatch" eder.

---

## 3. Mantıksal Akış
1.  **Analiz**: Unmanaged Registry üzerindeki Pozisyon bileşeninin kirli bayrak dizisi taranır.
2.  **Haritalama**: Değişen unmanaged veriler ile bunlara karşılık gelen Unity Transform'ları hızlı bir dizide (AccessArray) eşleştirilir.
3.  **Yürütüm (Job)**: Unity Job System tetiklenerek veriler arka planda (Worker Threads) kopyalanır.
4.  **Finalizasyon**: Senkronizasyon bittiğinde kirli bayraklar temizlenir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **C# Job System** | Unity içinde ağır hesaplama işlerini paralel çekirdeklere dağıtan yapı. |
| **TransformAccessArray** | Unity Transform verilerine toplu ve performanslı erişim sağlayan özel veri yapısı. |
| **Bit-Sweep** | Bellekteki bit bayraklarının hızlı bir şekilde baştan sona taranması. |
| **Sync Dispatch** | Veri kopyalama işleminin yürütülmek üzere bir işçi kuyruğuna (Job Queue) gönderilmesi. |

---

## 5. Kullanım Senaryosu
Bu bileşen genellikle bir "Source Generator" veya "NexusInitializer" tarafından yönetilir. Geliştirici sadece `RunSync` metodunu her karede çağırır; gerisini sistemin paralel işçileri halleder.

---

## 6. Tam Kaynak Kod (Conceptual Implementation)

```csharp
namespace Nexus.Bridge;

public class DirtySyncGenerator : MonoBehaviour
{
    public void RunSync(Registry.Registry registry)
    {
        // 1. Get bitmask of changed positions
        // 2. Dispatch Parallel Transform Job
    }
}
```

---

## Nexus Optimization Tip: Transform-Only Update
Unity'de nesnelerin sadece pozisyonu değişiyorsa, tüm hiyerarşiyi (Scale/Rotation) güncellemek yerine sadece `Transform.position`'ı hedefleyin. `DirtySyncGenerator` ile birleşen bu kısıtlama, **Transform senkronizasyon maliyetini ek bir %20 oranında düşürebilir.**
