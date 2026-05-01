using System;
using System.IO;
using System.Windows;
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
        MainFrame.Navigate(new RacePage());
    }

    private void OnMilestonesClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new MilestonesPage());
    }

    private void OnBlacklistClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new BlackListPage());
    }

    private void OnAboutAuthorClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new AboutAuthorPage());
    }

    private void OnLeadersClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new LeadersPage());
    }
    
    private void OnGenerateClick(object sender, RoutedEventArgs e)
    {
        Generate();
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
        
        ScriptParser parser = new();
        parser.Parse(MainViewModel.Config, content);
    }

}