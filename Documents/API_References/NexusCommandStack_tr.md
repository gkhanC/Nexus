# Nexus Prime Mimari Rehberi: NexusCommandStack (İşlem Geçmişi Yöneticisi)

## 1. Giriş
`NexusCommandStack.cs`, uygulama içindeki eylemlerin geri alınabilmesini (Undo) ve tekrar edilebilmesini (Redo) sağlayan bir "İşlem Geçmişi Katmanı"dır. HypeFire mimarisinden entegre edilmiştir. Özellikle editör araçlarında, karmaşık UI formlarında veya strateji oyunlarındaki hamle yönetiminde kritik rol oynar.

Bu yığının varlık sebebi; her eylemin bir "Undo" senaryosunu (Ters işlem) standart bir yapıda saklayarak, kullanıcının yaptığı değişiklikleri güvenli bir şekilde ileri/geri sarmasını sağlamaktır.

---

## 2. Teknik Analiz
İşlem yönetimi için şu mimari bileşenleri sunar:

- **Command Pattern**: Her eylem `NexusCommand<T>` sınıfından türetilir. Bu sınıflar hem `Execute` (Yürüt) hem de `Undo` (Geri Al) metodlarını içermek zorundadır.
- **Pointer-Based Navigation**: Geçmiş listesi içinde bir index (`_currentIndex`) tutarak, o anki geçmiş konumunu takip eder.
- **Branch Management**: Eğer kullanıcı geçmişte bir noktaya geri dönüp yeni bir işlem yaparsa, mevcut index'in ilerisindeki tüm "Redo" geçmişini otomatik olarak temizler (RemoveRange) ve yeni bir branş oluşturur.
- **Generic Context Support**: Komutlar jenerik bir `T` hedefi üzerinde çalışır. Bu hedef bir `World`, `Inventory` veya `EditorWindow` olabilir.

---

## 3. Mantıksal Akış
1.  **Yürütüm (Push)**: Yeni bir komut gelir. Önce `Execute` denenir. Başarılıysa listeye eklenir ve index ilerletilir.
2.  **Geri Al (Undo)**: Mevcut index'teki komutun `Undo` metodu çağrılır ve index geri çekilir.
3.  **Yenile (Redo)**: Index'in bir önündeki komut tekrar `Execute` edilir ve index ileri itilir.
4.  **Temizlik**: `Clear` ile tüm işlem geçmişi RAM'den silinir.

---

## 4. Kullanım Örneği
```csharp
// Örnek bir komut: Pozisyon Değiştirme
public class MoveCommand : NexusCommand<Transform> {
    private Vector3 _oldPos;
    private Vector3 _newPos;
    
    public override bool Execute(Transform t) { _oldPos = t.position; t.position = _newPos; return true; }
    public override bool Undo(Transform t) { t.position = _oldPos; return true; }
}

// Yığında kullan
var stack = new NexusCommandStack<Transform>();
stack.PushAndExecute(new MoveCommand(Vector3.up), target);
stack.Undo(target); // Eski pozisyona döner
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Communication;

public class NexusCommandStack<T>
{
    private readonly List<NexusCommand<T>> _commands = new();
    private int _currentIndex = -1;

    public void PushAndExecute(NexusCommand<T> command, T target) {
        if (_currentIndex < _commands.Count - 1)
            _commands.RemoveRange(_currentIndex + 1, _commands.Count - (_currentIndex + 1));

        if (command.Execute(target)) {
            _commands.Add(command);
            _currentIndex++;
        }
    }

    public void Undo(T target) {
        if (_currentIndex >= 0 && _commands[_currentIndex].Undo(target)) _currentIndex--;
    }
}
```

---

## Nexus Optimization Tip: History Capping
Gereksiz bellek kullanımını önlemek için geçmiş listesine bir limit (Örn: Son 100 işlem) koyun. Liste bu limiti aştığında en eski komutları silerek (FIFO), **uzun süren oturumlarda bellek şişmesini engelleyebilirsiniz.**
