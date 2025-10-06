using System;
using System.Collections.Generic;

namespace eticaret.Models;

public partial class KullaniciBilgileri
{
    public int Id { get; set; }

    public int? KullaniciId { get; set; }

    public string? Ad { get; set; }

    public string? Soyad { get; set; }

    public string? Telefon { get; set; }

    public int? Yetki { get; set; }

    public string? Mail { get; set; }
}
