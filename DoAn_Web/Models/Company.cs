using System;
using System.Collections.Generic;

namespace DoAn_Web.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string TaxCode { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Website { get; set; }

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? Address { get; set; }

    public bool? Verified { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsLocked { get; set; }

    public virtual ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}
