using System;
using System.Collections.Generic;

namespace DoAn_Web.Models;

public partial class Industry
{
    public int IndustryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
