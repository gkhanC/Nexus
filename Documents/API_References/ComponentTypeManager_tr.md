# Nexus Prime Mimari Rehberi: ComponentTypeManager (Hızlı Tip Kimlik Hattı)

## 1. Giriş
`ComponentTypeManager.cs`, Nexus Prime'ın "Sıfır Sözlük" (Zero Dictionary) politikasının baş mimarıdır. ECS sistemlerinde bileşen depolarına erişmek için tip adıyla (`typeof(T)`) yönetilen (managed) sözlük araması yapmak, saniyede binlerce kez dönen yoğun döngülerde yıkıcı bir CPU maliyeti oluşturur.

Bu yöneticinin varlık sebebi, her bileşen tipine çalışma zamanında (runtime) benzersiz, ardışık artan bir tam sayı (`int`) atayarak, bileşen depolarına **$O(1)$ mutlak dizin erişimi** sağlamaktır. Bu sayede, "Hangi tipte veri arıyoruz?" sorusunun cevabı karmaşık string hash'leriyle değil, doğrudan fiziksel bellek dizinleri üzerinden verilir.

---

## 2. Teknik Analiz ve Matematiksel İçyüzü
`ComponentTypeManager`, işlemci başına harcanan döngü (cycle) sayısını mimarinin kaldırabileceği en dip limite indirmek için CLR yeteneklerini donanım seviyesinde aşağıda gösterildiği gibi kullanır.

$$T_{Sozluk(Dictionary)} \approx 60 \ dongu \gg T_{StatikOnbellek} \approx 1 \ dongu$$

```mermaid
graph TD
    subgraph JIT_Optimize_Tip_Haritalamasi
        A[Cagri: GetId<Velocity>] --> B{TypeIdHolder<Velocity> Ilklendi mi?}
        B -->|Evet| C[Onbelleklenmis Sabit ID'yi Don: 5]
        B -->|Hayir: Ilk Cagri| D[Interlocked.Increment Global ID]
        D --> E[Statik Readonly Value icine 5 ID'sini Muhurle]
        E --> C
        
        C -.->|O 1 Dizi Indeks Aramasi| F(Registry._componentSetsArr[ 5 ])
    end
    style C fill:#ddffdd
    style F fill:#ddffff
```

---

## 3. Tam Kaynak Kod Uygulaması ve Satır Satır Açıklama
Bu sınıfta hiçbir orijinal mantık eksiltilmemiş olup, kesin üretim kodu analizi aşağıdadır.

```csharp
// Kaynak Kod (Source Code)
using System.Threading;
namespace Nexus.Core;

public static class ComponentTypeManager
{
    private static int _nextId = 0;

    public static int GetId<T>() where T : unmanaged
    {
        return TypeIdHolder<T>.Value;
    }

    private static class TypeIdHolder<T> where T : unmanaged
    {
        public static readonly int Value = Interlocked.Increment(ref _nextId) - 1;
    }

    public static int MaxTypes => _nextId;
}
```

### Satır Satır Kod Açıklaması (Line-By-Line Breakdown)
- `public static class ComponentTypeManager`: **(Satır 5)** Kesin şekilde `static` ilan edilmiştir; bu, CLR'ın sınıfı ekosistem merkezinde tek bir global (singleton) olarak tanımlayacağı anlamına gelir.
- `private static int _nextId = 0;`: **(Satır 7)** Tekil, sürekli artan global bir sayaç takipçisi. Tiplerin tescilasyon (registration) kronolojisini sıraya dizer.
- `public static int GetId<T>() where T : unmanaged`: **(Satır 9)** `unmanaged` kısıtlamasını dayatarak, sınıfımıza (`class`) ya da referans tiplerine ait hatalı girişimi engeller, tüm veri dizini baştan Blittable (donanımsal uyumlu) ilan eder.
- `return TypeIdHolder<T>.Value;`: **(Satır 11)** Dahili jenerik statik sınıfa fiziksel erişim emridir. Özel korumalı veya soyut bloklardan kaçınarak direkt yapı sınırları hedeflenir.
- `private static class TypeIdHolder<T>`: **(Satır 14)** "Tip Silme" (Type Erasure) mimarisinin kalbi. Microsoft CLR altyapısı, çalışma zamanında (runtime) gönderilen her ayrı `T` için bellekte tamamıyla yepyeni, bağımlılıksız bir sanal statik alt sınıf çakmasını garanti eder.
- `public static readonly int Value = ...`: **(Satır 16)** `TypeIdHolder` tetiklendiği mutlak milisaniyede `Interlocked` thread-race ihtimalinden kaçıp `_nextId`'yi kilitler. Üretilen sayıyı `readonly` ile mühürleyerek donanım seviyesi JIT işlemci yönergelerine bir sayısal `sabit` (constant) olarak sonsuza dek perçinler.
- `public static int MaxTypes`: **(Satır 19)** Registry dizi bloklarının bellek içi limit ölçümlemelerini ayarlamasına / genişlemesine fırsat veren total tepe noktasını iletir.

---

## 4. Kullanım Senaryosu ve En İyi Uygulamalar
Veri bağlamını (memory mapping) asla Dize adları (string names) ya da Dictionary kullanarak yönetmeyin. Kimlikleri doğrudan okuyun.

```csharp
// Senaryo: Ozel bir C# kütüphanesi uzantisi, fizik objelerini tespit etmek istiyor.

// Neredeyse sıfır donanım harcamasıyla JIT önbelleklenen ID'leri çöz.
int velocityID = ComponentTypeManager.GetId<Velocity>();
int torqueID = ComponentTypeManager.GetId<Torque>();

// Anlık aktif veri düzeni limitine (MaxTypes) en ideal dizi (Array) kalıbını çıkarma.
ISparseSet[] _frameworkSets = new ISparseSet[ComponentTypeManager.MaxTypes];

// Karakter eşleme (string Hash) yerine mükemmel O(1) sabit boyutla doğrudan çekim
ISparseSet targetSet = _frameworkSets[velocityID];
```

> [!WARNING]  
> **Sıralama / Tescil Zamanlaması (Registration Timing)**: `ComponentTypeManager` tiplere kimlik dağıtımını kod tarafından mantıksal olarak ilk çağrıldıkları an yapar. Component ID'si olarak `3` bugün `Health` bileşenine atanmış olabilir, ancak yarın kodunuzun çağrılma sırası değiştirilirse, `Health` bir anda ID `0` olabilir. Bu yüzden `GetId<T>()` fonksiyon sonuçlarını disk üzerindeki kayıt dosyalarına (Save-File) *asla kaydetmeyin* ve *gömmeyin*! Kayıt dosyalarında string isimlendirme (`nameof(Health)`) veya önceden manuel listelenmiş değişmez GUID değerleri saklayın ve dosyayı diske yüklerken onları runtime ID'sine geri map'leyin.
