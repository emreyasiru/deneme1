using eticaret.Models;

namespace eticaret.Modeller
{
    public class UrunListesi
    {
        public List<AnaKategori> Kategorilerim { get; set; }
        public List<Urunler> Urunlerim { get; set; }
        public List<UrunGorsel>UrunGorsellerim { get; set; }
        public List<AltKategori>Altkategorilerim { get; set; }
        public List<UrunDetay> UrunDetaylarım { get; set; }
    }
}
