using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentCounseling.Models;
using StudentCounseling.Services;

namespace StudentCounseling.ViewModels;

public partial class CounselingTabViewModel : ObservableObject
{
    public MainViewModel Main { get; }
    public ICollectionView StudentsView { get; }

    public ObservableCollection<CounselingItemViewModel> CounselingItems { get; } = new();
    public ObservableCollection<GroupCounselingGroupViewModel> GroupItems { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStudentTab))]
    [NotifyPropertyChangedFor(nameof(IsGroupTab))]
    private int selectedListTabIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedStudent))]
    [NotifyPropertyChangedFor(nameof(SelectedStudentClassLabel))]
    [NotifyPropertyChangedFor(nameof(ActiveSelection))]
    [NotifyCanExecuteChangedFor(nameof(EditStudentCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteStudentCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddCounselingCommand))]
    private Student? selectedStudent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedCounseling))]
    [NotifyCanExecuteChangedFor(nameof(EditCounselingCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCounselingCommand))]
    private CounselingItemViewModel? selectedCounselingItem;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedGroup))]
    [NotifyPropertyChangedFor(nameof(ActiveSelection))]
    [NotifyCanExecuteChangedFor(nameof(EditGroupCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteGroupCommand))]
    private GroupCounselingGroupViewModel? selectedGroup;

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string searchGrade = string.Empty;
    [ObservableProperty] private string searchClassNumber = string.Empty;
    [ObservableProperty] private string searchNumber = string.Empty;

    public bool IsStudentTab => SelectedListTabIndex == 0;
    public bool IsGroupTab => SelectedListTabIndex == 1;
    public bool HasSelectedStudent => SelectedStudent is not null;
    public bool HasSelectedGroup => SelectedGroup is not null;
    public bool HasSelectedCounseling => SelectedCounselingItem is not null;
    public object? ActiveSelection => SelectedStudent ?? (object?)SelectedGroup;
    public int StudentCount => StudentsView.Cast<object>().Count();
    public int CounselingCount => CounselingItems.Count;
    public int GroupCount => GroupItems.Count;

    public string SelectedStudentClassLabel =>
        SelectedStudent is null ? string.Empty
        : $"{SelectedStudent.Grade}학년 {SelectedStudent.ClassNumber}반 {SelectedStudent.Number}번";

    public CounselingTabViewModel(MainViewModel main)
    {
        Main = main;
        StudentsView = CollectionViewSource.GetDefaultView(Main.Students);
        StudentsView.Filter = FilterStudent;
        ((INotifyCollectionChanged)Main.Students).CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(StudentCount));
            ReloadGroupCounselings();
        };
        ((INotifyCollectionChanged)Main.Groups).CollectionChanged += (_, _) => ReloadGroupCounselings();
        ((INotifyCollectionChanged)Main.Counselings).CollectionChanged += (_, _) =>
        {
            ReloadCounselings();
            ReloadGroupCounselings();
        };
        ReloadGroupCounselings();
    }

    private bool FilterStudent(object obj)
    {
        if (obj is not Student s) return false;
        if (!string.IsNullOrWhiteSpace(SearchGrade) &&
            (!int.TryParse(SearchGrade.Trim(), out var grade) || s.Grade != grade))
            return false;

        if (!string.IsNullOrWhiteSpace(SearchClassNumber) &&
            (!int.TryParse(SearchClassNumber.Trim(), out var classNumber) || s.ClassNumber != classNumber))
            return false;

        if (!string.IsNullOrWhiteSpace(SearchNumber) &&
            (!int.TryParse(SearchNumber.Trim(), out var number) || s.Number != number))
            return false;

        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var keyword = SearchText.Trim();
        return s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    partial void OnSearchTextChanged(string value)
    {
        StudentsView.Refresh();
        ReloadGroupCounselings();
        OnPropertyChanged(nameof(StudentCount));
    }

    partial void OnSearchGradeChanged(string value) => RefreshStudentSearch();
    partial void OnSearchClassNumberChanged(string value) => RefreshStudentSearch();
    partial void OnSearchNumberChanged(string value) => RefreshStudentSearch();

    private void RefreshStudentSearch()
    {
        StudentsView.Refresh();
        OnPropertyChanged(nameof(StudentCount));
    }

    partial void OnSelectedListTabIndexChanged(int value)
    {
        SelectedCounselingItem = null;
        if (IsGroupTab)
            SelectedStudent = null;
        else
            SelectedGroup = null;
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        SelectedCounselingItem = null;
        ReloadCounselings();
    }

    private void ReloadCounselings()
    {
        CounselingItems.Clear();
        if (SelectedStudent is null && SelectedGroup is null) { OnPropertyChanged(nameof(CounselingCount)); return; }

        var source = SelectedGroup is not null
            ? Main.CounselingsOfGroup(SelectedGroup.Group.Id)
            : Main.CounselingsOf(SelectedStudent!.Id);

        foreach (var c in source)
            CounselingItems.Add(new CounselingItemViewModel(c, Main));
        OnPropertyChanged(nameof(CounselingCount));
    }

    private void ReloadGroupCounselings()
    {
        var selectedGroupId = SelectedGroup?.Group.Id;
        GroupItems.Clear();
        var query = Main.Groups.Select(g =>
        {
            var counselings = Main.CounselingsOfGroup(g.Id).ToList();
            var latest = counselings.FirstOrDefault();
            var names = g.StudentIds
                         .Select(id => Main.FindStudent(id)?.Name)
                         .Where(n => !string.IsNullOrEmpty(n));
            return new GroupCounselingGroupViewModel
            {
                Group = g,
                ParticipantNames = string.Join(", ", names),
                SessionCount = counselings.Count,
                LatestDate = latest?.Date,
            };
        });

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(i =>
                i.ParticipantNames.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                i.GroupName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var item in query.OrderByDescending(i => i.LatestDate ?? DateTime.MinValue).ThenBy(i => i.GroupName))
            GroupItems.Add(item);

        SelectedGroup = selectedGroupId.HasValue
            ? GroupItems.FirstOrDefault(i => i.Group.Id == selectedGroupId.Value)
            : null;

        OnPropertyChanged(nameof(GroupCount));
        ReloadCounselings();
    }

    partial void OnSelectedGroupChanged(GroupCounselingGroupViewModel? value)
    {
        SelectedStudent = null;
        SelectedCounselingItem = null;
        ReloadCounselings();
    }

    private static string GetGroupName(Counseling c)
        => string.IsNullOrWhiteSpace(c.GroupName) ? "이름 없는 집단" : c.GroupName.Trim();

    [RelayCommand]
    private void AddGroup()
    {
        var group = Main.Dialog.EditGroup(null, Main.Students);
        if (group is null) return;
        Main.Groups.Add(group);
        Main.PersistAll();
        SelectedListTabIndex = 1;
        SelectedGroup = GroupItems.FirstOrDefault(i => i.Group.Id == group.Id);
    }

    [RelayCommand(CanExecute = nameof(HasSelectedGroup))]
    private void EditGroup()
    {
        if (SelectedGroup is null) return;
        var existing = SelectedGroup.Group;
        var updated = Main.Dialog.EditGroup(existing, Main.Students);
        if (updated is null) return;
        existing.Name = updated.Name;
        existing.StudentIds = updated.StudentIds;
        foreach (var c in Main.Counselings.Where(c => c.GroupId == existing.Id))
        {
            c.GroupName = existing.Name;
            c.StudentIds = existing.StudentIds.ToList();
        }
        Main.PersistAll();
        ReloadGroupCounselings();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedGroup))]
    private void DeleteGroup()
    {
        if (SelectedGroup is null) return;
        var group = SelectedGroup.Group;
        var count = Main.Counselings.Count(c => c.GroupId == group.Id);
        if (!Main.Dialog.Confirm($"'{group.Name}' 집단을 삭제할까요?\n연결된 상담 {count}건은 유지되지만 집단 목록에서는 사라집니다.", "집단 삭제")) return;
        foreach (var c in Main.Counselings.Where(c => c.GroupId == group.Id))
            c.GroupId = null;
        SelectedGroup = null;
        Main.Groups.Remove(group);
        Main.PersistAll();
    }

    [RelayCommand]
    private void AddStudent()
    {
        var s = Main.Dialog.EditStudent(null);
        if (s is null) return;
        Main.Students.Add(s);
        Main.ResortStudents();
        Main.PersistAll();
        SelectedStudent = s;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedStudent))]
    private void EditStudent()
    {
        if (SelectedStudent is null) return;
        var updated = Main.Dialog.EditStudent(SelectedStudent);
        if (updated is null) return;
        SelectedStudent.Name = updated.Name;
        SelectedStudent.Grade = updated.Grade;
        SelectedStudent.ClassNumber = updated.ClassNumber;
        SelectedStudent.Number = updated.Number;
        SelectedStudent.Memo = updated.Memo;
        Main.ResortStudents();
        Main.PersistAll();
        OnPropertyChanged(nameof(SelectedStudent));
        OnPropertyChanged(nameof(SelectedStudentClassLabel));
    }

    [RelayCommand(CanExecute = nameof(HasSelectedStudent))]
    private void DeleteStudent()
    {
        if (SelectedStudent is null) return;
        var count = Main.Counselings.Count(c => c.StudentIds.Contains(SelectedStudent.Id));
        var choice = Main.Dialog.ConfirmDeleteStudent(count);
        if (choice == DeleteStudentChoice.Cancel) return;

        var sid = SelectedStudent.Id;
        var toRemove = SelectedStudent;
        SelectedStudent = null;

        if (choice == DeleteStudentChoice.StudentAndCounselings)
        {
            var dead = Main.Counselings.Where(c => c.StudentIds.Contains(sid)).ToList();
            foreach (var c in dead) Main.Counselings.Remove(c);
        }
        else
        {
            int orphaned = 0;
            foreach (var c in Main.Counselings.ToList())
            {
                if (c.StudentIds.Remove(sid))
                {
                    if (c.StudentIds.Count == 0)
                    {
                        Main.Counselings.Remove(c);
                        orphaned++;
                    }
                }
            }
            if (orphaned > 0)
                Main.Dialog.Info($"참여 학생이 없어진 상담 {orphaned}건을 함께 정리했습니다.", "안내");
        }

        Main.Students.Remove(toRemove);
        Main.PersistAll();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedStudent))]
    private void AddCounseling()
    {
        if (SelectedStudent is null) return;
        // Pre-select current student by passing all students; user can adjust
        var c = Main.Dialog.EditCounseling(null, Main.Students);
        if (c is null) return;

        // If user didn't pick anyone but current student was selected, default-include it
        if (c.StudentIds.Count == 0) c.StudentIds.Add(SelectedStudent.Id);

        Main.Counselings.Add(c);
        Main.PersistAll();
        SelectedCounselingItem = CounselingItems.FirstOrDefault(i => i.Counseling.Id == c.Id);
    }

    [RelayCommand]
    private void AddGroupCounseling()
    {
        if (SelectedGroup is null)
        {
            AddGroup();
            return;
        }

        var group = SelectedGroup.Group;
        var c = Main.Dialog.EditCounseling(
            null,
            Main.Students,
            CounselingType.집단상담,
            group.Name,
            group.Id,
            group.StudentIds);
        if (c is null) return;
        Main.Counselings.Add(c);
        Main.PersistAll();
        SelectedGroup = GroupItems.FirstOrDefault(i => i.Group.Id == group.Id);
        SelectedCounselingItem = CounselingItems.FirstOrDefault(i => i.Counseling.Id == c.Id);
    }

    [RelayCommand(CanExecute = nameof(HasSelectedCounseling))]
    private void EditCounseling()
    {
        if (SelectedCounselingItem is null) return;
        var existing = SelectedCounselingItem.Counseling;
        var updated = Main.Dialog.EditCounseling(existing, Main.Students);
        if (updated is null) return;
        existing.StudentIds = updated.StudentIds;
        existing.GroupId = updated.GroupId;
        existing.Type = updated.Type;
        existing.GroupName = updated.GroupName;
        existing.SubCategory = updated.SubCategory;
        existing.Method = updated.Method;
        existing.Date = updated.Date;
        existing.StartTime = updated.StartTime;
        existing.EndTime = updated.EndTime;
        existing.DurationMinutes = updated.DurationMinutes;
        existing.NextDate = updated.NextDate;
        existing.Content = updated.Content;
        existing.FollowUp = updated.FollowUp;
        Main.PersistAll();
        ReloadCounselings();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedCounseling))]
    private void DeleteCounseling()
    {
        if (SelectedCounselingItem is null) return;
        if (!Main.Dialog.Confirm("이 상담 기록을 삭제할까요?", "상담 삭제")) return;
        var target = SelectedCounselingItem.Counseling;
        SelectedCounselingItem = null;
        Main.Counselings.Remove(target);
        Main.PersistAll();
    }

    [RelayCommand]
    private void ImportStudents()
    {
        var path = Main.Dialog.PickOpenFile("Excel 파일 (*.xlsx)|*.xlsx");
        if (path is null) return;

        var mode = Main.Dialog.AskImportMode();
        if (mode == ImportMode.Cancel) return;

        ExcelImportResult res;
        try { res = Main.Excel.ImportStudents(path); }
        catch (Exception ex) { Main.Dialog.Error($"엑셀 파일을 읽지 못했습니다.\n{ex.Message}", "오류"); return; }

        if (res.Students.Count == 0)
        {
            var msg = res.RowErrors.Count > 0
                ? "가져올 학생이 없습니다.\n\n" + string.Join("\n", res.RowErrors)
                : "가져올 학생이 없습니다. 시트가 비어있거나 헤더가 다를 수 있습니다.";
            Main.Dialog.Error(msg, "가져오기 실패");
            return;
        }

        if (mode == ImportMode.Replace)
        {
            // Match existing students by name+grade+class+number to preserve counseling links
            var oldKey = Main.Students.ToDictionary(s => StudentKey(s), s => s.Id);
            var newStudents = res.Students.Select(s => s).ToList();
            int matched = 0, unmatched = 0;
            var idRemap = new System.Collections.Generic.Dictionary<Guid, Guid>();

            foreach (var s in newStudents)
            {
                var key = StudentKey(s);
                if (oldKey.TryGetValue(key, out var existingId))
                {
                    idRemap[existingId] = s.Id;
                    matched++;
                }
            }
            // For existing students missing from new set, their counseling participations will be removed
            foreach (var old in Main.Students.ToList())
            {
                if (!idRemap.ContainsKey(old.Id)) unmatched++;
            }

            // Apply remap on counselings
            foreach (var c in Main.Counselings.ToList())
            {
                var rebuilt = new System.Collections.Generic.List<Guid>();
                foreach (var sid in c.StudentIds)
                {
                    if (idRemap.TryGetValue(sid, out var newId)) rebuilt.Add(newId);
                }
                if (rebuilt.Count == 0)
                {
                    Main.Counselings.Remove(c);
                }
                else
                {
                    c.StudentIds = rebuilt;
                }
            }

            Main.Students.Clear();
            foreach (var s in newStudents) Main.Students.Add(s);
            Main.ResortStudents();
            Main.PersistAll();

            var rep = $"전체 교체 완료.\n신규: {newStudents.Count}명\n매칭: {matched}명 / 매칭 실패: {unmatched}명";
            if (res.RowErrors.Count > 0) rep += "\n\n행 오류:\n" + string.Join("\n", res.RowErrors);
            Main.Dialog.Info(rep, "가져오기 결과");
        }
        else // Append
        {
            var existingKeys = Main.Students.Select(StudentKey).ToHashSet();
            int added = 0, skipped = 0;
            foreach (var s in res.Students)
            {
                if (existingKeys.Contains(StudentKey(s))) { skipped++; continue; }
                Main.Students.Add(s);
                added++;
            }
            Main.ResortStudents();
            Main.PersistAll();
            var rep = $"추가 완료.\n신규: {added}명 / 중복 스킵: {skipped}명";
            if (res.RowErrors.Count > 0) rep += "\n\n행 오류:\n" + string.Join("\n", res.RowErrors);
            Main.Dialog.Info(rep, "가져오기 결과");
        }
    }

    private static string StudentKey(Student s) => $"{s.Name}|{s.Grade}|{s.ClassNumber}|{s.Number}";

    [RelayCommand]
    private void ExportStudents()
    {
        var path = Main.Dialog.PickSaveFile("Excel 파일 (*.xlsx)|*.xlsx", $"학생명단_{DateTime.Now:yyyyMMdd}.xlsx");
        if (path is null) return;
        try
        {
            Main.Excel.ExportStudents(path, Main.Students);
            Main.Dialog.Info($"학생 명단을 저장했습니다.\n{Path.GetFileName(path)}", "내보내기 완료");
        }
        catch (Exception ex)
        {
            Main.Dialog.Error($"저장 실패: {ex.Message}", "오류");
        }
    }
}

public class GroupCounselingGroupViewModel
{
    public CounselingGroup Group { get; init; } = new();
    public string GroupName => Group.Name;
    public string ParticipantNames { get; init; } = string.Empty;
    public int SessionCount { get; init; }
    public DateTime? LatestDate { get; init; }
    public string LatestDateLabel => LatestDate.HasValue ? LatestDate.Value.ToString("yyyy-MM-dd") : "상담 없음";
}
