# Nexus Prime Mimari Rehberi: NexusFolderManager (Klasör Yöneticisi)

## 1. Giriş
`NexusFolderManager.cs`, Nexus Prime projelerinde kurumsal düzeyde bir dosya yapısı kuran "Dizin Mimarı"dır. Projenin karmaşıklaştıkça dosyaların kaybolmasını önler ve Nexus sistemlerinin (Örn: Model, Entity, Data) doğru yerlerde konumlanmasını sağlar.

Bu aracın varlık sebebi; her yeni projede klasör yapısını (Folder Structure) elle kurmaktan kaynaklanan zaman kaybını önlemek ve ekip içi dökümantasyon standartlarını otomatik hale getirmektir.

---

## 2. Teknik Analiz
Klasör yöneticisi şu operasyonel güvenlik ve otomasyon araçlarına sahiptir:

- **Safe Directory Creation**: `Directory.CreateDirectory` kullanarak klasörleri oluştururken, `AssetDatabase.Refresh` ile Unity'nin bunları anında tanımasını sağlar.
- **Meta-Safe Deletion**: Klasör silerken sadece klasörü değil, ona bağlı olan `.meta` dosyasını da (`FileUtil.DeleteFileOrDirectory`) silerek Unity dökümantasyon hatalarını önler.
- **Project Skeleton Blueprint**: Proje için kritik olan `Scripts`, `Entities`, `Prefabs`, `Data`, `UI`, `VFX`, `SFX`, `Materials`, `Textures`, `Shaders`, `Plugins` ve `Documents` klasörlerini tek tuşla hiyerarşik bir düzende kurar.

---

## 3. Mantıksal Akış
1.  **Tetikleme**: Geliştirici `Nexus/Setup/Create Standard Folders` menüsünü seçer.
2.  **Katalog Tarama**: Sistem, oluşturulması gereken standart klasör listesini döngüye alır.
3.  **Varlık Kontrolü**: `Directory.Exists` ile klasörün zaten var olup olmadığı kontrol edilir (Veri kaybını önlemek için).
4.  **İnşa**: Eksik olan klasörler oluşturulur ve `Refresh` yapılarak Editör'e yansıtılır.

---

## 4. Kullanım Örneği
```csharp
// Standart bir Nexus projesi başlatmak:
NexusFolderManager.SetupStandardFolders();
// Sonuç: "Assets" dizini altında 10+ standart klasör anında belirir.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusFolderManager
{
    public static void CreateFolder(string path) {
        string fullPath = Path.Combine(Application.dataPath, path);
        if (!Directory.Exists(fullPath)) {
            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Nexus/Setup/Create Standard Folders")]
    public static void SetupStandardFolders() {
        CreateFolder("Scripts");
        CreateFolder("Entities");
        // ... list of core folders ...
    }
}
#endif
```

---

## Nexus Optimization Tip: Root Cleanliness
Tüm Nexus bileşenlerini ana bir üst klasör altında (Örn: `_Project` veya `_Game`) toplayın. Bu, **Unity'nin binlerce dosya arasındaki arama performansını artırır ve kütüphaneleri (Plugins) ana proje dosyalarından izole eder.**
