using System.Collections.Generic;

namespace StudentCounseling.Models;

public class DataStore
{
    public List<Student> Students { get; set; } = new();
    public List<CounselingGroup> Groups { get; set; } = new();
    public List<Counseling> Counselings { get; set; } = new();
}
