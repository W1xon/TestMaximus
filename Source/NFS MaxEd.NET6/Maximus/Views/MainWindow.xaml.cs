using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Maximus.Services;
using Maximus.Services.Parsers;
using Maximus.ViewModels;
using Microsoft.Win32;

namespace Maximus.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    private MainViewModel MainViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();
        
        Loaded += MainWindow_Loaded;
        
        MainViewModel = MainViewModel.Instance;
        DataContext = MainViewModel;
        MainFrame.Navigated += MainFrame_Navigated;
        MainFrame.Content = new RacePage();
        ColoringButton("RacesButton");
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (!FirstRunService.IsFirstRun()) return;
        DateTime startTime = DateTime.UtcNow;
        TimeSpan timer = TimeSpan.FromSeconds(10);
        
        while (!UpdateChecker.HasChecked || DateTime.UtcNow - startTime < timer)
        {
            await Task.Delay(100);
        }
        if(!UpdateChecker.IsUpdateAvailable)
            ShowWelcomeWindow();
    }

    private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
    {
        if (e.Content is Page page)
        {
            UpdateWindowControls(page);
        }
    }

    private void UpdateWindowControls(Page page)
    {
        page.DataContext = MainViewModel;
    }


    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void DragWindow(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }


    private void OnRacesClick(object sender, RoutedEventArgs e)
    {
        ColoringButton("RacesButton");
        SaveButton.IsEnabled = true;
        MainFrame.Navigate(new RacePage());
    }

    private void OnMilestonesClick(object sender, RoutedEventArgs e)
    {
        ColoringButton("MilestonesButton");
        SaveButton.IsEnabled = true;
        MainFrame.Navigate(new MilestonesPage());
    }

    private void OnBlacklistClick(object sender, RoutedEventArgs e)
    {
        ColoringButton("BlacklistButton");
        SaveButton.IsEnabled = true;
        MainFrame.Navigate(new BlackListPage());
    }

    private void OnAboutAuthorClick(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;
        MainFrame.Navigate(new AboutAuthorPage());
    }

    private void OnLeadersClick(object sender, RoutedEventArgs e)
    {
        ColoringButton("LeadersButton");
        SaveButton.IsEnabled = true;
        MainFrame.Navigate(new LeadersPage());
    }
    
    private void OnGenerateClick(object sender, RoutedEventArgs e)
    {
        Generate();
    }
    
   private void ColoringButton(string buttonName)
   {
       var names = new[] { "RacesButton", "MilestonesButton", "BlacklistButton", "LeadersButton" };
       var backgroundMedium = TryFindResource("BackgroundMedium") as Brush ?? Brushes.Transparent;
       var accent = TryFindResource("AccentColor") as Brush ?? Brushes.Transparent;
   
       foreach (var name in names)
       {
           if (FindName(name) is Button buttonOther)
               buttonOther.Background = backgroundMedium;
       }
   
       if (FindName(buttonName) is Button button)
           button.Background = accent;
   }
    private void ShowWelcomeWindow()
    {
        var welcomeWindow = new WelcomeWindow();
        
        welcomeWindow.ShowDialog();
    }
    private void DropZone_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (sender is Button btn)
            {
                btn.BorderBrush = (Brush)FindResource("AccentColor");
            
                var accentColor = ((SolidColorBrush)FindResource("AccentColor")).Color;
                btn.Background = new SolidColorBrush(Color.FromArgb(40, accentColor.R, accentColor.G, accentColor.B));
            }
        }
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        if (sender is Button btn)
        {
            btn.BorderBrush = (Brush)FindResource("BorderColor");
            btn.Background = (Brush)FindResource("BackgroundLight"); 
        }
    }

    private void UIElement_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is Button btn)
        {
            btn.BorderBrush = (Brush)FindResource("BorderColor");
            btn.Background = (Brush)FindResource("BackgroundLight");
        }

        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
    
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files?.Length > 0)
        {
            Parse(filePath: files[0]);
        }
    }

    private void OpenScript_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Выберите скрипт для открытия", Filter = "NFSMS Files (*.nfsms)|*.nfsms" };
        if (dialog.ShowDialog() != true) return;
        string filePath = dialog.FileName;

        Parse(filePath: filePath);
    }

    private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        bool isPasteCombo = e.Key == Key.V && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift);
        bool isSaveCombo = e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control;
        bool isOpenCombo = e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control;
        bool isPreviewCombo = e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control;
        if (isPasteCombo)
        {

            e.Handled = true;

            if (!Clipboard.ContainsText()) return;

            string clipboardText = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(clipboardText)) return;

            try
            {
                Parse(content: clipboardText);
            }
            catch (Exception ex)
            {
                string message =
                    "ОШИБКА ПАРСИНГА\n" +
                    "_________________________________\n\n" +
                    "Не удалось обработать скрипт из буфера.\n\n" +
                    "Причина:\n" +
                    $"• {ex.Message}\n" +
                    "_________________________________";

                MessageBox.Show(message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }
        else if(isSaveCombo)
        {
            Generate();
        }
        else if (isOpenCombo)
        {
            OpenScript_Click(sender, e);
        }
        else if (isPreviewCombo)
        {
            e.Handled = true;
            TogglePreviewPanel();
        }
    }

    private void Generate()
    {
        if (MainFrame.Content is not IGeneratable page)
        {
            MessageBox.Show("Эта страница не поддерживает генерацию скрипта.",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        
        CodeInfo codeInfo = page.GenerateCode();
        FileService.SaveNFSMS(codeInfo.Line, codeInfo.Name);
    }
    private void Parse(string filePath = null, string content = null)
    {
        if(filePath is not null)
            content = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(content)) return;

        var page = MainFrame.Content as Page;
        if( page is not IGeneratable)
        {
            MessageBox.Show("Эта страница не поддерживает парсинг скрипта.",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (page is RacePage)
        {
            RaceScriptParser parser = new(MainViewModel.Config);
            parser.Parse(content);
        }
        else if (page is MilestonesPage)
        {
            MilestoneScriptParser parser = new(MainViewModel.MilestoneConfig);
            parser.Parse(content);
        }
        else if (page is BlackListPage)
        {
            BlackListScriptParser parser = new(MainViewModel.BlacklistConfig);
            parser.Parse(content);
        }
    }

    private double _navPanelNormalWidth;

    private void TogglePreviewPanel()
    {
        if (double.IsNaN(NavigationPanel.Width))
        {
            _navPanelNormalWidth = NavigationPanel.ActualWidth;
        }

        var navAnimation = new System.Windows.Media.Animation.DoubleAnimation();
        navAnimation.Duration = TimeSpan.FromMilliseconds(250);
        navAnimation.EasingFunction = new System.Windows.Media.Animation.CubicEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };

        var previewAnimation = new System.Windows.Media.Animation.DoubleAnimation();
        previewAnimation.Duration = TimeSpan.FromMilliseconds(250);
        previewAnimation.EasingFunction = new System.Windows.Media.Animation.CubicEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };
        
        if (PreviewBorder.Width == 0)
        {
            NavigationPanel.Width = NavigationPanel.ActualWidth;
            navAnimation.To = 0;
            previewAnimation.To = 500;
            UpdatePreviewContent();
        }
        else
        {
            navAnimation.To = _navPanelNormalWidth;
            previewAnimation.To = 0;

            navAnimation.Completed += (s, e) =>
            {
                NavigationPanel.BeginAnimation(FrameworkElement.WidthProperty, null);
                NavigationPanel.Width = double.NaN;
            };
        }
        
        NavigationPanel.BeginAnimation(FrameworkElement.WidthProperty, navAnimation);
        PreviewBorder.BeginAnimation(FrameworkElement.WidthProperty, previewAnimation);
    }

    private void UpdatePreviewContent()
    {
        if (MainFrame.Content is not IGeneratable page) return;
        CodeInfo? codeInfo = null;
        try
        { 
            codeInfo = page.GenerateCode();
        }
        catch (Exception e)
        {
            codeInfo = new CodeInfo()
            {
                Line = $"// Не удалось сгенерировать код для предпросмотра\n// Причина:\n// {e.Message}",
                Name = "Error"
            };
        }
        if (string.IsNullOrWhiteSpace(codeInfo.Line)) return;

        FlowDocument document = new FlowDocument();
        Paragraph paragraph = new Paragraph();

        string code = codeInfo.Line;
        
        string pattern = @"(?<comment>//[^\r\n]*)|(?<string>""[^""]*"")|(?<keyword>\b(add_node|add_field|update_field|resize_field|change_vault|gameplay|True|False|true|false)\b)";
        
        int lastIndex = 0;
        foreach (Match match in Regex.Matches(code, pattern))
        {
            if (match.Index > lastIndex)
            {
                paragraph.Inlines.Add(new Run(code.Substring(lastIndex, match.Index - lastIndex)) { Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4D4D4")) });
            }

            Brush colorBrush;
            if (match.Groups["comment"].Success)
            {
                colorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            }
            else if (match.Groups["string"].Success)
            {
                colorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A8759"));
            }
            else if (match.Groups["keyword"].Success)
            {
                colorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC7832"));
            }
            else
            {
                colorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4D4D4"));
            }

            paragraph.Inlines.Add(new Run(match.Value) { Foreground = colorBrush });
            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < code.Length)
        {
            paragraph.Inlines.Add(new Run(code.Substring(lastIndex)) { Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4D4D4")) });
        }

        document.Blocks.Add(paragraph);
        PreviewRichTextBox.Document = document;
    }
}