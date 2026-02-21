# Nexus Prime Mimari Rehberi: Nexus Command Engines (Komut Sistemleri)

## 1. Giriş
Nexus Prime içindeki Komut Sistemleri (`NexusConsole.cs` ve `CommandConsole.cs`), geliştiricinin Unity Editör içinden unmanaged ECS dünyasına doğrudan metin tabanlı komutlar göndermesini sağlayan "Giriş Terminalleri"dir.

Bu sistemlerin varlık sebebi; kod yazıp build almadan veya Inspector içinde karmaşık değerlerle uğraşmadan, terminal üzerinden hızlıca nesne oluşturmak, sistemleri durdurmak veya veri manipülasyonu yapmaktır.

---

## 2. Teknik Analiz
Terminaller şu iki ana işlev üzerinden çalışır:

- **NexusConsole (Terminal)**: `NexusCommandManager` ile doğrudan entegre olan, yürütme geçmişini (History) gösteren profesyonel bir log terminalidir. Başarılı ve başarısız yürütmeleri `NexusLogger` üzerinden takip eder.
- **CommandConsole (CLI - Komut Satırı)**: Daha çok "Sözdizimi" (Syntax) odaklı bir terminaldir. `nexus create --type Orc --pos 0,0,0` gibi karmaşık parametreleri çözümlemek (Parsing) için tasarlanmıştır.

---

## 3. Mantıksal Akış
1.  **Girdi**: Geliştirici Editör penceresindeki metin kutusuna komutu yazar.
2.  **Onay**: "Enter" tuşuna basıldığında komut yakalanır.
3.  **Yönlendirme**: Komut, `NexusCommandManager.Execute(input)` metoduna iletilir.
4.  **Geri Bildirim**: İşlem sonucu (Başarı/Hata) konsol penceresinde görselleştirilir.

---

## 4. Kullanım Örneği
```text
// Nexus Console Kullanımı:
> timescale 0.5
> entities count
> snapshot restore last

// Command Console (CLI) Örneği:
> nexus health --set 100 --target Player
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusConsole : EditorWindow {
    private string _input = "";
    private void OnGUI() {
        _input = EditorGUILayout.TextField("Execute", _input);
        if (GUILayout.Button("Submit")) {
            Nexus.Unity.Communication.NexusCommandManager.Execute(_input);
            _input = "";
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Command Buffering
Çok sık kullanılan komutları birer makro (Sequence) haline getirin. CommandManager üzerinden "batch" komutlar göndererek, **test süreçlerindeki manuel giriş süresini %40 oranında düşürebilirsiniz.**
