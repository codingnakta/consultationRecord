using CommunityToolkit.Mvvm.ComponentModel;
using StudentCounseling.Models;

namespace StudentCounseling.ViewModels;

public partial class StudentCheckItem : ObservableObject
{
    public Student Student { get; }

    [ObservableProperty] private bool isChecked;

    public string Label => $"{Student.Name}  ({Student.Grade}-{Student.ClassNumber}-{Student.Number})";

    public StudentCheckItem(Student s, bool initialChecked)
    {
        Student = s;
        IsChecked = initialChecked;
    }
}
