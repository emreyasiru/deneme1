using System;
using System.Collections.Generic;

namespace eticaret.Models;

public partial class MisafirDetay
{
    public int Id { get; set; }

    public int? Musteriid { get; set; }

    public int? Il { get; set; }

    public int? Ilce { get; set; }

    public string? Adres { get; set; }

    public string? Cinsiyet { get; set; }

    public string? Telefon { get; set; }

    public string? Tc { get; set; }
}
