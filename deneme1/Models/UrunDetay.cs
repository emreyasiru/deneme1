using System;
using System.Collections.Generic;

namespace eticaret.Models;

public partial class UrunDetay
{
    public int Id { get; set; }

    public int? Urunid { get; set; }

    public int? Numara { get; set; }

    public string? Beden { get; set; }

    public string? Renk { get; set; }

    public string? Boy { get; set; }

    public string? Marka { get; set; }
}
