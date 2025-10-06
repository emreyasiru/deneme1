using System;
using System.Collections.Generic;

namespace eticaret.Models;

public partial class Vergi
{
    public int Id { get; set; }

    public string? Adi { get; set; }

    public int? Orani { get; set; }
}
