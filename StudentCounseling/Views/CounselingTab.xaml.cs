using System.Windows;
using System.Windows.Controls;

namespace StudentCounseling.Views;

public partial class CounselingTab : UserControl
{
    public CounselingTab() { InitializeComponent(); }

    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button b && b.ContextMenu is not null)
        {
            b.ContextMenu.PlacementTarget = b;
            b.ContextMenu.IsOpen = true;
        }
    }
}
