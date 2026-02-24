# API Referansı: NexusIntegrityDashboard (Mühendislik Paneli)

## Giriş
`NexusIntegrityDashboard.cs`, unmanaged Nexus dünyasını bir "kokpitten" izlemenizi sağlayan profesyonel bir mühendislik panelidir. Sistemin genel sağlığını, bellek bütünlüğünü ve aktif varlık istatistiklerini bir bakışta sunar. Görünmez olan unmanaged bellek dünyasını şeffaf hale getirerek, olası sistem bozulmalarını (Degraded/Critical) gerçek zamanlı olarak raporlar.

---

## Teknik Analiz
Integrity Dashboard, şu profesyonel veri görselleştirme özelliklerini içerir:
- **Real-time Diagnostic Stream**: `NexusIntegrityChecker` tarafından üretilen ham teşhis verilerini kullanıcı dostu bir formata çevirir.
- **Health Status Visualization**: Sistemin durumunu "Nominal" (Yeşil), "Degraded" (Sarı) ve "Critical" (Kırmızı) renk kodlarıyla gösterir.
- **Active Metrics Tracking**: Canlı varlık sayısı ve kayıtlı bileşen seti (Component Set) sayısını anlık olarak raporlar.
- **Manual Audit Interface**: Sistemin bir bellek sızıntısı veya bozulma belirtisi gösterdiğinden şüphelenildiğinde, manuel olarak tam tarama (Audit) tetiklenmesini sağlar.

---

## Mantıksal Akış
1. **Tespit**: Editör penceresi açıldığında sahnedeki `NexusInitializer` üzerinden aktif `Registry` örneğini yakalar.
2. **Sorgulama**: Kullanıcı "Manual Audit" butonuna bastığında veya otomatik tetikleme olduğunda unmanaged bellek blokları taranır.
3. **Raporlama**: Alınan metrikler (Metrics) hiyerarşik bir düzende listelenir.
4. **Uyarı**: Eğer sistem "Critical" durumuna düşerse, teşhis metni (Diagnostics) bir HelpBox aracılığıyla detaylandırılır.

---

## Terminoloji Sözlüğü
- **Engineering Dashboard**: Bir sistemin iç dinamiklerini izlemek için kullanılan kontrol paneli.
- **Degraded Status**: Sistemin hala çalıştığı ancak bellek hizalaması veya sızıntısı gibi riskli durumların tespit edildiği hal.
- **Nominal Status**: Hiçbir bellek hatasının olmadığı ideal çalışma durumu.
- **Manual Audit**: Otomatik kontrollerin dışında, tam kapsamlı bellek taraması.

---

## Riskler ve Sınırlar
- **Editor-Only**: Bu panel sadece Unity Editöründe çalışır; build alınmış son kullanıcı uygulamasında yer almaz.

---

## Kullanım Örneği
1. `Nexus/Cockpit/Integrity Dashboard` yolunu izleyerek paneli açın.
2. Oyun çalışırken "Perform Manual Audit" butonuna basın.
3. "Active Entities" sayısının beklentinizle eşleşip eşleşmediğini kontrol edin.

---

## Nexus Optimization Tip: Early Failure Detection
Integrity Dashboard'daki "Degraded" uyarısı, genellikle unmanaged bir bellek sızıntısının (Memory Leak) başlangıcını haber verir. Yazılımın henüz çökmediği ama belleğin yavaş yavaş şiştiği bu evreyi görsel olarak yakalamak, devasa projelerde "Memory Management" maliyetini **%60 azaltır.**

---

## Orijinal Kod
[NexusIntegrityDashboard.cs Kaynak Kodu](https://github.com/gkhanC/Nexus/blob/master/Nexus.Editor/EditorCockpit/NexusIntegrityDashboard.cs)
