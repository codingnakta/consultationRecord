using System.Windows;
using StudentCounseling.Services;
using StudentCounseling.ViewModels;
using StudentCounseling.Views;

namespace StudentCounseling;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnLastWindowClose;

        var repo = new JsonDataRepository();
        var dialog = new DialogService();
        var excel = new ExcelService();
        var vm = new MainViewModel(repo, dialog, excel);

        var win = new MainWindow { DataContext = vm };
        MainWindow = win;
        win.Show();

        if (!string.IsNullOrEmpty(vm.LoadError))
            MessageBox.Show(win, vm.LoadError, "로드 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
