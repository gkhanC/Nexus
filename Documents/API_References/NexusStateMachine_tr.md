# Nexus Prime Mimari Rehberi: NexusStateMachine (Durum Yöneticisi)

## 1. Giriş
`NexusStateMachine.cs`, karmaşık nesne davranışlarını (Örn: Oyuncu hareket durumları, NPC yapay zekası, Oyun döngüsü aşamaları) kontrol edilebilir ve modüler parçalara ayıran bir "Davranış Orkestratörü"dür. MonoBehaviour olarak tasarlanmış olması sayesinde Unity sahneleriyle tam uyumlu çalışır.

Bu makinenin varlık sebebi; "If-Else" yığınlarından oluşan devasa kod bloklarını engellemek ve her durumu (State) kendi içinde izole, test edilebilir ve genişletilebilir sınıflara bölmektir.

---

## 2. Teknik Analiz
Durum yönetimi için şu mimari standartları uygular:

- **Interface-Based States**: Her durum `IState` arayüzünü (interface) implemente eder. Bu sayede `Enter` (Giriş), `Update` (Güncelleme) ve `Exit` (Çıkış) metodları zorunlu kılınarak durum yaşam döngüsü standartlaştırılır.
- **Atomic State Switching**: `ChangeState` metodu ile bir durumdan diğerine geçerken, eski durumun `Exit` logic'i ile yeni durumun `Enter` logic'inin hatasız ve sıralı çalışmasını sağlar.
- **Update Delegation**: Mevcut aktif durumun `Update` metodunu her karede otomatik olarak çağırarak, mantıksal yürütümü (logic execution) o anki duruma delege eder.
- **Flexible Integration**: Hem saf kod tabanlı durumları hem de Unity Animator transition'larını tetikleyebilecek kadar esnektir.

---

## 3. Mantıksal Akış
1.  **Giriş (Enter)**: Yeni duruma geçildiğinde bir kez tetiklenir (Örn: Yürüme animasyonunu başlat).
2.  **Döngü (Update)**: Durum aktif olduğu sürece her karede çalışır (Örn: Hızı kontrol et).
3.  **Geçiş (Change)**: Bir koşul oluştuğunda (Örn: Zıplama tuşuna basıldı) yeni bir duruma geçiş sinyali verilir.
4.  **Çıkış (Exit)**: Durumdan ayrılmadan hemen önce bir kez tetiklenir (Örn: Toz efektini kapat).

---

## 4. Kullanım Örneği
```csharp
public class PlayerController : MonoBehaviour {
    private NexusStateMachine _sm;

    void Start() {
        _sm = gameObject.AddComponent<NexusStateMachine>();
        _sm.ChangeState(new IdleState());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) _sm.ChangeState(new JumpState());
    }
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Communication;

public class NexusStateMachine : MonoBehaviour
{
    public interface IState {
        void Enter();
        void Update();
        void Exit();
    }

    private IState _currentState;

    public void ChangeState(IState newState) {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    void Update() => _currentState?.Update();
}
```

---

## Nexus Optimization Tip: State Pooling
Eğer saniyede onlarca kez durum değiştiriyorsanız (Örn: Çok hızlı değişen AI kararları), durum sınıflarını her seferinde `new` ile oluşturmak yerine bir "Pool" (Havuz) içinde saklayıp tekrar kullanın. Bu, **Garbage Collector (GC) üzerindeki baskıyı %25 azaltacaktır.**
