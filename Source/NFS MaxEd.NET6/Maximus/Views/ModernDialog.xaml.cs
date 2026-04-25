using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Maximus.Views;

public partial class ModernDialog : Window
{
    public enum DialogType
    {
        Info,
        Warning,
        Error,
        Confirm
    }

    public ModernDialog(string message, string title, DialogType type)
    {
        InitializeComponent();
        TxtMessage.Text = message;
        TxtTitle.Text = title;

        // Настройка стиля под тип
        switch (type)
        {
            case DialogType.Info: TypeAccent.Fill = Brushes.Cyan; break;
            case DialogType.Warning: TypeAccent.Fill = Brushes.Orange; break;
            case DialogType.Error: TypeAccent.Fill = Brushes.Crimson; break;
            case DialogType.Confirm:
                TypeAccent.Fill = Brushes.LimeGreen;
                BtnCancel.Visibility = Visibility.Visible;
                break;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public static bool? Show(string message, string title = "Maximus", DialogType type = DialogType.Info)
    {
        var dlg = new ModernDialog(message, title, type);
        return dlg.ShowDialog();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
        BeginAnimation(OpacityProperty, anim);
    }
}