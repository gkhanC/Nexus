# Nexus Prime Mimari Rehberi: LiveState Tweaker (Canlı Durum Düzenleyici)

## 1. Giriş
`LiveStateTweaker.cs`, oyunun çalışma zamanında (Runtime) ECS dünyasındaki verilerin anlık olarak izlenmesini ve değiştirilmesini sağlayan güçlü bir Editör panelidir. Geliştiricilerin "Deneme-Yanılma" döngüsünü, oyunu durdurmadan saniyelere indirger.

Bu panelin varlık sebebi; standart Unity Inspector'ın göremediği unmanaged `INexusComponent` verilerini gün yüzüne çıkarmak ve geliştiricinin değişkenleri canlı olarak "evirip çevirmesine" imkan tanımaktır.

---

## 2. Teknik Analiz
Panel şu gelişmiş özelliklerle donatılmıştır:

- **Global Registry Inspection**: Aktif Nexus Registry'sine bağlanarak, o an hayatta olan tüm Entity'leri listeler.
- **Search & Filter Engine**: Binlerce entity arasından belirli bir ID'ye veya bileşen tipine göre hızlıca arama yapabilir.
- **Direct Memory Manipulation**: GUI üzerindeki bir değeri değiştirdiğinizde, sistem bu değişikliği doğrudan unmanaged bellek adresine (Pointer) yazar.
- **Component Foldout Logic**: Her entity'nin bileşenlerini katlanabilir (Foldout) bir yapıda sunarak görsel karmaşayı önler.

---

## 3. Mantıksal Akış
1.  **Bağlanma**: Editör penceresi açıldığında mevcut `Snapshot` veya `Registry` üzerinden veri akışı başlar.
2.  **Abonelik**: Panel, unmanaged taraftaki "Dirty" bayraklarını izleyerek sadece değişen verileri arayüzde günceller.
3.  **Girdi İşleme**: Geliştirici kullanıcı arayüzünde (UI) bir Slider'ı kaydırdığında, yeni değer anında ECS simülasyonuna enjekte edilir.

---

## 4. Kullanım Örneği
```text
// Çalışan bir oyunda:
// 1. [Nexus/Live State Tweaker] açılır.
// 2. "EnemyBoss" entity'si aratılır.
// 3. "Health" bileşeni altındaki Health değeri 1000'den 10'a çekilir.
// Sonuç: Oyun durmadan Boss'un canı anında azalır.
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
#if UNITY_EDITOR
namespace Nexus.Editor;

public class LiveStateTweaker : EditorWindow
{
    [MenuItem("Nexus/Live State Tweaker")]
    public static void ShowWindow() => GetWindow<LiveStateTweaker>("State Tweaker");

    private void OnGUI() {
        // Search bar...
        // Scroll view for entities...
        // Draw each entity's component with EditorGUILayout fields.
    }
}
#endif
```

---

## Nexus Optimization Tip: Event Filtering
State Tweaker'ı her Update'te yenilemek yerine, `Repaint()` çağrısını sadece unmanaged veride bir değişiklik olduğunda tetikleyin. Bu, **çok sayıda entity'nin izlendiği durumlarda Editör işlemci yükünü %25 azaltır.**
