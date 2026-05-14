using System.Windows;
using StudentCounseling.Models;
using StudentCounseling.ViewModels;

namespace StudentCounseling.Views;

public partial class CounselingEditDialog : Window
{
    public Counseling? Result { get; private set; }

    public CounselingEditDialog() { InitializeComponent(); }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CounselingEditViewModel vm) return;
        if (!vm.TryBuild(out var c, out var error))
        {
            MessageBox.Show(this, error, "확인", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        Result = c;
        DialogResult = true;
    }
}
