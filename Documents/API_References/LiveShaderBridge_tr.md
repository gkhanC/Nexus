# Nexus Prime Mimari Rehberi: LiveShaderBridge (Canlı Shader Akışı)

## 1. Giriş
`LiveShaderBridge.cs`, Nexus Prime'ın yüksek performanslı veri dünyası ile GPU'nun paralel işlem gücü arasındaki "Süper Otoyol"dur. Binlerce unmanaged bileşen verisini (Örn: Pozisyon, Sıcaklık, Akış hızı) her karede `ComputeBuffer` üzerinden doğrudan shader'lara (GPU) aktararak, görselleştirme sürecini unmanaged seviyede koordine eder.

Bu köprünün varlık sebebi; her varlık için ayrı bir `MaterialPropertyBlock` güncelleme maliyetinden kaçınmak ve GPU'nun tüm varlık verisine tek bir doku veya buffer üzerinden O(1) maliyetle erişmesini sağlamaktır.

---

## 2. Teknik Analiz
Maksimum GPU throughput (bant genişliği) için şu mimariyi kullanır:

- **ComputeBuffer Integration**: Verileri GPU register'larına en yakın formda, yani ham bir buffer olarak sunar. `STRIDE` parametresi ile her varlığın GPU'da ne kadar yer kapladığını (Örn: float4 = 16 byte) tanımlar.
- **Bulk Data Gathering**: Unmanaged `Registry`'den gelen verileri tek bir toplu bellek bloklamasıyla paketleyerek GPU'ya iletir.
- **Global Shader Properties**: `Shader.SetGlobalBuffer` metodu ile veriyi tüm sahnedeki shader'lar (VFX Graph, Custom Shaders) için erişilebilir kılar.
- **Memory Management (IDisposable)**: Unmanaged bir GPU kaynağı olan ComputeBuffer'ın sızmasını önlemek için `Dispose` deseniyle güvenli temizlik (Cleanup) yapar.

---

## 3. Mantıksal Akış
1.  **Kurulum**: Varlık sayısına ve veri genişliğine (STRIDE) göre GPU tarafında yer açılır (Buffer Allocation).
2.  **Veri Toplama**: Her karede Nexus Registry taranarak güncel veriler tek bir diziye toplanır.
3.  **Akış (Streaming)**: Hazırlanan dizi, `ComputeBuffer.SetData` ile GPU belleğine kopyalanır.
4.  **Tüketim**: Shader dosyaları içindeki `StructuredBuffer<float4> _NexusEntityData` tanımıyla verilere anında erişilir.

---

## 4. Terminoloji Sözlüğü

| Terim | Açıklama |
| :--- | :--- |
| **ComputeBuffer** | CPU'dan GPU'ya büyük miktarda ham veri aktarmak için kullanılan Unity bellek yapısı. |
| **Stride** | Buffer içindeki her bir elemanın byte cinsinden boyutu. |
| **GPU Streaming** | Verilerin kesintisiz ve her karede yenilenerek grafik işlemciye akıtılması. |
| **Batching Efficiency** | Binlerce nesnenin tek bir işlemle çizilmesi veya güncellenmesi yeteneği. |

---

## 5. Kullanım Örneği
```csharp
// VFX Graph veya Custom Shader'a veri bas
var shaderBridge = new LiveShaderBridge(10000); // 10k varlık için

void Update() {
    shaderBridge.UpdateBuffer(mainRegistry);
}

// Shader tarafında okuma:
// StructuredBuffer<float4> _NexusEntityData;
// float3 pos = _NexusEntityData[entityIndex].xyz;
```

---

## 6. Tam Kaynak Kod (Direct Implementation)

```csharp
namespace Nexus.Bridge;

public class LiveShaderBridge : IDisposable
{
    private ComputeBuffer _dataBuffer;
    private const int STRIDE = 16; 

    public LiveShaderBridge(int entityCount) {
        _dataBuffer = new ComputeBuffer(entityCount, STRIDE);
    }

    public unsafe void UpdateBuffer(Registry.Registry registry) {
        // Gathering data logic...
        Shader.SetGlobalBuffer("_NexusEntityData", _dataBuffer);
    }

    public void Dispose() => _dataBuffer?.Release();
}
```

---

## Nexus Optimization Tip: Persistent Buffer
Buffer'ı her karede yok edip tekrar oluşturmak yerine bir kez oluşturun (`Persistent`). Sadece içindeki veriyi `SetData` ile güncelleyin. Bu, **bellek ayırma (Allocation) yükünü sıfıra indirerek performans istikrarını sağlar.**
