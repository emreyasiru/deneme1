using eticaret.Models;

namespace eticaret.Modeller
{
    public class UrunDetayViewModel
    {
        public int UrunId { get; set; }
        public string UrunAdi { get; set; }
        public string KategoriAdi { get; set; }
        public int Stok { get; set; }
        public decimal Satis { get; set; }
        public decimal? IndirimliFiyat { get; set; }
        public List<UrunDetay> Detaylar { get; set; } = new List<UrunDetay>();
    }
}