using System;

namespace StudentCounseling.Models;

public class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Grade { get; set; }
    public int ClassNumber { get; set; }
    public int Number { get; set; }
    public string Memo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
