using System.Windows;
using StudentCounseling.ViewModels;

namespace StudentCounseling.Views;

public partial class StudentEditDialog : Window
{
    public StudentEditDialog() { InitializeComponent(); }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is StudentEditViewModel vm && !vm.IsValid())
        {
            MessageBox.Show(this, "이름을 입력해주세요.", "확인", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        DialogResult = true;
    }
}
