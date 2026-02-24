# API Referansı: NexusObjectPool (Nesne Havuzlama)

## Giriş
`NexusObjectPool.cs`, Unity'nin pahalı `Instantiate` ve `Destroy` operasyonlarına karşı geliştirilmiş yüksek performanslı bir havuzlama (pooling) sistemidir. Oyun sırasında sıkça yaratılıp yok edilen varlıkları (mermiler, efektler vb.) bellekten silmek yerine pasif hale getirip bir kuyrukta bekletir. Bu sayede Garbage Collector (GC) üzerindeki yükü kaldırarak kare hızı dalgalanmalarını (lag spikes) önler.

---

## Teknik Analiz
Havuzlama sistemi şu mekanizmalarla çalışır:
- **Queue-Based Storage**: Her prefab tipi için ayrı bir `Queue<GameObject>` tutarak O(1) hızında erişim sağlar.
- **INexusPoolable Interface**: Havuzdan çıkan veya havuza giren nesnelerin durumlarını sıfırlayabilmesi (OnSpawn/OnDespawn) için bir yaşam döngüsü arayüzü sunar.
- **Dynamic Growth**: Eğer havuzda boş nesne yoksa, sistem otomatik olarak yeni bir tane oluşturur (Instantiate).
- **Name-Key Mapping**: Prefab isimlerini anahtar (key) olarak kullanarak farklı tipteki objeleri organize eder.

---

## Mantıksal Akış
1. **Talep**: `Spawn` metodu çağrıldığında, ilgili prefaba ait kuyruk kontrol edilir.
2. **Yeniden Kullanım**: Eğer kuyrukta obje varsa, aktif edilir, pozisyonu ayarlanır ve `OnSpawn` tetiklenir.
3. **Tahliye**: `Despawn` çağrıldığında, `OnDespawn` tetiklenir, obje pasif hale getirilir ve ait olduğu kuyruğa geri döner.
4. **Temizlik**: Sahne yüklendiğinde veya uygulama kapandığında tüm havuzlar serbest bırakılır.

---

## Terminoloji Sözlüğü
- **Object Pooling**: Nesnelerin yok edilmeyip tekrar kullanılmak üzere saklanması tekniği.
- **Lag Spike**: İşlemcinin ağır bir işlem (örn: GC) nedeniyle kare üretiminde gecikme yaşaması.
- **Active/Inactive State**: Bir nesnenin Unity hiyerarşisinde görünürlük ve çalışma durumu.
- **Lifecycle Hook**: Bir nesnenin belirli aşamalarında (yaratılma, yok olma) çalışan kod parçacıkları.

---

## Riskler ve Sınırlar
- **State Reset**: `OnDespawn` içinde nesnenin durumu (hız, sağlık, görsel efektler) manuel olarak sıfırlanmazsa, tekrar spawn edildiğinde eski verilerle görünebilir. 

---

## Kullanım Örneği
```csharp
GameObject bullet = NexusObjectPool.Spawn(bulletPrefab, pos, rot);
// ... mermi işi bitince
NexusObjectPool.Despawn(bullet);
```

---

## Nexus Optimization Tip: Pre-Warming Pools
Kritik sahnelerin başında (loading ekranında), sık kullanılan nesneleri önceden spawn edip hemen despawn ederek havuzu "ısıtın". Bu, **savaşın en yoğun anında oluşabilecek ilk Instantiate maliyetini önceden ödemenizi sağlar.**

---

## Orijinal Kod
[NexusDevelopmentTools.cs Kaynak Kodu](https://github.com/gkhanC/Nexus/blob/master/Nexus.Unity/Core/NexusDevelopmentTools.cs)
