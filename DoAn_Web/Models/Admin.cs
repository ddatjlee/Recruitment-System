using System;
using System.Collections.Generic;

namespace DoAn_Web.Models;

public partial class Admin
{
    public int AdminId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
