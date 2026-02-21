# Nexus Prime Mimari Rehberi: NexusMath (Donanım Hızlandırmalı Matematik)

## 1. Giriş
`NexusMath.cs`, Nexus Prime'ın hesaplama gücünün arkasındaki motorudur. Standart matematik kütüphanelerinin aksine, modern işlemcilerin SIMD (Single Instruction, Multiple Data) yeteneklerini doğrudan kullanarak, tek bir saat döngüsünde birden fazla veri üzerinde işlem yapabilmeyi sağlar.

Bu kütüphanenin varlık sebebi; fizik simülasyonları, partikül sistemleri veya yapay zeka gibi yoğun veri işleme (high-throughput) gerektiren senaryolarda CPU darboğazını ortadan kaldırmak ve donanımın teorik limitlerine yaklaşmaktır.

---

## 2. Teknik Analiz
NexusMath, performans için üç katmanlı bir yürütüm stratejisi izler:

- **AVX (Advanced Vector Extensions)**: İşlemci destekliyorsa (modern x86), verileri 256-bit genişliğindeki register'lara yükler. Bu sayede **8 adet float sayıyı tek bir komutla toplar veya çarpar.**
- **SSE (Streaming SIMD Extensions)**: AVX'in olmadığı daha eski işlemcilerde 128-bit register'ları kullanarak 4'lü gruplar halinde işlem yapar.
- **Scalar Fallback**: Eğer veri miktarı 4 veya 8'in katı değilse, kalan elemanlar için standart tekli işlem döngüsüne döner.
- **Aggressive Inlining**: Metodlar `[MethodImpl(MethodImplOptions.AggressiveInlining)]` ile işaretlenmiştir. Bu, metod çağrısı maliyetini (stack frame overhead) sıfıra indirerek kodu doğrudan çağrıldığı yere gömer.

---

## 3. Mantıksal Akış
1.  **Donanım Algılama**: `Avx.IsSupported` veya `Sse.IsSupported` kontrolleriyle CPU yetenekleri saptanır.
2.  **Vektörizasyon**: Veri pointerları üzerinden 8'li (AVX) veya 4'lü (SSE) bloklar halinde bellekten register'lara çekilir.
3.  **Hızlandırılmış İşlem**: Donanım seviyesinde paralel matematiksel işlem yürütülür.
4.  **Belleğe Yazım**: Sonuçlar tekrar hedef bellek adresine (`float*`) boşaltılır.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **SIMD** | Tek bir komutla birden fazla veriyi paralel işleme tekniği. |
| **AVX-256** | 256 bit genişliğinde, 8 float kapasiteli işlemci register seti. |
| **Fast Inverse Sqrt** | Karekökün tersini (`1/√x`) bit seviyesinde "sihirli" bir sabit kullanarak hesaplayan çok hızlı algoritma. |
| **Vectorization** | Seri bir işlemin paralel (vektörel) hale getirilmesi süreci. |

---

## 5. Riskler ve Sınırlar
- **Alignment Requirement**: AVX işlemlerinde en iyi performans için bellek adreslerinin 32-byte hizalı (aligned) olması gerekir. Nexus Prime `NexusLayout` ile bunu garanti eder.
- **Precision**: `FastInverseSqrt` gibi yaklaşıksal (approximate) fonksiyonlar, %100 kesinlik gerektiren finansal hesaplamalar için uygun değildir; oyun mekanikleri için tasarlanmıştır.

---

## 6. Kullanım Örneği
```csharp
// İki devasa float dizisini SIMD ile topla
unsafe {
    float* a = stackalloc float[1024];
    float* b = stackalloc float[1024];
    float* result = stackalloc float[1024];
    
    NexusMath.Add(a, b, result, 1024); // 8'li bloklar halinde toplanır (AVX)
}

// Hızlı interpolasyon
float smoothVal = NexusMath.FastSmoothStep(0, 1, 0.5f);
```

---

## 7. Tam Kaynak Kod (Direct Implementation)

```csharp
public static unsafe class NexusMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(float* a, float* b, float* result, int count)
    {
        int i = 0;
        if (Avx.IsSupported)
        {
            for (; i <= count - 8; i += 8) {
                var va = Avx.LoadVector256(a + i);
                var vb = Avx.LoadVector256(b + i);
                Avx.Store(result + i, Avx.Add(va, vb));
            }
        }
        // Fallback loops...
    }
}
```

---

## Nexus Optimization Tip: Instruction Pipelining
SIMD operasyonlarından en yüksek verimi almak için verilerinizi "Sequential" (ardışık) bellek bloklarında tutun. Dağınık bellek (Cache Miss) durumunda, işlemci SIMD register'larını doldurmak için beklemek zorunda kalır. **Ardışık veri dizilimi, SIMD throughput'unu %300-400 oranında artıracaktır.**
