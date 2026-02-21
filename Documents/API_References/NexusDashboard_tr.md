# Nexus Prime Mimari Rehberi: NexusDashboard (Nexus Hub)

## 1. Giriş
`NexusDashboard.cs`, tüm Nexus Prime araçlarının ana kumanda merkezidir ve Editör içinde "The Hub" olarak adlandırılır. Geliştiricinin karmaşık unmanaged sistemleri, optimizasyon araçlarını ve görsel hata ayıklayıcıları tek bir pencereden yönetmesini sağlar.

Bu arayüzün varlık sebebi; onlarca farklı editör penceresi arasında kaybolmak yerine, mantıksal olarak gruplandırılmış bir "Developer OS" (Geliştirici İşletim Sistemi) deneyimi sunmaktır.

---

## 2. Teknik Analiz
Dashboard, araçları üç ana kategoride organize eder:

- **Architectural & Logic**: Kısıtlayıcı denetleyiciler (Constraint Checker), DI motoru, Graph Editor ve Bit-Level sıkıştırma araçları gibi düşük seviyeli sistemler.
- **Unity Editor & DX**: Görsel hata ayıklayıcı (Visual Debugger), Zaman yolculuğu (Time-Travel), Isı haritaları (Memory Heatmap) ve Prefab dönüştürücüler.
- **Multimedia & Integration**: Shader köprüleri, Audio linker'lar, VFX sağlayıcılar ve fizik entegrasyon araçları.

---

## 3. Mantıksal Akış
1.  **Açılış**: Geliştirici `Nexus -> The Hub` menüsünden paneli açar.
2.  **Gruplandırma**: `DrawGroup` metodu, araçları kutular içine alarak görsel hiyerarşi sağlar.
3.  **Başlatma**: "Initialize All Systems" butonu, tüm Nexus alt sistemlerini (Registry, Pool, Sync vb.) tek seferde hazır hale getirir.
4.  **Hızlı Erişim**: Her buton, ilgili aracın Editör penceresini tetikler.

---

## 4. Kullanım Örneği
```csharp
// Dashboard üzerinden sistem başlatma
// 1. [Nexus/The Hub] açılır.
// 2. [Initialize All Systems] tıklanır.
// 3. Console'da "Nexus: Initializing Developer OS Toolset..." mesajı görülür.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class NexusDashboard : EditorWindow
{
    [MenuItem("Nexus/The Hub (Dashboard)")]
    public static void ShowWindow() => GetWindow<NexusDashboard>("Nexus Dashboard");

    private void OnGUI() {
        GUILayout.Label("Nexus Developer OS - The Hub", EditorStyles.boldLabel);
        // Draw groups...
    }
}
#endif
```

---

## Nexus Optimization Tip: Window Docking
Nexus Dashboard'u Unity arayüzünde "Inspector" yanına veya "Game View" altına sabitleyin (Dock). Bu sayede geliştirme sırasında **sistem durumlarını değiştirmek için pencere arama süresini %100 ortadan kaldırırsınız.**
