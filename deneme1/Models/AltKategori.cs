using System;
using System.Collections.Generic;

namespace eticaret.Models;

public partial class AltKategori
{
    public int Id { get; set; }

    public int AnaKategoriId { get; set; }

    public string KategoriAdi { get; set; } = null!;

    public bool Durum { get; set; }

    public int? UstKategoriId { get; set; }
}
