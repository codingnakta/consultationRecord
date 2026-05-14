using System.Linq;
using StudentCounseling.Models;

namespace StudentCounseling.ViewModels;

public class CounselingItemViewModel
{
    public Counseling Counseling { get; }
    public string ParticipantNames { get; }
    public bool IsGroup { get; }
    public bool HasNextDate => Counseling.NextDate.HasValue;
    public string GroupTitle => string.IsNullOrWhiteSpace(Counseling.GroupName) ? "이름 없는 집단" : Counseling.GroupName;

    public CounselingItemViewModel(Counseling c, MainViewModel main)
    {
        Counseling = c;
        IsGroup = c.Type == CounselingType.집단상담;
        ParticipantNames = string.Join(", ",
            c.StudentIds.Select(id => main.FindStudent(id)?.Name).Where(n => !string.IsNullOrEmpty(n)));
    }

    public string TimeLabel =>
        $"{Counseling.StartTime:hh\\:mm}~{Counseling.EndTime:hh\\:mm} · {Counseling.DurationMinutes}분";

    public string ContentFirstLine
    {
        get
        {
            var s = Counseling.Content ?? string.Empty;
            var idx = s.IndexOfAny(new[] { '\r', '\n' });
            if (idx >= 0) s = s.Substring(0, idx);
            return s;
        }
    }
}
