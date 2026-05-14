using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentCounseling.Models;

namespace StudentCounseling.ViewModels;

public enum PeriodPreset { 이번학기, 올해, 작년, 전체, 사용자지정 }

public class CategoryStat
{
    public CounselingType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Count { get; init; }
    public int MaxInType { get; init; }
    public double Percent => MaxInType == 0 ? 0 : (double)Count / MaxInType * 100.0;
}

public class TopStudentStat
{
    public string ClassLabel { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Count { get; init; }
}

public class MethodStat
{
    public CounselingMethod Method { get; init; }
    public int Count { get; init; }
    public int MaxValue { get; init; }
    public double Percent => MaxValue == 0 ? 0 : (double)Count / MaxValue * 100.0;
}

public class TypeSection
{
    public string TypeName { get; init; } = string.Empty;
    public List<CategoryStat> Items { get; init; } = new();
}

public partial class StatsTabViewModel : ObservableObject
{
    public MainViewModel Main { get; }

    public ObservableCollection<PeriodPreset> Presets { get; } = new(Enum.GetValues<PeriodPreset>());

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomRange))]
    private PeriodPreset selectedPreset = PeriodPreset.이번학기;

    [ObservableProperty] private DateTime customStart = DateTime.Today.AddMonths(-3);
    [ObservableProperty] private DateTime customEnd = DateTime.Today;

    public bool IsCustomRange => SelectedPreset == PeriodPreset.사용자지정;

    [ObservableProperty] private int totalCount;
    [ObservableProperty] private int individualCount;
    [ObservableProperty] private int groupCount;
    [ObservableProperty] private int parentCount;
    [ObservableProperty] private int testCount;
    [ObservableProperty] private int uniqueStudentCount;
    [ObservableProperty] private int groupSessionCount;
    [ObservableProperty] private int upcomingCount;

    public ObservableCollection<TypeSection> CategorySections { get; } = new();
    public ObservableCollection<TopStudentStat> TopStudents { get; } = new();
    public ObservableCollection<MethodStat> MethodStats { get; } = new();

    public StatsTabViewModel(MainViewModel main)
    {
        Main = main;
        ((INotifyCollectionChanged)Main.Counselings).CollectionChanged += (_, _) => Recompute();
        ((INotifyCollectionChanged)Main.Students).CollectionChanged += (_, _) => Recompute();
        Recompute();
    }

    partial void OnSelectedPresetChanged(PeriodPreset value) => Recompute();
    partial void OnCustomStartChanged(DateTime value) { if (IsCustomRange) Recompute(); }
    partial void OnCustomEndChanged(DateTime value) { if (IsCustomRange) Recompute(); }

    public (DateTime Start, DateTime End) GetRange()
    {
        var today = DateTime.Today;
        return SelectedPreset switch
        {
            PeriodPreset.이번학기 => GetSemester(today),
            PeriodPreset.올해 => (new DateTime(today.Year, 1, 1), new DateTime(today.Year, 12, 31)),
            PeriodPreset.작년 => (new DateTime(today.Year - 1, 1, 1), new DateTime(today.Year - 1, 12, 31)),
            PeriodPreset.전체 => (DateTime.MinValue, DateTime.MaxValue),
            PeriodPreset.사용자지정 => (CustomStart.Date, CustomEnd.Date),
            _ => (DateTime.MinValue, DateTime.MaxValue),
        };
    }

    private static (DateTime, DateTime) GetSemester(DateTime today)
    {
        // 1학기: 3/1 - 8/31, 2학기: 9/1 - 다음해 2/28
        if (today.Month >= 3 && today.Month <= 8)
            return (new DateTime(today.Year, 3, 1), new DateTime(today.Year, 8, 31));
        if (today.Month >= 9)
            return (new DateTime(today.Year, 9, 1), new DateTime(today.Year + 1, 2, 28));
        return (new DateTime(today.Year - 1, 9, 1), new DateTime(today.Year, 2, 28));
    }

    public IEnumerable<Counseling> Filtered()
    {
        var (start, end) = GetRange();
        return Main.Counselings.Where(c => c.StudentIds.Count > 0 && c.Date >= start && c.Date <= end);
    }

    public void Recompute()
    {
        var data = Filtered().ToList();
        TotalCount = data.Count;
        IndividualCount = data.Count(c => c.Type == CounselingType.개인상담);
        GroupCount = data.Count(c => c.Type == CounselingType.집단상담);
        ParentCount = data.Count(c => c.Type == CounselingType.학부모상담);
        TestCount = data.Count(c => c.Type == CounselingType.심리검사);

        UniqueStudentCount = data.SelectMany(c => c.StudentIds).Distinct().Count();
        GroupSessionCount = data.Count(c => c.Type == CounselingType.집단상담);

        var today = DateTime.Today;
        UpcomingCount = Main.Counselings.Count(c => c.NextDate.HasValue && c.NextDate.Value.Date >= today);

        // CategorySections
        CategorySections.Clear();
        foreach (var t in Enum.GetValues<CounselingType>())
        {
            var byCat = data.Where(c => c.Type == t)
                            .GroupBy(c => c.SubCategory)
                            .Select(g => new { Name = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Count)
                            .ToList();
            if (byCat.Count == 0) continue;
            var max = byCat.Max(x => x.Count);
            var section = new TypeSection
            {
                TypeName = t.ToString(),
                Items = byCat.Select(x => new CategoryStat
                {
                    Type = t,
                    Name = x.Name,
                    Count = x.Count,
                    MaxInType = max,
                }).ToList(),
            };
            CategorySections.Add(section);
        }

        // Top students
        TopStudents.Clear();
        var byStudent = data
            .SelectMany(c => c.StudentIds.Select(sid => new { sid, c }))
            .GroupBy(x => x.sid)
            .Select(g => new { sid = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);
        foreach (var x in byStudent)
        {
            var s = Main.FindStudent(x.sid);
            if (s is null) continue;
            TopStudents.Add(new TopStudentStat
            {
                ClassLabel = $"{s.Grade}-{s.ClassNumber}",
                Name = s.Name,
                Count = x.Count,
            });
        }

        // Method distribution
        MethodStats.Clear();
        var byMethod = data.GroupBy(c => c.Method)
                           .Select(g => new { Method = g.Key, Count = g.Count() })
                           .ToDictionary(x => x.Method, x => x.Count);
        var allMethods = Enum.GetValues<CounselingMethod>();
        var maxM = allMethods.Max(m => byMethod.TryGetValue(m, out var v) ? v : 0);
        if (maxM == 0) maxM = 1;
        foreach (var m in allMethods)
        {
            MethodStats.Add(new MethodStat
            {
                Method = m,
                Count = byMethod.TryGetValue(m, out var v) ? v : 0,
                MaxValue = maxM,
            });
        }
    }

    [RelayCommand]
    private void ExportCounselings()
    {
        var path = Main.Dialog.PickSaveFile("Excel 파일 (*.xlsx)|*.xlsx", $"상담기록_{DateTime.Now:yyyyMMdd}.xlsx");
        if (path is null) return;
        try
        {
            Main.Excel.ExportCounselings(path, Filtered(), Main.StudentLookup());
            Main.Dialog.Info($"상담 기록을 저장했습니다.\n{Path.GetFileName(path)}", "내보내기 완료");
        }
        catch (Exception ex)
        {
            Main.Dialog.Error($"저장 실패: {ex.Message}", "오류");
        }
    }
}
