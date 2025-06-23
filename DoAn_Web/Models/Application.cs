using System;
using System.Collections.Generic;

namespace DoAn_Web.Models;

public partial class Application
{
    public int ApplicationId { get; set; }

    public int JobId { get; set; }

    public int StudentId { get; set; }

    public string? CoverLetter { get; set; }

    public string ResumeUrl { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? AppliedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();

    public virtual JobPosting Job { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
