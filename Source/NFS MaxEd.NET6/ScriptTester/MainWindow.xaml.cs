using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScriptTester;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Tester _tester;
    public MainWindow()
    {
        InitializeComponent();
        _tester = new Tester();
    }

    private void RunScriptButton_Click(object sender, RoutedEventArgs e)
    {
        string[] goldenScriptFileNames = _tester.OpenFile();
        string[] testScriptFileNames = _tester.OpenFile();
        StringBuilder sb = new();
        if (goldenScriptFileNames.Length == 0 || testScriptFileNames.Length == 0)
        {
            MessageBox.Show("Please select both a golden script and a test script.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        for (int i = 0; i < Math.Min(goldenScriptFileNames.Length, testScriptFileNames.Length); i++)
        {
            string goldenScript = File.ReadAllText(goldenScriptFileNames[i]);
            string testScript = File.ReadAllText(testScriptFileNames[i]);
            string result = _tester.Check(goldenScript, testScript);
            sb.AppendLine(result);
        }

        OutputTextBlock.Text = sb.ToString();
    }
}