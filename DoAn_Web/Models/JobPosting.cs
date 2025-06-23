using System;
using System.Collections.Generic;

namespace DoAn_Web.Models;

public partial class JobPosting
{
    public int JobId { get; set; }

    public int CompanyId { get; set; }

    public int JobTypeId { get; set; }

    public int LevelId { get; set; }

    public int? LocationId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Requirements { get; set; } = null!;

    public string? Benefits { get; set; }

    public string? SalaryRange { get; set; }

    public DateTime ApplicationDeadline { get; set; }

    public int? Vacancies { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsApproved { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<ApprovalHistory> ApprovalHistories { get; set; } = new List<ApprovalHistory>();

    public virtual Company Company { get; set; } = null!;

    public virtual JobType JobType { get; set; } = null!;

    public virtual ExperienceLevel Level { get; set; } = null!;

    public virtual Location? Location { get; set; }

    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
