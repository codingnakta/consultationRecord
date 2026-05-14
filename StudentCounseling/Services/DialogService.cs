using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using StudentCounseling.Models;
using StudentCounseling.ViewModels;
using StudentCounseling.Views;

namespace StudentCounseling.Services;

public class DialogService : IDialogService
{
    public Student? EditStudent(Student? existing)
    {
        var vm = StudentEditViewModel.FromStudent(existing);
        var dlg = new StudentEditDialog
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow,
        };
        return dlg.ShowDialog() == true ? vm.ToStudent() : null;
    }

    public CounselingGroup? EditGroup(CounselingGroup? existing, IReadOnlyList<Student> allStudents)
    {
        var vm = existing is null
            ? GroupEditViewModel.ForNew(allStudents)
            : GroupEditViewModel.FromGroup(existing, allStudents);
        var dlg = new GroupEditDialog
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow,
        };
        return dlg.ShowDialog() == true ? dlg.Result : null;
    }

    public Counseling? EditCounseling(
        Counseling? existing,
        IReadOnlyList<Student> allStudents,
        CounselingType? defaultType = null,
        string defaultGroupName = "",
        Guid? defaultGroupId = null,
        IReadOnlyCollection<Guid>? defaultStudentIds = null)
    {
        var vm = existing is null
            ? CounselingEditViewModel.ForNew(allStudents, defaultType, defaultGroupName, defaultGroupId, defaultStudentIds)
            : CounselingEditViewModel.FromCounseling(existing, allStudents);
        var dlg = new CounselingEditDialog
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow,
        };
        return dlg.ShowDialog() == true ? dlg.Result : null;
    }

    public bool Confirm(string message, string title)
        => MessageBox.Show(Application.Current.MainWindow, message, title,
            MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK;

    public void Info(string message, string title)
        => MessageBox.Show(Application.Current.MainWindow, message, title,
            MessageBoxButton.OK, MessageBoxImage.Information);

    public void Error(string message, string title)
        => MessageBox.Show(Application.Current.MainWindow, message, title,
            MessageBoxButton.OK, MessageBoxImage.Error);

    public DeleteStudentChoice ConfirmDeleteStudent(int counselingCount)
    {
        if (counselingCount == 0)
        {
            return Confirm("이 학생을 삭제할까요?", "학생 삭제")
                ? DeleteStudentChoice.StudentOnly
                : DeleteStudentChoice.Cancel;
        }
        var msg =
            $"이 학생이 참여한 상담 {counselingCount}건이 있습니다.\n\n" +
            "[예] 학생만 삭제 (상담은 유지)\n" +
            "[아니오] 학생 + 상담 모두 삭제\n" +
            "[취소]";
        var r = MessageBox.Show(Application.Current.MainWindow, msg, "학생 삭제",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        return r switch
        {
            MessageBoxResult.Yes => DeleteStudentChoice.StudentOnly,
            MessageBoxResult.No => DeleteStudentChoice.StudentAndCounselings,
            _ => DeleteStudentChoice.Cancel,
        };
    }

    public ImportMode AskImportMode()
    {
        var msg =
            "기존 학생 명단과 어떻게 처리할까요?\n\n" +
            "[예] 전체 교체 (기존 삭제 후 새로 추가, 상담은 이름+학년반번호로 매칭)\n" +
            "[아니오] 추가 (동일 항목 스킵)\n" +
            "[취소]";
        var r = MessageBox.Show(Application.Current.MainWindow, msg, "엑셀 가져오기",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        return r switch
        {
            MessageBoxResult.Yes => ImportMode.Replace,
            MessageBoxResult.No => ImportMode.Append,
            _ => ImportMode.Cancel,
        };
    }

    public string? PickOpenFile(string filter)
    {
        var dlg = new OpenFileDialog { Filter = filter };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? PickSaveFile(string filter, string defaultName)
    {
        var dlg = new SaveFileDialog { Filter = filter, FileName = defaultName };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }
}
