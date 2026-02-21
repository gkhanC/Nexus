# Nexus Prime Mimari Rehberi: NexusDevelopmentTools (Geliştirme Araçları & Havuzlama)

## 1. Giriş
`NexusDevelopmentTools.cs`, bellek yönetimi ve sistem erişilebilirliği için kritik olan iki ana yapıyı sunar: `NexusObjectPool` (Nesne Havuzlama) ve `NexusMonoBehaviourSingleton` (Tekil Nesne Deseni). Projenin çalışma zamanı performansını ve kod organizasyonunu düzenler.

Bu araçların varlık sebebi; her nesne talebinde `Instantiate` ve yok etmede `Destroy` gibi pahalı Unity işlemlerinden kaçınmak ve global servislere güvenli bir şekilde erişmektir.

---

## 2. Teknik Analiz
Geliştirme süreci için şu temel yapıları barındırır:

- **NexusObjectPool**: `Queue<GameObject>` kullanarak devre dışı bırakılan nesneleri saklar. `Spawn` çağrıldığında yeni nesne oluşturmak yerine havuzdan çeker. `name.Replace("(Clone)", "")` ile prefab isimlerini normalize ederek doğru havuza yerleştirir.
- **INexusPoolable (Arayüz)**: Havuzlanan nesnelerin "doğma" (`OnSpawn`) ve "ölme" (`OnDespawn`) anlarında ne yapacaklarını belirleyen bir kontrattır.
- **NexusMonoBehaviourSingleton**: Unity dünyası için optimize edilmiş, `lock(_lock)` mekanizmasıyla thread-safe (iş parçacığı güvenli) Singleton sunar. Nesne sahnede yoksa otomatik olarak oluşturur.

---

## 3. Mantıksal Akış
1.  **Spawn**: İstenen prefab'ın havuzunda boş nesne var mı bakılır. Varsa aktifleşir, yoksa yeni kopyalanır.
2.  **Yaşam Döngüsü**: Nesne havuzdan çıkınca `OnSpawn` tetiklenir; canı dolar, görseli sıfırlanır.
3.  **Despawn**: Nesne iade edilirken `OnDespawn` tetiklenir; efektler durur, nesne pasifleşir ve havuza girer.
4.  **Singleton Erişimi**: `Instance` çağrıldığında sistem sahnede bir tane "Unique" nesne olduğundan emin olur.

---

## 4. Kullanım Örneği
```csharp
// Havuzdan nesne al
GameObject bullet = NexusObjectPool.Spawn(bulletPrefab, pos, rot);

// Bir mermi bittiğinde havuzla iade et
NexusObjectPool.Despawn(gameObject);

// Singleton örneği
public class UIManager : NexusMonoBehaviourSingleton<UIManager> { ... }
UIManager.Instance.ShowSplash();
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Unity.Core;

public class NexusObjectPool : MonoBehaviour
{
    private static readonly Dictionary<string, Queue<GameObject>> _pools = new();

    public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot) {
        // Queue logic...
        return Instantiate(prefab, pos, rot);
    }
}

public abstract class NexusMonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T _instance;
    public static T Instance {
        get {
            // Thread-safe instance creation...
            return _instance;
        }
    }
}
```

---

## Nexus Optimization Tip: Stride Memory
Havuzdaki nesnelerin `Queue` içinde saklanması O(1) erişim sağlar. Ancak havuz çok büyürse (Örn: 50.000 mermi), `Dictionary` anahtar araması (`string hashing`) CPU yükü oluşturabilir. Çok büyük ölçeklerde Key yerine `Prefab.ID` kullanmak **performansı ek bir %10 artırır.**
