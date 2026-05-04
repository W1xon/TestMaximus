using Maximus.ViewModels;

namespace Maximus.Views;

public partial class EngagePage : Page
{
    public EngagePage()
    {
        InitializeComponent();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        MainViewModel.Instance.EngageConfig.IsEngageEnabled = false;
        MainViewModel.Instance.EngageConfig.IsZoneEnabled = false;
        MainViewModel.Instance.EngageConfig.IsSpeedtrapEnabled = false;
        
        var engageType = MainViewModel.Instance.EngageConfig.EngageType;
        if (engageType == "Engage")
            MainViewModel.Instance.EngageConfig.IsEngageEnabled = true;
        else if (engageType == "Zone")
            MainViewModel.Instance.EngageConfig.IsZoneEnabled = true;
        else if (engageType == "Speedtrap")
            MainViewModel.Instance.EngageConfig.IsSpeedtrapEnabled = true;
    }
}