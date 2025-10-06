using System;
using System.Collections.Generic;

namespace eticaret.Models;

public partial class Misafir
{
    public int Id { get; set; }

    public string? Isim { get; set; }

    public string? Mail { get; set; }

    public string? Sifre { get; set; }

    public string? Kod { get; set; }

    public bool? Durum { get; set; }
}
