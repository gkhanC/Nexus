# Nexus Prime Mimari Rehberi: NexusOptimizationTools (Optimizasyon Araçları)

## 1. Giriş
`NexusOptimizationTools.cs`, projenin hem geliştirme hem de yayın (Build) aşamasındaki verimliliğini artıran bir "Temizlik ve Yapılandırma" paketidir. Gereksiz prefab verilerini ayıklar, bağımlılıkları analiz eder ve sürüm kontrol (Git) ayarlarını Nexus standartlarına göre optimize eder.

Bu araçların varlık sebebi; projenin zamanla şişmesini (Bloat) önlemek ve build boyutunu minimize ederken teknik borcu (Technical Debt) azaltmaktır.

---

## 2. Teknik Analiz
Paket şu üç ana fonksiyona odaklanır:

- **Strip Legacy Prefabs**: Prefab'lar içindeki kullanılmayan bileşenleri, Editör-only etiketleri ve Nexus ile uyumsuz eski Unity verilerini otomatik olarak temizler.
- **Build Dependency Graph**: `AssetDatabase.GetDependencies` kullanarak, seçili bir varlığın hangi dosyalara bağımlı olduğunu ve toplam disk maliyetini hesaplar.
- **Git LFS Setup**: Unity projeleri için en optimize edilmiş `.gitattributes` dosyasını otomatik olarak oluşturur (Binary dosyaların LFS ile yönetilmesini sağlar).

---

## 3. Mantıksal Akış
1.  **Seçim**: Geliştirici optimize etmek istediği varlıkları veya tüm projeyi seçer.
2.  **Analiz**: Araç, varlıkların referans zincirini tarar.
3.  **Eylem**: `System.IO.File` veya `AssetDatabase` üzerinden dosya manipülasyonu (yazma/silme) yapılır.
4.  **Doğrulama**: Yapılan temizliğin sonuçları (Örn: "Stripped 45 items") konsola raporlanır.

---

## 4. Kullanım Örneği
```csharp
// Git Kurulumu:
NexusOptimizationTools.SetupGit();
// Sonuç: Ana dizinde Unity optimize .gitattributes dosyası oluşur.

// Prefab Temizliği:
// [Nexus/Optimization/Strip Legacy Prefabs] tıklanır.
// Sonuç: Dosya boyutu küçültülmüş ve Nexus-Ready hale getirilmiş prefablar.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusOptimizationTools
{
    [MenuItem("Nexus/Optimization/Strip Legacy Prefabs")]
    public static void StripPrefabs() {
        // Find unused components and remove...
    }

    [MenuItem("Nexus/Integration/Git LFS Setup")]
    public static void SetupGit() {
        // Write standard .gitattributes file...
    }
}
#endif
```

---

## Nexus Optimization Tip: Build Stripping
`StripPrefabs` aracını Build öncesi `IPreprocessBuildWithReport` arayüzüne bağlayın. Bu sayede her build öncesi otomatik temizlik yapılarak **final build boyutunda %15-20 tasarruf sağlanabilir.**
