using System;
using System.Collections.Generic;

namespace eticaret.Models;

public partial class Urunler
{
    public int Id { get; set; }

    public string UrunAdi { get; set; } = null!;

    public int Stok { get; set; }

    public int KategoriId { get; set; }

    public decimal? Alis { get; set; }

    public decimal Satis { get; set; }

    public int VergiId { get; set; }

    public decimal? IndirimliFiyat { get; set; }

    public string? Aciklama { get; set; }
}
