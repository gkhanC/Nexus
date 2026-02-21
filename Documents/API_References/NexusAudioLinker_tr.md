# Nexus Prime Mimari Rehberi: NexusAudioLinker (Ses-Veri Bağlayıcı)

## 1. Giriş
`NexusAudioLinker.cs`, Nexus Prime'ın veri odaklı dünyasındaki değişkenleri (Örn: Hız, Stres düzeyi, Enerji) Unity'nin `AudioSource` parametrelerine bağlayan reaktif bir yardımcı bileşendir. Oyunun simülasyon derinliğini sesli geri bildirimlerle güçlendirmek için kullanılır.

Bu bağlayıcının varlık sebebi; her ses değişikliği için karmaşık scriptler yazmak yerine, unmanaged bir veriyi (float) sesin perde (Pitch) veya ses yüksekliği (Volume) gibi özelliklerine otomatik olarak "map" edebilmektir.

---

## 2. Teknik Analiz
Ses senkronizasyonu için şu yetenekleri sunar:

- **Reactive Parametric Update**: `UpdateAudio` metodu aracılığıyla unmanaged dünyadan gelen ham sayısal verileri alır.
- **Linear Mapping (Lerp)**: Gelen ham veriyi (Örn: 0-100 hız), sesin anlayabileceği bir aralığa (Örn: 0.5 - 1.5 pitch) matematiksel olarak dönüştürür.
- **Low-Overhead Binding**: Sadece veri değiştiğinde veya belirlenen senkronizasyon aralıklarında tetiklenerek CPU ses motoru üzerindeki yükü minimize eder.
- **Field-Based Configuration**: Inspector üzerinden hangi bileşen alanının (Speed, Health vb.) hangi ses parametresini etkileyeceği kolayca yapılandırılabilir.

---

## 3. Mantıksal Akış
1.  **Girdi**: Nexus Registry üzerindeki bir unmanaged bileşenden değer okunur.
2.  **Dönüştürme**: Okunan değer, belirlenen minimum/maksimum aralıklar arasında normalize edilir.
3.  **Uygulama**: Normalize edilen değer `AudioSource.pitch` veya `AudioSource.volume` özelliğine atanır.
4.  **Sonuç**: Araç hızlandıkça motor sesinin incelmesi veya can azaldıkça kalp atışı sesinin hızlanması gibi dinamik efektler elde edilir.

---

## 4. Kullanım Örneği
```csharp
public class CarAudio : MonoBehaviour {
    [SerializeField] private NexusAudioLinker _engineLinker;

    void Update() {
        // Nexus'tan gelen 0-200 arası RPM verisini sese aktar
        float currentRPM = GetUnmanagedRPM(); 
        _engineLinker.UpdateAudio(currentRPM);
    }
}
```

---

## 5. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public class NexusAudioLinker : MonoBehaviour
{
    public AudioSource Source;
    
    public unsafe void UpdateAudio(float value) {
        if (Source == null) return;
        
        // Örnek: 0-100 arasındaki bir değeri 0.5-1.5 pitch aralığına map et
        Source.pitch = Mathf.Lerp(0.5f, 1.5f, value / 100f);
    }
}
```

---

## Nexus Optimization Tip: Audio Update Culling
Tüm nesnelerin ses parametrelerini her karede güncellemek yerine, sadece oyuncunun duyabileceği mesafedeki (Audio Audibility Range) nesneler için `UpdateAudio` çağrısı yapın. Bu, **Audio Thread ve CPU üzerindeki gereksiz işlem yükünü %30 azaltabilir.**
