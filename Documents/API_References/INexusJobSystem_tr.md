# Nexus Prime Mimari Rehberi: INexusJobSystem (Sistem Orkestrasyon Sözleşmesi)

## 1. Giriş
`INexusJobSystem.cs`, Nexus Prime framework'ünün iş yürütme katmanını tanımlayan üst düzey arayüzdür. Tüm mantıksal sistemlerin (`INexusSystem`) hangi sıra ile ve hangi donanım kaynaklarını kullanarak çalıştırılacağını belirleyen orkestratörün kontratıdır.

Bu arayüzün varlık sebebi; oyun döngüsü (Game Loop) ile iş mantığı arasındaki bağı koparmak, sistemleri veri bağımlılıklarına göre otomatik olarak paralelleştirmek ve her bir işin işlemci üzerindeki maliyetini (Metrics) takip edilebilir kılmaktır.

---

## 2. Teknik Analiz
INexusJobSystem, yüksek performanslı sistem yönetimi için şu yetenekleri zorunlu kılar:

- **System Registration**: `AddSystem()` ile kaydedilen her sistem, Reflection yoluyla bağımlılık analizine tabi tutulur. Bu, manuel "Thread" yönetimini ortadan kaldırır.
- **Dependency-Aware Execution**: `Execute()` çağrıldığında, sistemler birbirini bloklamadan (Data Race öncelikli) en uygun çekirdeklerde paralel olarak tetiklenir.
- **Performance Monitoring**: `GetLastExecutionMetrics()` üzerinden, her bir sistemin kaç mikrosaniyede tamamlandığına dair bilimsel veri sağlar.
- **Thread Safety Abstraction**: Geliştiricinin kilit (Lock/Mutex) mekanizmalarıyla uğraşmasını engeller; paralel güvenliği [Read]/[Write] öznitelikleri üzerinden arayüz seviyesinde sağlar.

---

## 3. Mantıksal Akış
1.  **Kayıt**: Sistemler oyunun başlangıcında `JobSystem`'e eklenir.
2.  **Sıralama**: Sistemler arası bağımlılık grafiği (DAG) oluşturulur.
3.  **Tetikleme**: Ana döngü her karede `Execute()` metodunu çağırır.
4.  **Analiz**: Performans metrikleri toplanarak optimizasyon için geliştiriciye sunulur.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Orchestrator** | İş parçacıklarının ve sistemlerin koordinatörü. |
| **Execution Metrics** | Bir işin çalışma süresi, CPU kullanımı ve gecikme verileri. |
| **Worker Thread** | Arka planda ağır hesaplamaları yapan yardımcı işlemci kolu. |
| **Dispatcher** | İşleri uygun olan boş çekirdeklere dağıtan birim. |

---

## 5. Riskler ve Sınırlar
- **Main Thread Stalls**: Eğer `Execute()` içinde çok ağır ve paralel olmayan bir iş varsa, oyunun kare hızı (FPS) düşebilir.
- **Ordering Dependencies**: İki sistem aynı veriye yazıyorsa, arayüz bunların sırasını garanti eder ancak bu durum paralelliği kısıtlar.

---

## 6. Kullanım Örneği
```csharp
public void SetupGame(INexusJobSystem jobSystem, Registry registry)
{
    // Sistemleri ekle
    jobSystem.AddSystem(new PhysicsSystem());
    jobSystem.AddSystem(new AISystem());

    // Her karede çalıştır
    void OnUpdate() {
        jobSystem.Execute();
        
        // Performansı izle
        var metrics = jobSystem.GetLastExecutionMetrics();
        foreach(var m in metrics)
            Console.WriteLine($"{m.SystemName}: {m.DurationMs}ms");
    }
}
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
using System.Collections.Generic;
namespace Nexus.Core;

public interface INexusJobSystem
{
    void AddSystem(INexusSystem system);
    void Execute();
    List<JobSystem.ExecutionMetrics> GetLastExecutionMetrics();
}
```

---

## Nexus Optimization Tip: Cycle Budgeting
`GetLastExecutionMetrics()` verilerini kullanarak, her kare için bir **Saat Döngüsü Bütçesi (Cycle Budget)** belirleyin. Eğer bir sistem bütçeyi aşıyorsa, onu daha küçük parçalara (Jobs) bölerek paralellikten maksimum fayda sağlamayı hedefleyin.
