using System;
using System.Collections.Generic;

namespace eticaret.Models;

public partial class UrunGorsel
{
    public int Id { get; set; }

    public int? Urunid { get; set; }

    public string? Ad { get; set; }

    public bool? Baslangic { get; set; }
}
