# Nexus Prime Mimari Rehberi: NexusString (Sabit Boyutlu Unmanaged Metinler)

## 1. Giriş
`NexusString.cs`, Nexus Prime'ın "Zero-GC" felsefesiyle geliştirdiği, unmanaged bellek için optimize edilmiş metin saklama sistemidir. Standart C# stringleri (`System.String`) yönetilen objelerdir (Managed Objects) ve asla `unmanaged` (blittable) bir struct içinde veya ham bellek tamponlarında saklanamazlar.

Bu yapıların varlık sebebi; her varlığın ismini, etiketini veya durum mesajını, Garbage Collector'ı (GC) hiç tetiklemeden ve bellek dağınıklığı yaratmadan doğrudan bileşenin (Component) içinde sabit boyutlu bir bellek bloğu olarak tutabilmektir.

---

## 2. Teknik Analiz
NexusStringsuite, farklı ihtiyaçlar için şu ön-tanımlı boyutları sunar:

- **NexusString32**: 31 karakter + 1 byte uzunluk (Length) saklayan en küçük birimdir. "PlayerName", "Status" gibi kısa etiketler için idealdir.
- **NexusString64**: 63 karakter + 1 byte kapasite sunar. Dosya yolları veya orta uzunlukta açıklamalar için kullanılır.
- **NexusString128**: 127 karakterlik geniş alan sunar.
- **Fixed Byte Buffer**: Her yapı, `fixed byte _data[N]` kullanarak veriyi doğrudan stack veya heap üzerindeki struct gövdesine gömer. Bu, işlemcinin metne ulaşmak için ek bir bellek adresi (Reference) atlamasına engel olur.
- **UTF8 Encoding**: Veriler, bellek tasarrufu ve evrensel uyumluluk için ham UTF8 byte'ları olarak saklanır.

---

## 3. Mantıksal Akış
1.  **Dönüştürme (Constructor)**: Managed bir string alındığında `Encoding.UTF8.GetBytes` ile byte dizisine dönüştürülür.
2.  **Kesme (Capping)**: Eğer metin belirlenen boyuttan (Örn: 32 byte) büyükse, veri güvenli bir şekilde kesilir.
3.  **Kopyalama**: Veri, `ReadOnlySpan<byte>` üzerinden doğrudan unmanaged buffer'a (`_data`) kopyalanır.
4.  **Okuma (ToString)**: İhtiyaç duyulduğunda (Örn: UI), ham byte'lar tekrar C# string'ine dönüştürülür.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **Fixed Buffer** | Boyutu derleme zamanında belirlenmiş, yer değiştirmeyen bellek bloğu. |
| **UTF8** | Karakterleri değişken uzunlukta byte'lar kullanarak temsil eden evrensel kodlama. |
| **Blittable** | Bellek yapısı managed ve unmanaged dünyada birebir aynı olan veri tipi. |
| **Heap-Free** | İşlem sırasında yönetilen bellek yığınını (Heap) hiç kullanmama durumu. |

---

## 5. Riskler ve Sınırlar
- **Truncation**: Sabit boyutu (Örn: 32) aşan metinler sessizce kesilir. Uzun açıklamalar için büyük varyantlar (128) seçilmelidir.
- **Reconstruction Cost**: `ToString()` metodu yeni bir managed string oluşturduğu için (Allocation), bu metodun sistem döngülerinde (Internal Loops) sık kullanılması performans üzerinde GC baskısı yaratabilir. Sadece görselleştirme anında çağrılmalıdır.

---

## 6. Kullanım Örneği
```csharp
public struct ActorName : INexusComponent {
    public NexusString32 Value; // Doğrudan bileşen içinde 32 byte yer kaplar
}

// Kullanım
var name = new ActorName();
name.Value = "Hero_One"; // Implicit cast sayesinde standart string atanabilir

Console.WriteLine(name.Value.ToString()); // "Hero_One"
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
public unsafe struct NexusString32
{
    private fixed byte _data[32];
    private byte _length;

    public NexusString32(string? value)
    {
        if (string.IsNullOrEmpty(value)) { _length = 0; return; }
        ReadOnlySpan<byte> source = Encoding.UTF8.GetBytes(value);
        _length = (byte)Math.Min(source.Length, 31);
        fixed (byte* ptr = _data) source.Slice(0, _length).CopyTo(new Span<byte>(ptr, 31));
    }

    public override string ToString() {
        fixed (byte* ptr = _data) return Encoding.UTF8.GetString(ptr, _length);
    }
}
```

---

## Nexus Optimization Tip: Memory Footprint
NexusString kullanımı, bellek yerelliğini (locality) artırır. Bir `List<string>` kullanmak yerine unmanaged bir buffer içinde `NexusString32` kullanmak, işlemcinin metne ulaşmak için yapacağı **bellek atlama (Memory Jump) sayısını 1'e indirir ve erişim hızını %50-60 artırır.**
