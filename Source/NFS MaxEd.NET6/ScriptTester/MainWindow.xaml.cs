using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace ScriptTester;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Tester _tester = new();
    private readonly OpenFileDialog _fileDialog = new() 
    { 
        Filter = "NFSMS Script files (*.nfsms)|*.nfsms" 
    };


    public MainWindow() => InitializeComponent();

    private ListBoxItem CreateLineItem(int num, string text, bool isMatch)
    {
        return new ListBoxItem
        {
            Content = $"{num:D3} | {text}",
            Foreground = isMatch ? Brushes.LightGray : Brushes.Tomato,
            Background = isMatch ? Brushes.Transparent : new SolidColorBrush(Color.FromArgb(30, 255, 0, 0))
        };
    }
    private void SelectFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Multiselect = true, Title = "Выберите GOLDEN скрипты" };
        if (dialog.ShowDialog() != true) return;
        string[] goldens = dialog.FileNames;

        dialog.Title = "Выберите TEST скрипты (в том же порядке)";
        if (dialog.ShowDialog() != true) return;
        string[] tests = dialog.FileNames;

        var pairs = new List<ScriptPair>();
        int count = Math.Min(goldens.Length, tests.Length);

        for (int i = 0; i < count; i++)
        {
            var goldenContent = File.ReadAllText(goldens[i]);
            var testContent = File.ReadAllText(tests[i]);
        
            var result = _tester.Check(goldenContent, testContent);
        
            pairs.Add(new ScriptPair {
                FileName = Path.GetFileName(goldens[i]),
                GoldenPath = goldens[i],
                TestPath = tests[i],
                IsMatch = result.IsFullMatch
            });
        }

        PairsListBox.ItemsSource = pairs;
    }

    private void PairsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PairsListBox.SelectedItem is ScriptPair selected)
        {
            GoldenLabel.Text = $"Golden: {selected.GoldenPath}";
            TestLabel.Text = $"Test: {selected.TestPath}";

            var result = _tester.Check(File.ReadAllText(selected.GoldenPath), File.ReadAllText(selected.TestPath));
        
            GoldenLinesList.Items.Clear();
            TestLinesList.Items.Clear();

            foreach (var row in result.Rows)
            {
                GoldenLinesList.Items.Add(CreateLineItem(row.LineNumber, row.GoldenLine, row.IsMatch));
                TestLinesList.Items.Add(CreateLineItem(row.LineNumber, row.TestLine, row.IsMatch));
            }
        }
    }
    private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
            this.WindowState = WindowState.Normal;
        else
            this.WindowState = WindowState.Maximized;
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}