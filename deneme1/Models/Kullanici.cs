using System;
using System.Collections.Generic;

namespace eticaret.Models;

public partial class Kullanici
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool? Durum { get; set; }
}
