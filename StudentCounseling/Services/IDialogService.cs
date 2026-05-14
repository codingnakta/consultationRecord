using System;
using System.Collections.Generic;
using StudentCounseling.Models;

namespace StudentCounseling.Services;

public enum DeleteStudentChoice { Cancel, StudentOnly, StudentAndCounselings }
public enum ImportMode { Cancel, Replace, Append }

public interface IDialogService
{
    Student? EditStudent(Student? existing);
    CounselingGroup? EditGroup(CounselingGroup? existing, IReadOnlyList<Student> allStudents);
    Counseling? EditCounseling(
        Counseling? existing,
        IReadOnlyList<Student> allStudents,
        CounselingType? defaultType = null,
        string defaultGroupName = "",
        Guid? defaultGroupId = null,
        IReadOnlyCollection<Guid>? defaultStudentIds = null);
    bool Confirm(string message, string title);
    void Info(string message, string title);
    void Error(string message, string title);
    DeleteStudentChoice ConfirmDeleteStudent(int counselingCount);
    ImportMode AskImportMode();
    string? PickOpenFile(string filter);
    string? PickSaveFile(string filter, string defaultName);
}
