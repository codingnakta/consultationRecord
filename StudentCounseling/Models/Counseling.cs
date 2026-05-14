using System;
using System.Collections.Generic;

namespace StudentCounseling.Models;

public class Counseling
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<Guid> StudentIds { get; set; } = new();
    public Guid? GroupId { get; set; }
    public CounselingType Type { get; set; } = CounselingType.개인상담;
    public string GroupName { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public CounselingMethod Method { get; set; } = CounselingMethod.대면;
    public DateTime Date { get; set; } = DateTime.Today;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime? NextDate { get; set; }
    public string Content { get; set; } = string.Empty;
    public string FollowUp { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public void RecalculateDuration()
    {
        var diff = EndTime - StartTime;
        DurationMinutes = (int)Math.Round(diff.TotalMinutes);
    }
}
