using System;
using System.Collections.Generic;

namespace DoAn_Web.Models;

public partial class ApprovalHistory
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime ActionDate { get; set; }

    public int AdminId { get; set; }

    public virtual JobPosting Job { get; set; } = null!;
}
