using System;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentCounseling.Models;

namespace StudentCounseling.ViewModels;

public partial class StudentEditViewModel : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private int grade = 1;
    [ObservableProperty] private int classNumber = 1;
    [ObservableProperty] private int number = 1;
    [ObservableProperty] private string memo = string.Empty;
    [ObservableProperty] private string title = "학생 추가";

    public Guid? Id { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;

    public static StudentEditViewModel FromStudent(Student? s)
    {
        var vm = new StudentEditViewModel();
        if (s is null) return vm;
        vm.Id = s.Id;
        vm.Name = s.Name;
        vm.Grade = s.Grade;
        vm.ClassNumber = s.ClassNumber;
        vm.Number = s.Number;
        vm.Memo = s.Memo;
        vm.CreatedAt = s.CreatedAt;
        vm.Title = "학생 편집";
        return vm;
    }

    public Student ToStudent() => new()
    {
        Id = Id ?? Guid.NewGuid(),
        Name = Name.Trim(),
        Grade = Grade,
        ClassNumber = ClassNumber,
        Number = Number,
        Memo = Memo ?? string.Empty,
        CreatedAt = CreatedAt,
    };

    public bool IsValid() => !string.IsNullOrWhiteSpace(Name);
}
