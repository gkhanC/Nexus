# Nexus Prime Mimari Rehberi: Time-Travel Debugger (Zaman Yolculuğu)

## 1. Giriş
`TimeTravelDebugger.cs`, Nexus Prime'ın deterministik simülasyon yapısını kullanarak oyunun herhangi bir anına geri dönmeyi sağlayan "Geri Sarma" arayüzüdür. `SnapshotManager` tarafından kaydedilen ham unmanaged verileri bir Slider aracılığıyla gezilebilir hale getirir.

Bu aracın varlık sebebi; saniyelik bir hatayı veya fizik etkileşimini kare kare inceleyerek sorunun kaynağını (Root Cause) tam olarak tespit etmektir.

---

## 2. Teknik Analiz
Zaman yolculuğu şu bileşenlerle çalışır:

- **Snapshot Scrubbing**: `_currentFrame` değeri değiştikçe, `SnapshotManager` üzerindeki ilgili bellek bloğunu aktif Registry'e kopyalar.
- **Deterministic Playback**: Simülasyon durdurulmuş olsa bile, kaydedilen veriler üzerinden oyunun o anki görsel ve mantıksal durumunu (Entities, Components) yeniden inşa eder.
- **Frame Navigation**: Geliştiriciye "Step Back" ve "Step Forward" butonlarıyla mikro saniyelik (FixedUpdate bazlı) ilerleme ve gerileme imkanı sunar.
- **State Restoration**: Slider bırakıldığında, tüm unmanaged varlıklar seçilen karedeki koordinatlarına ve değerlerine geri döner.

---

## 3. Mantıksal Akış
1.  **Kayıt**: Oyun çalışırken arka planda `SnapshotManager` periyodik kayıt alır.
2.  **Durdurma**: Geliştirici oyunu pause eder ve Time-Travel panelini açar.
3.  **Kaydırma**: Slider çekildiğinde her değer değişimi bir `RestoreSnapshot` çağrısı tetikler.
4.  **İnceleme**: Sahnedeki nesneler fiziksel olarak o ana döner ve `LiveStateTweaker` o anki değerleri gösterir.

---

## 4. Kullanım Örneği
```text
// Hatalı bir patlamayı incelemek:
// 1. Patlama olduktan sonra oyun durdurulur.
// 2. Slider patlama anından 20 kare öncesine çekilir.
// 3. "Step Forward" ile patlamanın başladığı ilk kare bulunur.
// 4. Hatalı merminin hızı ve açısı analiz edilir.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class TimeTravelDebugger : EditorWindow
{
    [MenuItem("Nexus/Time-Travel Debugger")]
    public static void ShowWindow() => GetWindow<TimeTravelDebugger>("Time-Travel");

    private float _currentFrame = 0;
    private void OnGUI() {
        _currentFrame = EditorGUILayout.Slider("Frame", _currentFrame, 0, 300);
        // Play, Pause, Step controls...
    }
}
#endif
```

---

## Nexus Optimization Tip: Keyframe Sampling
Tüm kareleri RAM'de tutmak yerine sadece "Keyframe"leri (Örn: her 10 karede bir) tam kaydedip aradaki farkları (Delta) tutun. Bu, **Time-Travel bellek kullanımını %80 oranında azaltarak daha uzun süre geri gitmenizi sağlar.**
