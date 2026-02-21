# Nexus Prime Mimari Rehberi: NexusEntity (Unity-ECS Kimlik Köprüsü)

## 1. Giriş
`NexusEntity.cs`, Nexus Prime'ın hibrit mimarisindeki en temel bileşendir. Bir Unity `GameObject`'ini, Nexus'un unmanaged dünyasındaki bir `EntityId` ile eşleştiren "Kimlik Bağlantısı" (Identity Link) görevini görür.

Bu bileşenin varlık sebebi; Unity'nin sahnelerindeki görsel nesnelerin, yüksek performanslı veriye dayalı (data-driven) mantık katmanıyla (Simülasyon) senkronize olabilmesini sağlamaktır. Her NexusEntity, aslında ECS dünyasındaki bir varlığın Unity tarafındaki "Temsilcisi"dir.

---

## 2. Teknik Analiz
NexusEntity, kimlik yönetimi için şu stratejileri izler:

- **Entity Identification**: Varlığa atanmış olan `EntityId`'yi (Index ve Version) saklar. Bu ID, unmanaged `Registry` üzerindeki verilere ulaşmak için kullanılan anahtardır.
- **Strict Singleton Component**: `[DisallowMultipleComponent]` ile bir nesnenin sadece tek bir ECS varlığıyla temsil edilmesini garanti eder.
- **Auto-Initialization**: Eğer nesne bir ECS sistemi tarafından oluşturulmadıysa (Örn: Standart Unity Instantiation), `Awake` anında Unity'nin `InstanceID`'sini kullanarak geçici bir sanal kimlik oluşturur.
- **Read-Only Inspection**: Inspector üzerinde ID'nin görünmesini sağlar ama elle değiştirilmesini engelleyerek veri bütünlüğünü korur.

---

## 3. Mantıksal Akış
1.  **Awake**: Nesne sahnede aktif olduğunda kimliği kontrol edilir.
2.  **Eşleme**: Eğer ID geçersizse, nesneye bir sanal ECS kimliği atanır.
3.  **Hizmet**: Diğer Unity bileşenleri (Örn: `NexusSyncTransform`), bu nesnenin `Id` özelliğini kullanarak Nexus Registry'den veri çeker.

---

## 4. Kullanım Örneği
```csharp
// Bir nesnenin ECS kimliğini öğren
var entity = GetComponent<NexusEntity>();
if (entity.Id.IsNotNull) {
    Debug.Log($"Varlık Indeksi: {entity.Id.Index}");
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity;

[DisallowMultipleComponent]
public class NexusEntity : MonoBehaviour
{
    [SerializeField, ReadOnly] private EntityId _id = EntityId.Null;

    public EntityId Id {
        get => _id;
        internal set => _id = value;
    }

    private void Awake() {
        if (_id.IsNull) _id = new EntityId { Index = (uint)gameObject.GetInstanceID(), Version = 0 };
    }
}
```

---

## Nexus Optimization Tip: Explicit ID Assignment
Hibrit bir projede nesneleri `Instantiate` ederken, `NexusEntity.Id` değerini manual olarak Nexus Registry üzerinden oluşturduğunuz gerçek bir ID ile eşleyin. Sanal ID kullanımı (InstanceID tabanlı), **Registry üzerindeki unmanaged sistemlerle tam uyumlu çalışmayabilir.**
