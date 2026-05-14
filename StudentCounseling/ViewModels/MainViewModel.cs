using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentCounseling.Models;
using StudentCounseling.Services;

namespace StudentCounseling.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDataRepository _repo;
    public IDialogService Dialog { get; }
    public ExcelService Excel { get; }

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<CounselingGroup> Groups { get; } = new();
    public ObservableCollection<Counseling> Counselings { get; } = new();

    public CounselingTabViewModel CounselingTab { get; }
    public StatsTabViewModel StatsTab { get; }

    [ObservableProperty] private string? loadError;

    public MainViewModel(IDataRepository repo, IDialogService dialog, ExcelService excel)
    {
        _repo = repo;
        Dialog = dialog;
        Excel = excel;

        DataStore data;
        try { data = _repo.Load(); }
        catch (Exception ex)
        {
            LoadError = $"데이터를 불러오지 못했습니다. 빈 데이터로 시작합니다.\n\n{ex.Message}";
            data = new DataStore();
        }

        foreach (var s in data.Students.OrderBy(s => s.Grade).ThenBy(s => s.ClassNumber).ThenBy(s => s.Number))
            Students.Add(s);
        foreach (var g in data.Groups.OrderBy(g => g.Name))
            Groups.Add(g);
        foreach (var c in data.Counselings.OrderByDescending(c => c.Date).ThenByDescending(c => c.StartTime))
            Counselings.Add(c);

        CounselingTab = new CounselingTabViewModel(this);
        StatsTab = new StatsTabViewModel(this);
    }

    public void PersistAll()
    {
        var data = new DataStore
        {
            Students = Students.ToList(),
            Groups = Groups.ToList(),
            Counselings = Counselings.ToList(),
        };
        _repo.Save(data);
    }

    public void ResortStudents()
    {
        var sorted = Students.OrderBy(s => s.Grade).ThenBy(s => s.ClassNumber).ThenBy(s => s.Number).ToList();
        Students.Clear();
        foreach (var s in sorted) Students.Add(s);
    }

    public Student? FindStudent(Guid id) => Students.FirstOrDefault(s => s.Id == id);
    public CounselingGroup? FindGroup(Guid id) => Groups.FirstOrDefault(g => g.Id == id);

    public IEnumerable<Counseling> CounselingsOf(Guid studentId)
        => Counselings.Where(c => c.StudentIds.Contains(studentId))
                       .OrderByDescending(c => c.Date).ThenByDescending(c => c.StartTime);

    public IEnumerable<Counseling> CounselingsOfGroup(Guid groupId)
        => Counselings.Where(c => c.GroupId == groupId)
                       .OrderByDescending(c => c.Date).ThenByDescending(c => c.StartTime);

    public IReadOnlyDictionary<Guid, Student> StudentLookup()
        => Students.ToDictionary(s => s.Id);
}
