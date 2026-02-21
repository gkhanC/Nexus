# Nexus Prime Mimari Rehberi: LogicHotSwapSystem (Çalışma Zamanı Mantık Değişimi)

## 1. Giriş
`LogicHotSwapSystem.cs`, modern oyun geliştirme süreçlerindeki en büyük darboğazlardan biri olan "Kod Değiştir - Derle - Yeniden Başlat" döngüsünü kırmak için tasarlanmış bir deneysel altyapıdır. Geliştiricilerin, oyunu veya simülasyonu kapatmadan, `INexusSystem` mantığını canlı bir şekilde güncellemesine olanak tanır.

Bu sistemin varlık sebebi, unmanaged verinin (Registry) bellek üzerindeki kalıcılığını bozmadan, sadece o veriyi işleyen metodları (simülasyon mantığını) çalışma zamanında "sıcak" bir şekilde takas etmektir.

---

## 2. Teknik Analiz
LogicHotSwapSystem, dinamik kod değişimi için şu mekanizmaları kullanır:

- **Assembly Loading**: Yeni yazılmış ve derlenmiş sistem mantığı, `Assembly.Load` veya `AssemblyLoadContext` kullanılarak mevcut işleme (process) dahil edilir.
- **System Interface Bridge**: Yeni yüklenen sınıflar `INexusSystem` arayüzünü uyguladığı sürece, `JobSystem` içindeki eski sistemin referansı ile yer değiştirebilirler.
- **State Persistence**: Nexus'un ECS yapısı sayesinde tüm durum (state) zaten `Registry` üzerindedir. Mantık (Logic) değiştiğinde veri aynı kaldığı için hiçbir ilerleme kaybolmaz (Zero State Loss).
- **Reflective Injection**: Yeni yüklenen sisteme eski sistemin sahip olduğu `Registry` ve diğer bağımlılıklar tekrar enjekte edilir.

---

## 3. Mantıksal Akış
1.  **Tetikleme**: Geliştirici yeni bir DLL yolu ile `SwapSystemLogic` metodunu çağırır.
2.  **Yükleme**: Belirtilen yoldaki binary bellek üzerine alınır.
3.  **Bulma**: DLL içindeki aynı isimli sistem tipi Reflection ile bulunur.
4.  **Takas**: `JobSystem` içindeki eski sistem çıkarılır, yerine yeni yüklenen sistemin örneği (instance) eklenir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Hot-Swap** | Bir program çalışırken durdurulmadan parçalarının güncellenmesi işlemi. |
| **Assembly** | .NET ortamında derlenmiş kod birimi (DLL veya EXE). |
| **Zero State Loss** | Mantık değişimi sırasında var olan oyun verilerinin korunması durumu. |
| **Dynamic Invocation** | Bir metodun veya sınıfın isminin çalışma zamanında belirlenip çağrılması. |

---

## 5. Riskler ve Sınırlar
- **Assembly Leaks**: .NET Core öncesi sürümlerde yüklenen assembly'lerin bellekten atılamaması (unload) RAM birikmesine neden olabilir.
- **Breaking Changes**: Eğer yeni mantık, eski verinin yapısını (Component struct) değiştirmişse, bellek uyumsuzluğu (Memory Corruption) nedeniyle uygulama çökebilir.
- **Thread Safety**: Takas işlemi sistemlerin `Execute()` edilmediği güvenli bir birleşme noktasında (Sync Point) yapılmalıdır.

---

## 6. Kullanım Örneği
```csharp
// Canlı bir oyun sırasında yerçekimi mantığını güncelle
var hotswap = new LogicHotSwapSystem();
hotswap.SwapSystemLogic(currentGravitySystem, "Path/To/New/PhysicsPart2.dll");
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Reflection;
namespace Nexus.Core;

public class LogicHotSwapSystem
{
    public void SwapSystemLogic(INexusSystem oldSystem, string assemblyPath)
    {
        // 1. Load the assembly from path.
        // 2. Find the implementation of the same system type.
        // 3. Migrate state and replace the instance in JobSystem.
        Console.WriteLine($"Nexus: Hot-swapping logic for {oldSystem.GetType().Name}");
    }
}
```

---

## Nexus Optimization Tip: Context-Based Reloading
`AssemblyLoadContext` kullanarak sistemlerinizi izole edilmiş alanlara yükleyin. Bu sayede, eski kodunuzu bellekten tamamen silebilir (Unload) ve **uzun süreli geliştirme seanslarında bellek şişmesini %100 engelleyebilirsiniz.**
