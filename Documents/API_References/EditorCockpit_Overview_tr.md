# Nexus Prime Mimari Rehberi: Editor Cockpit (Profesyonel Kontrol Paneli)

## 1. Giriş
`EditorCockpit`, Nexus Prime framework'ünün en ileri seviye tanılama (diagnostic) ve müdahale araçlarını barındıran "Üst Düzey Yönetim" modülüdür. Standart editör araçlarından farklı olarak, bu araçlar doğrudan unmanaged belleğe, kayıt (Registry) yapılarına ve Snapshot hiyerarşisine en alt seviyeden erişir.

Bu modülün varlık sebebi; oyunun kalbindeki en karmaşık mühendislik sorunlarını (Bellek fragmantasyonu, veri bütünlüğü, zaman yolculuğu sapmaları) profesyonel bir kokpit arayüzü ile çözmektir.

---

## 2. Teknik Analiz (Araç Seti)

Kokpit modülü şu beş ana profesyonel aracı içerir:

### A. Entity Search Pro
Sıradan bir aramadan öte, `SELECT Entities WHERE HasComponent(Position)` gibi SQL tabanlı sorgularla milyonlarca varlık arasında veri madenciliği yapar.

### B. Live Tweaker (Cockpit Edition)
`Pointer.Unbox` ve `Marshal` tekniklerini kullanarak, unmanaged bileşen verilerini çalışma zamanında (Runtime) Slider'lar aracılığıyla doğrudan RAM üzerinde manipüle etmenizi sağlar.

### C. Memory Heatmap (Occupancy)
Birimlerin (Chunk) doluluk oranlarını (`Count / Capacity`) görselleştirerek, bellek baskısını ve verimsiz fragmantasyon alanlarını otomatik olarak saptar.

### D. Integrity Dashboard
`NexusIntegrityChecker` sonuçlarını görselleştirir. ECS dünyasının sağlığını "Nominal", "Degraded" veya "Critical" olarak raporlar ve derinlemesine diyagnostik sunar.

### E. Time-Travel Debugger (Timeline)
Görsel bir zaman çizelgesi (Timeline) ve "Play/Rewind" kontrolleri sunarak, Snapshot'lar arasında saniyeler içinde seyahat etmenizi ve veri akışını izlemenizi sağlar.

---

## 3. Mantıksal Akış
1.  **Bağlantı**: Kokpit araçları `NexusInitializer` veya `Registry` referansını yakalayarak "Live View" moduna geçer.
2.  **Derin Analiz**: Araçlar, unmanaged bellek adreslerini (Raw Pointers) ve meta-data tablolarını tarar.
3.  **Görselleştirme**: Karmaşık veri yoğunlukları ve sistem durumları, Editör GUI'si üzerinde renkli grafikler ve barlar olarak çizilir.
4.  **Müdahale**: Geliştiricinin yaptığı her değişiklik, "Thread-Safe" bir şekilde ECS dünyasına enjekte edilir.

---

## 4. Kullanım Örneği
```csharp
// Kritik bir State bug'ını çözmek:
// 1. [Nexus/Cockpit/Integrity Dashboard] ile genel sağlık kontrolü yapılır.
// 2. [Entity Search Pro] ile hatalı veri üreten entity'ler filtrelenir.
// 3. [Live Tweaker] ile değişkenler canlı olarak "balance" edilir.
// 4. [Time-Travel] ile hatanın başladığı kareye gidilip bellek hizalaması (Alignment) kontrol edilir.
```

---

## Nexus Optimization Tip: Context Injection
Kokpit araçlarını kullanırken `SetContext` metodunu kullanarak aynı Registry üzerindeki farklı araçlar arasında "Senkronize" çalışın. Örneğin, Search Pro'da bulduğunuz bir entity'yi tek tuşla Live Tweaker'a aktarmak, **tanılama süresini %60 oranında kısaltır.**
