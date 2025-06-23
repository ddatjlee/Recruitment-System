using System;
using System.Collections.Generic;

namespace DoAn_Web.Models;

public partial class Interview
{
    public int InterviewId { get; set; }

    public int ApplicationId { get; set; }

    public string InterviewType { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? Location { get; set; }

    public string? OnlineLink { get; set; }

    public string? Notes { get; set; }

    public string? Result { get; set; }

    public virtual Application? Application { get; set; } = null!;
}
