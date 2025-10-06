namespace eticaret.Modeller
{
    public class UrunListeView
    {
        public int Id { get; set; }
        public string UrunAdi { get; set; }
        public int Stok { get; set; }
        public int KategoriId { get; set; }
        public string KategoriAdi { get; set; } // Kategori adını göstermek için
        public decimal? Alis { get; set; }
        public decimal Satis { get; set; }
        public decimal? IndirimliFiyat { get; set; }
        public string? Aciklama { get; set; }
    }
}