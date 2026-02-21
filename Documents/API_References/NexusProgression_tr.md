# Nexus Prime Mimari Rehberi: NexusProgression (Seviye ve İlerleme Sistemi)

## 1. Giriş
`NexusProgression.cs`, RPG ve ilerleme temelli oyun mekanikleri için tasarlanmış unmanaged bir veri yapısıdır. HypeFire mimarisinden miras alınan bu yapı, tecrübe puanı (XP), yetenek geliştirme veya görev ilerlemesi gibi durumları takip etmek için optimize edilmiş bir "Akümülatör" (Biriktirici) görevi görür.

Bu yapının varlık sebebi; her varlığın (NPC, Oyuncu vb.) ilerleme durumunu managed objeler üzerinden değil, doğrudan bileşenin (Component) bellek bloğunda tutarak, binlerce birimin aynı anda seviye atlama kontrolünü sıfır GC maliyetiyle yapabilmektir.

---

## 2. Teknik Analiz
NexusProgression, veri bütünlüğü için şu alanları yönetir:

- **CurrentProgress**: Mevcut birikmiş miktar (Örn: 250 XP).
- **Goal**: Bir sonraki seviyeye geçmek için gereken hedef miktar (Örn: 1000 XP).
- **FillRatio**: UI katmanı için mevcut ilerlemenin hedefe oranını (0.0 - 1.0) hesaplar.
- **Auto-Leveling Logic**: `Add` metodu içinde bulunan `while` döngüsü sayesinde, bir kerede büyük miktarda puan gelmesi durumunda (Örn: 5000 XP) ardışık seviye atlamaları (Multiple Level-ups) doğru bir şekilde hesaplanır.

---

## 3. Mantıksal Akış
1.  **Girdi**: `Add(amount)` metoduyla sisteme yeni ilerleme puanı girer.
2.  **Kontrol**: Mevcut miktar hedefi (Goal) aşıp aşmadığı kontrol edilir.
3.  **Seviye Artırımı**: Hedef aşıldığı sürece `Level` artırılır ve aşan miktar `CurrentProgress` içinde bir sonraki seviyenin başlangıç puanı olarak korunur.
4.  **Sıfırlama**: İlerleme, hedeften çıkarılarak "Modulo" benzeri bir mantıkla devredilir.

---

## 4. Kullanım Örneği
```csharp
public struct PlayerLevel : INexusComponent {
    public NexusProgression XP;
}

// Kullanım
ref var xp = ref registry.Get<PlayerLevel>(e).XP;
xp.Goal = 1000;
xp.Add(2500); // Level 2 artar, 500 XP kalır.
float uiFill = xp.FillRatio; // 0.5f (500/1000)
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Data;

public struct NexusProgression
{
    public float CurrentProgress;
    public float Goal;
    public int Level;

    public float FillRatio => Goal > 0 ? CurrentProgress / Goal : 0;

    public void Add(float amount)
    {
        CurrentProgress += amount;
        while (CurrentProgress >= Goal && Goal > 0)
        {
            CurrentProgress -= Goal;
            Level++;
        }
    }
}
```

---

## Nexus Optimization Tip: Carry-over Precision
`while` döngüsü kullanmak, tecrübe puanlarının "kaybolmasını" (Lost carry-over) önler. Tek bir `if` kontrolü yerine bu yapıyı kullanmak, yüksek puanlı ödüllerde **seviye hesaplama hatalarını %100 oranında ortadan kaldırır.**
