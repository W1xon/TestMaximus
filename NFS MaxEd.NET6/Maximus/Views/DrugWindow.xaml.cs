using System.Windows;

namespace Maximus.Views;

public partial class DrugWindow : Window
{
    public DrugWindow()
    {
        InitializeComponent();
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}