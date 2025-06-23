using System;
using System.Collections.Generic;

namespace DoAn_Web.Models;

public partial class ExperienceLevel
{
    public int LevelId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}
