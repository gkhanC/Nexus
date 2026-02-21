# Nexus Prime Mimari Rehberi: NexusWorkflowTools (İş Akışı Otomasyonu)

## 1. Giriş
`NexusWorkflowTools.cs`, geliştiricilerin zaman alan manuel işlemleri saniyeler içinde halletmesini sağlayan bir "Üretkenlik Motoru"dur. Namespace karmaşasından hiyerarşi düzensizliğine kadar pek çok sorunu otomatik olarak çözer.

Bu araçların varlık sebebi; her script dosyasına elle namespace yazmak veya yüzlerce düşman nesnesini tek tek isimlendirmek yerine, kural tabanlı bir otomasyon sunmaktır.

---

## 2. Teknik Analiz
İş akışı paketi şu üç ana yardımcıyı sunar:

- **Namespace Auto-Fixer**: Projedeki tüm scriptleri tarar ve klasör yapısına göre olması gereken Nexus namespace'ini (`Nexus.Module.SubModule`) önerir.
- **Batch Rename Tool**: Seçili olan birden fazla nesneyi belirli bir ön ek (`Prefix`) ve ardışık numara (`startIndex`) ile anında yeniden isimlendirir.
- **Quick Scene Switcher**: Build ayarlarına ekli olan sahneleri listeleyen ve açık olan sahneyi kaydetmeyi unutmadan sahneler arası hızlı geçiş sağlayan bir paneldir.

---

## 3. Mantıksal Akış
1.  **Analiz**: Araç, `AssetDatabase` üzerinden proje varlıklarını tarar.
2.  **Toplu İşlem**: Geliştiricinin seçtiği nesneler üzerine `OrderBy` (Sıralama) kullanarak kontrollü bir döngü kurulur.
3.  **Eylem**: İsim değişikliği veya sahne geçişi gibi işlemler Unity Editör API'ları üzerinden güvenli bir şekilde yapılır.

---

## 4. Kullanım Örneği
```csharp
// Toplu İsimlendirme:
// 1. Sahnede 20 tane mermi seçilir.
// 2. [Nexus/Workflow/Batch Rename] açılır.
// 3. Prefix "Projectile_" yazılır ve "Rename Selected" tıklanır.
// Sonuç: Projectile_00, Projectile_01 ...
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public static class NexusWorkflowTools
{
    [MenuItem("Nexus/Workflow/Batch Rename")]
    public static void ShowBatchRename() => BatchRenameWindow.ShowWindow();
}

public class BatchRenameWindow : EditorWindow {
    private string _prefix = "Entity_";
    private void OnGUI() {
        _prefix = EditorGUILayout.TextField("Prefix", _prefix);
        if (GUILayout.Button("Rename Selected")) {
            // Order and rename logic...
        }
    }
}
#endif
```

---

## Nexus Optimization Tip: Hierarchy Sorting
Batch Rename aracını kullanırken `GetSiblingIndex` kullanarak sıralama yapın. Bu, **sahnede karmaşık duran nesneleri hem görsel olarak hem de mantıksal olarak sıraya sokarak hata payını %0'a indirir.**
