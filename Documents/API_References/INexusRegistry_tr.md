# Nexus Prime Mimari Rehberi: INexusRegistry (Varlık ve Bileşen Yönetim Sözleşmesi)

## 1. Giriş
`INexusRegistry.cs`, Nexus Prime ECS mimarisinin kalbi olan merkezi yönetim arayüzüdür. Tüm varlıkların (Entities) yaşam döngüsünü kontrol eden ve bileşenlerin (Components) SparseSet depolarında nasıl saklanacağını belirleyen kural setini tanımlar.

Bu arayüzün varlık sebebi, sistemin geri kalanına (Systems, Bridges, UI) tutarlı, yüksek performanslı ve donanım dostu bir API sunmaktır. `Registry` sınıfı bu sözleşmeyi uygulayarak, verinin ham bellek (RAM) üzerindeki yerleşimini ve erişim hızını garanti eder.

---

## 2. Teknik Analiz
INexusRegistry, ECS operasyonları için şu kritik fonksiyonları zorunlu kılar:

- **Entity Lifecycle**: `Create()` ve `Destroy()` metodları ile varlıkların oluşturulması ve silinmesi yönetilir. Burada "Versioned ID" yapısı kullanılarak, silinen bir varlığın yerine gelen yeni varlığın eski verilerle karışması (dangling pointer) önlenir.
- **Pointer-Based Access**: `Get<T>` ve `Add<T>` metodları, bileşene güvenli (safe) bir referans yerine, performansın zirvesi olan bir ham pointer (`T*`) döner. Bu, C# kopyalama maliyetini sıfıra indirir.
- **Type-Erased Storage**: `ComponentSets` özelliği üzerinden, tüm özelleşmiş bileşen depolarına jenerik olmayan bir (`ISparseSet`) arayüzle erişim sağlanır. Bu, sistem genelinde toplu işlem yapmayı kolaylaştırır.
- **O(1) Access**: Has, Get, Add ve Remove operasyonlarının tamamı SparseSet matematiği sayesinde sabit zamanda (O(1)) gerçekleşir.

---

## 3. Mantıksal Akış
1.  **Varlık Oluşturma**: `Create()` çağrıldığında, `Registry` unmanaged bir indeks ayırır ve versiyon numarasını artırır.
2.  **Bileşen Ekleme**: `Add<T>` çağrıldığında, ilgili `SparseSet<T>` bulunur ve varlığın indeksi üzerinden unmanaged bellek alanı tahsis edilir.
3.  **Doğrulama**: `IsValid()` ile bir `EntityId`'nin hala geçerli bir nesneyi mi yoksa silinmiş bir atığı mı temsil ettiği kontrol edilir.
4.  **Temizlik**: `Dispose()` çağrıldığında, Registry'ye bağlı tüm SparseSet'ler ve unmanaged bellek alanları RAM'den temizlenir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Registry** | Varlıkların ve onlara bağlı bileşenlerin merkezi kayıt defteri. |
| **Dangling Pointer** | Geçersiz bir bellek adresini işaret eden tehlikeli referans. |
| **Versioned ID** | Bir indeksin kaç kez geri dönüştürüldüğünü (recycle) takip eden sayaç. |
| **Typed Storage** | Her bileşen tipi için ayrı, optimize edilmiş bellek blokları. |

---

## 5. Riskler ve Sınırlar
- **Manual Disposal**: `INexusRegistry` bir `IDisposable` nesnesidir. Eğer program sonunda `Dispose()` edilmezse, unmanaged bellek sızıntısı (leak) oluşur.
- **Pointer Safety**: `T*` üzerinden alınan veri üzerinde işlem yaparken, varlığın silinmesi durumunda bu pointer geçersiz kalır. Bu yüzden yoğun işlemlerde `IsValid` kontrolü kritiktir.

---

## 6. Kullanım Örneği
```csharp
public unsafe void ProcessCombat(INexusRegistry registry, EntityId player, EntityId enemy)
{
    if (registry.IsValid(player) && registry.Has<Health>(player))
    {
        Health* hp = registry.Get<Health>(player);
        hp->Amount -= 10;
        
        if (hp->Amount <= 0)
            registry.Destroy(player);
    }
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

public unsafe interface INexusRegistry : IDisposable
{
    EntityId Create();
    void Destroy(EntityId entity);
    bool IsValid(EntityId entity);
    
    T* Add<T>(EntityId entity, T component = default) where T : unmanaged;
    T* Get<T>(EntityId entity) where T : unmanaged;
    bool Has<T>(EntityId entity) where T : unmanaged;
    void Remove<T>(EntityId entity) where T : unmanaged;
    
    SparseSet<T> GetSet<T>() where T : unmanaged;
    IEnumerable<ISparseSet> ComponentSets { get; }
}
```

---

## Nexus Optimization Tip: Handle Reuse Safety
`EntityId` yapısı içindeki 32-bit versiyon alanı, bir ID'nin **4 milyar kez** geri dönüştürülmesine olanak tanır. Bu, oyununuzda milyarlarca varlık silinse bile, eski sistemlerin yanlışlıkla yeni varlıkların üzerine veri yazmasını (Corruption) donanım seviyesinde engeller.
