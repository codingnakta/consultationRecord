using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentCounseling.Models;

namespace StudentCounseling.ViewModels;

public partial class GroupEditViewModel : ObservableObject
{
    public ObservableCollection<StudentCheckItem> StudentItems { get; } = new();

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string title = "새 집단 만들기";

    public Guid? Id { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;

    private GroupEditViewModel() { }

    public static GroupEditViewModel ForNew(IReadOnlyList<Student> allStudents)
    {
        var vm = new GroupEditViewModel();
        vm.LoadStudents(allStudents, new HashSet<Guid>());
        return vm;
    }

    public static GroupEditViewModel FromGroup(CounselingGroup group, IReadOnlyList<Student> allStudents)
    {
        var vm = new GroupEditViewModel
        {
            Id = group.Id,
            Name = group.Name,
            CreatedAt = group.CreatedAt,
            Title = "집단 편집",
        };
        vm.LoadStudents(allStudents, new HashSet<Guid>(group.StudentIds));
        return vm;
    }

    private void LoadStudents(IReadOnlyList<Student> allStudents, HashSet<Guid> selected)
    {
        StudentItems.Clear();
        foreach (var s in allStudents.OrderBy(s => s.Grade).ThenBy(s => s.ClassNumber).ThenBy(s => s.Number))
            StudentItems.Add(new StudentCheckItem(s, selected.Contains(s.Id)));
    }

    public bool TryBuild(out CounselingGroup group, out string? error)
    {
        group = null!;
        if (string.IsNullOrWhiteSpace(Name))
        {
            error = "집단명을 입력해주세요.";
            return false;
        }

        var picked = StudentItems.Where(i => i.IsChecked).Select(i => i.Student.Id).ToList();
        if (picked.Count < 2)
        {
            error = "집단에는 학생 2명 이상을 선택해주세요.";
            return false;
        }

        group = new CounselingGroup
        {
            Id = Id ?? Guid.NewGuid(),
            Name = Name.Trim(),
            StudentIds = picked,
            CreatedAt = CreatedAt,
        };
        error = null;
        return true;
    }
}
