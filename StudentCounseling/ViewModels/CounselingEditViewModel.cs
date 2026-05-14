using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentCounseling.Models;

namespace StudentCounseling.ViewModels;

public partial class CounselingEditViewModel : ObservableObject
{
    public ObservableCollection<CounselingType> Types { get; } = new(Enum.GetValues<CounselingType>());
    public ObservableCollection<CounselingMethod> Methods { get; } = new(Enum.GetValues<CounselingMethod>());
    public ObservableCollection<string> SubCategoriesView { get; } = new();
    public ObservableCollection<StudentCheckItem> StudentItems { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGroup))]
    [NotifyPropertyChangedFor(nameof(SingleSelectionHint))]
    private CounselingType type = CounselingType.개인상담;

    [ObservableProperty] private CounselingMethod method = CounselingMethod.대면;
    [ObservableProperty] private string groupName = string.Empty;
    [ObservableProperty] private string subCategory = string.Empty;
    [ObservableProperty] private DateTime date = DateTime.Today;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationLabel))]
    private string startTimeText = "14:00";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationLabel))]
    private string endTimeText = "14:30";
    [ObservableProperty] private DateTime? nextDate;
    [ObservableProperty] private string content = string.Empty;
    [ObservableProperty] private string followUp = string.Empty;
    [ObservableProperty] private string windowTitle = "새 상담 기록";

    public bool IsGroup => Type == CounselingType.집단상담;
    public string SingleSelectionHint => IsGroup ? "참여 학생 (2명 이상)" : "참여 학생 (1명만)";

    public string DurationLabel
    {
        get
        {
            if (TimeSpan.TryParse(StartTimeText, out var s) && TimeSpan.TryParse(EndTimeText, out var e))
            {
                var min = (int)Math.Round((e - s).TotalMinutes);
                return min >= 0 ? $"{min}분" : "—";
            }
            return "—";
        }
    }

    public Guid? Id { get; private set; }
    public Guid? GroupId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;

    private CounselingEditViewModel() { }

    public static CounselingEditViewModel ForNew(
        IReadOnlyList<Student> allStudents,
        CounselingType? defaultType = null,
        string defaultGroupName = "",
        Guid? defaultGroupId = null,
        IReadOnlyCollection<Guid>? defaultStudentIds = null)
    {
        var vm = new CounselingEditViewModel();
        if (defaultType.HasValue) vm.Type = defaultType.Value;
        vm.GroupName = defaultGroupName;
        vm.GroupId = defaultGroupId;
        vm.LoadStudents(allStudents, defaultStudentIds is null ? new HashSet<Guid>() : new HashSet<Guid>(defaultStudentIds));
        vm.RefreshSubCategories(preserveSelection: false);
        vm.WindowTitle = "새 상담 기록";
        return vm;
    }

    public static CounselingEditViewModel FromCounseling(Counseling c, IReadOnlyList<Student> allStudents)
    {
        var vm = new CounselingEditViewModel
        {
            Id = c.Id,
            GroupId = c.GroupId,
            Type = c.Type,
            GroupName = c.GroupName,
            Method = c.Method,
            Date = c.Date.Date,
            StartTimeText = c.StartTime.ToString("hh\\:mm"),
            EndTimeText = c.EndTime.ToString("hh\\:mm"),
            NextDate = c.NextDate,
            Content = c.Content,
            FollowUp = c.FollowUp,
            CreatedAt = c.CreatedAt,
            WindowTitle = "상담 편집",
        };
        vm.LoadStudents(allStudents, new HashSet<Guid>(c.StudentIds));
        vm.RefreshSubCategories(preserveSelection: false);
        vm.SubCategory = c.SubCategory;
        return vm;
    }

    private void LoadStudents(IReadOnlyList<Student> all, HashSet<Guid> selected)
    {
        StudentItems.Clear();
        foreach (var s in all.OrderBy(s => s.Grade).ThenBy(s => s.ClassNumber).ThenBy(s => s.Number))
        {
            var item = new StudentCheckItem(s, selected.Contains(s.Id));
            item.PropertyChanged += Item_PropertyChanged;
            StudentItems.Add(item);
        }
    }

    private bool _suppressEnforce;
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(StudentCheckItem.IsChecked)) return;
        if (_suppressEnforce) return;
        if (IsGroup) return;

        // Single-selection enforcement
        if (sender is StudentCheckItem changed && changed.IsChecked)
        {
            _suppressEnforce = true;
            foreach (var it in StudentItems)
                if (!ReferenceEquals(it, changed) && it.IsChecked) it.IsChecked = false;
            _suppressEnforce = false;
        }
    }

    partial void OnTypeChanged(CounselingType value)
    {
        RefreshSubCategories(preserveSelection: true);

        if (!IsGroup)
        {
            var first = StudentItems.FirstOrDefault(i => i.IsChecked);
            _suppressEnforce = true;
            foreach (var it in StudentItems)
                it.IsChecked = ReferenceEquals(it, first) && first is not null;
            _suppressEnforce = false;
        }
    }

    private void RefreshSubCategories(bool preserveSelection)
    {
        var prev = SubCategory;
        SubCategoriesView.Clear();
        if (CounselingCategories.SubCategories.TryGetValue(Type, out var arr))
            foreach (var s in arr) SubCategoriesView.Add(s);

        if (preserveSelection && SubCategoriesView.Contains(prev))
            SubCategory = prev;
        else
            SubCategory = SubCategoriesView.FirstOrDefault() ?? string.Empty;
    }

    public bool TryBuild(out Counseling result, out string? error)
    {
        result = null!;

        var picked = StudentItems.Where(i => i.IsChecked).Select(i => i.Student.Id).ToList();
        if (IsGroup)
        {
            if (picked.Count < 2) { error = "집단상담은 학생 2명 이상을 선택해주세요."; return false; }
            if (string.IsNullOrWhiteSpace(GroupName)) { error = "집단명을 입력해주세요."; return false; }
        }
        else
        {
            if (picked.Count != 1) { error = "학생 1명을 선택해주세요."; return false; }
        }
        if (string.IsNullOrWhiteSpace(SubCategory)) { error = "세부 카테고리를 선택해주세요."; return false; }
        if (!TimeSpan.TryParse(StartTimeText, out var st)) { error = "시작 시각을 HH:mm 형식으로 입력해주세요."; return false; }
        if (!TimeSpan.TryParse(EndTimeText, out var et)) { error = "종료 시각을 HH:mm 형식으로 입력해주세요."; return false; }
        if (et < st) { error = "종료 시각이 시작 시각보다 빠릅니다."; return false; }

        result = new Counseling
        {
            Id = Id ?? Guid.NewGuid(),
            StudentIds = picked,
            GroupId = IsGroup ? GroupId : null,
            Type = Type,
            GroupName = IsGroup ? GroupName.Trim() : string.Empty,
            SubCategory = SubCategory,
            Method = Method,
            Date = Date.Date,
            StartTime = st,
            EndTime = et,
            NextDate = NextDate,
            Content = Content ?? string.Empty,
            FollowUp = FollowUp ?? string.Empty,
            CreatedAt = CreatedAt,
        };
        result.RecalculateDuration();
        error = null;
        return true;
    }
}
