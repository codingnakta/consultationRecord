using System;
using System.Collections.Generic;

namespace StudentCounseling.Models;

public class CounselingGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<Guid> StudentIds { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
