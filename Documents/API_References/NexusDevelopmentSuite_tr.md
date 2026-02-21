# Nexus Prime Mimari Rehberi: NexusDevelopmentSuite (Geliştirme Paketi)

## 1. Giriş
`NexusDevelopmentSuite.cs`, Nexus Prime projelerinin kurulumunu, bakımını ve günlük iş akışını otomatize eden bir "Kolaylaştırıcı" paketidir. Projenin ilk gününden son gününe kadar dosya düzenini ve sistem bütünlüğünü korur.

Bu paketin varlık sebebi; her yeni projede klasörleri elle oluşturmak, git ayarlarını yapmak veya yerelleştirme tablolarını manuel güncellemek gibi angarya işleri ortadan kaldırmaktır.

---

## 2. Teknik Analiz
Paket şu ana bileşenlerden oluşur:

- **InitializeOnLoad Hook**: Editor açıldığında veya kod derlendiğinde otomatik olarak çalışır. İçinde 5 dakikada bir varlıkları kaydeden (`Auto Save`) sessiz bir iş parçacığı barındırır.
- **NexusWizard (Sihirbaz)**: Projenin standart Nexus klasör yapısını (`Scripts`, `Entities`, `Data`, `UI` vb.) tek tuşla kurar.
- **Optimization Tools Integration**: Git ayarlarını ve proje yapılandırmalarını Nexus standartlarına göre optimize eder.
- **Localization Bridge**: Unmanaged bellek ile uyumlu yerelleştirme tablolarını otomatik olarak üretir.

---

## 3. Mantıksal Akış
1.  **Kurulum**: Geliştirici `Nexus -> Wizard -> Setup Project` yolunu izler.
2.  **Otomasyon**: Sihirbaz, `NexusFolderManager` ve `NexusOptimizationTools` sınıflarını çağırarak tüm altyapıyı inşa eder.
3.  **Sürekli İzleme**: Editör açık olduğu sürece periyodik olarak arka planda veri güvenliğini (Auto-save) sağlar.

---

## 4. Kullanım Örneği
```csharp
// Yeni bir projede:
// 1. [Nexus/Wizard/Setup Project] tıklanır.
// 2. "Initialize All Systems" butonuna basılır.
// 3. Proje klasörleri, Git ayarları ve Core sistemler milisaniyeler içinde hazır olur.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
[InitializeOnLoad]
public static class NexusDevelopmentSuite
{
    static NexusDevelopmentSuite() {
        EditorApplication.update += () => {
            // Auto-save logic...
        };
    }

    [MenuItem("Nexus/Wizard/Setup Project")]
    public static void OpenWizard() => NexusWizard.ShowWindow();
}
#endif
```

---

## Nexus Optimization Tip: Auto-Save Frequency
Varsayılan 5 dakikalık (300 saniye) auto-save süresini donanım gücünüze göre optimize edin. Çok büyük sahnelerde bu süreyi 10 dakikaya çıkarmak, **editördeki anlık takılmaları (Freeze) minimize eder.**
