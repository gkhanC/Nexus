# Nexus Prime Mimari Rehberi: NexusAttributes (Mühendislik Öznitelikleri)

## 1. Giriş
`NexusAttributes.cs`, hem Unity Inspector görselleştirmesini düzenleyen hem de Nexus ECS sistemine veri yönetimi hakkında ipuçları veren bir "Öznitelik Paketi"dir. Kodun hem editör tarafında hem de unmanaged simülasyon tarafında nasıl davranacağını belirleyen metadata katmanıdır.

Bu özniteliklerin varlık sebebi; "Decoration" (Süsleme) mantığını kullanarak, ekstra kod yazmadan veri güvenliğini (`ReadOnly`), görsel hiyerarşiyi (`ProgressBar`) ve sistem davranışlarını (`Sync`, `Persistent`) kontrol etmektir.

---

## 2. Teknik Analiz
Paket içinde iki ana tip öznitelik grubu bulunur:

### A. Görsel & Düzenleyici Öznitelikler
- **[ReadOnly]**: Alanın Inspector'da görünmesini sağlar ancak elle değiştirilmesini engeller.
- **[Required]**: Kritik referansları işaretler. Eğer alan boşsa Inspector'da uyarı fırlatır.
- **[NexusProgressBar]**: Sayısal değerleri (Can, Enerji vb.) görsel bir bar olarak çizer.
- **[MinMaxSlider]**: Belirlenen aralıkta iki uçlu (Min/Max) bir kaydırıcı sunar.

### B. Mimari & ECS Öznitelikleri
- **[Sync]**: Bir unmanaged verinin Unity `Transform` veya `Physics` dünyasıyla otomatik senkronize edileceğini belirtir.
- **[Persistent]**: Verinin sahne geçişlerinde bellekten atılmamasını ve kayıt (Save) sistemine dahil edilmesini sağlar.
- **[BitPacked]**: Verinin iletim sırasında bit seviyesinde sıkıştırılması gerektiğini bildiren (Source Generator'a yönelik) bir bayraktır.

---

## 3. Mantıksal Akış
1.  **Tanımlama**: Geliştirici bileşen alanını `[Sync]` veya `[ReadOnly]` ile işaretler.
2.  **Editör Analizi**: Unity'nin `PropertyDrawer` mekanizması bu bayrağı görür ve özel çizim yapar.
3.  **Simülasyon Entegrasyonu**: Nexus Source Generator, `[Sync]` etiketli alanlar için otomatik senkronizasyon kodunu üretir.
4.  **Kalıcılık**: `[Persistent]` etiketli veriler `SnapshotManager` tarafından otomatik olarak yakalanır.

---

## 4. Kullanım Örneği
```csharp
public struct HealthComponent : INexusComponent {
    [NexusProgressBar(100, "Red")]
    public float Current;

    [Sync(SyncTarget.UI)]
    public float Max;
}

[Persistent]
public struct PlayerData : INexusComponent { ... }
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Attributes;

public class ReadOnlyAttribute : PropertyAttribute { }
public class RequiredAttribute : PropertyAttribute { }
public class SyncAttribute : Attribute {
    public SyncTarget Target { get; }
    public SyncAttribute(SyncTarget target = SyncTarget.Transform) => Target = target;
}
```

---

## Nexus Optimization Tip: Attribute Stripping
`[Persistent]` veya `[Sync]` gibi mimari öznitelikler sadece çalışma zamanında sistemler tarafından okunur. Ancak `[ReadOnly]` gibi görsel öznitelikler sadece Editör'de (Build dışında) gereklidir. Build boyutunu küçültmek için Editör-only öznitelikleri `#if UNITY_EDITOR` blokları içine almayı düşünün.
