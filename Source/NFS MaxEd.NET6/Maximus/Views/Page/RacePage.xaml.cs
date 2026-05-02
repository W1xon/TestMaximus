using System.Windows;
using System.Windows.Input;
using Maximus.Services;
using Maximus.ViewModels;
using System.Windows.Media;

namespace Maximus.Views;

public partial class RacePage : Page, IGeneratable
{
    public RacePage()
    {
        InitializeComponent();
    }

    private void OpenMap_Click(object sender, RoutedEventArgs e)
    {
        var mapWindow = new MapWindow()
        {
            Owner = Window.GetWindow(this),
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        mapWindow.Show();
    }

    public CodeInfo GenerateCode() => new CodeInfo()
    {
        Line = RaceParserFactory.CreateRaceParser(MainViewModel.Instance.Config).GenerateCode(),
        Name = MainViewModel.Instance.Config.NodeType.ToString()
    } ;
    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        MainViewModel.Instance.ResetConfigs();
        MainViewModel.Instance.UpdateVisibility();
    }


    private void ShowCashgrabInfo_OnClick(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this); 

        var infoWindow = new CashgrabInfo
        {
            Owner = window,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        infoWindow.ShowDialog();
    }


    private void ShowDragInfo_OnClick(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this); 

        var infoWindow = new DrugWindow()
        {
            Owner = window,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        infoWindow.ShowDialog();
    }
    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        var hitElement = e.OriginalSource as DependencyObject;
    
        if (IsInsideVerticalScrollViewer(hitElement))
        {
            return; 
        }
    
        if (e.Delta > 0)
        {
            scrollViewer.LineLeft();
        }
        else
        {
            scrollViewer.LineRight();
        }
    
        e.Handled = true;
    }
    
    private bool IsInsideVerticalScrollViewer(DependencyObject element)
    {
        while (element != null && element != SettingsScrollViewer)
        {
            if (element is ScrollViewer sv && sv.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                if (sv.ScrollableHeight > 0)
                    return true;
            }
            element = VisualTreeHelper.GetParent(element);
        }
        return false;
    }
}