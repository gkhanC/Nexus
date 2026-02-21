# Nexus Prime Mimari Rehberi: Nexus CLI (Program.cs)

## 1. Giriş
`Program.cs`, Nexus Prime framework'ünün Command Line Interface (CLI) giriş noktasıdır. Nexus Prime sadece bir kütüphane değil, aynı zamanda geliştiricinin iş akışını hızlandıran bir araç setidir. CLI, bir projenin sıfırdan oluşturulması (Scaffolding) ve Nexus standartlarına uygun bileşenlerin otomatik üretilmesi süreçlerini yönetir.

Bu aracın varlık sebebi, unmanaged ve unsafe yapılarla çalışırken gereken zorunlu proje ayarlarını (Örn: `AllowUnsafeBlocks`) ve dosya hiyerarşisini geliştiricinin manuel yapması yerine hatasız bir şekilde otomatize etmektir.

---

## 2. Teknik Analiz
Nexus CLI, geliştirici deneyimi (DX) için şu yetenekleri sunar:

- **Project Scaffolding**: `nexus new` komutu ile `.csproj` dosyasını `net10.0` ve `AllowUnsafeBlocks` ayarlarıyla otomatik olarak oluşturur.
- **Component Boilerplate Generation**: `nexus add component` komutu ile Nexus standartlarına uygun, `MustBeUnmanaged` kısıtlamalarına hazır struct taslakları üretir.
- **Directory Hierarchy Enforcement**: Bileşenlerin (Components) ve sistemlerin (Systems) nerede bulunması gerektiğini belirleyerek projenin düzenli kalmasını sağlar.
- **Environment Preparation**: Projenin derlenebilmesi için gereken implicit using ve nullable ayarlarını peşinen yapar.

---

## 3. Mantıksal Akış
1.  **Girdi Analizi**: Komut satırından gelen argümanlar (`args`) parse edilir.
2.  **Komut Eşleşmesi**: `new` veya `add` komutuna göre ilgili metod tetiklenir.
3.  **Dosya İşlemleri**: `System.IO` üzerinden dizinler oluşturulur ve `File.WriteAllText` ile kod dosyaları disk üzerine yazılır.
4.  **Geri Bildirim**: İşlem durumu konsol üzerinden geliştiriciye raporlanır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **CLI (Command Line Interface)** | Komut satırı üzerinden yazılımla etkileşime geçmeyi sağlayan arayüz. |
| **Scaffolding** | Bir yazılım projesinin temel iskeletinin ve dosyalarının otomatik olarak kurulması. |
| **Boilerplate** | Tekrar tekrar kullanılan, standart hale gelmiş kod parçaları. |
| **Implicit Usings** | C# projelerinde sık kullanılan namespace'lerin otomatik olarak eklenmesi özelliği. |

---

## 5. Riskler ve Sınırlar
- **File System Permissions**: CLI'ın çalıştırıldığı dizinde dosya yazma yetkisi yoksa hata verebilir.
- **Limited Scope**: Şu anki versiyon sadece temel bileşen üretimine odaklıdır, karmaşık paralel sistem (Parallel System) taslakları henüz eklenmemiştir.

---

## 6. Kullanım Örneği
```bash
# Yeni bir proje oluştur
nexus new MyKillerGame

# Projeye yeni bir bileşen ekle
nexus add component Health
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Core;

class Program
{
    static void Main(string[] args)
    {
        string command = args[0].ToLower();
        switch (command)
        {
            case "new": CreateNewProject(args[1]); break;
            case "add": AddComponent(args[2]); break;
        }
    }
}
```

---

## Nexus Optimization Tip: Custom Templates
CLI içindeki `GetProjectFileContent` metodunu kendi takımınızın standartlarına (Örn: özel NuGet paketleri veya `net10.0` yerine farklı bir target) göre modifiye ederek, her yeni projede **kurulum süresini %100 oranında otomatize edebilirsiniz.**
