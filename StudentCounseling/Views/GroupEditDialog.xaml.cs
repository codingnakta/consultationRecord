using System.Windows;
using StudentCounseling.Models;
using StudentCounseling.ViewModels;

namespace StudentCounseling.Views;

public partial class GroupEditDialog : Window
{
    public CounselingGroup? Result { get; private set; }

    public GroupEditDialog() { InitializeComponent(); }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not GroupEditViewModel vm) return;
        if (!vm.TryBuild(out var group, out var error))
        {
            MessageBox.Show(this, error, "확인", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Result = group;
        DialogResult = true;
    }
}
