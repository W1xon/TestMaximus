using System.Collections.ObjectModel;
using Maximus.Models.Entities.Engage;

namespace Maximus.Models;

public class EngageConfig : ObservableObject
{

    public EngageConfig()
    {
        EngageType = EngageTypes[0];
        RaceBin = RaceBins[0];
    }
    public ObservableCollection<string> EngageTypes { get; } = new()
    {
        "Engage",
        "Zone",
        "Speedtrap"
    };
    public ObservableCollection<string> RaceBins { get; } = new ObservableCollection<string>
    {
        "race_bin_01", "race_bin_02", "race_bin_03", "race_bin_04", "race_bin_05",
        "race_bin_06", "race_bin_07", "race_bin_08", "race_bin_09", "race_bin_10",
        "race_bin_11", "race_bin_12", "race_bin_13", "race_bin_14", "race_bin_15",
        "race_bin_challenge"
    };
    private bool _isEngageEnabled;
    public bool IsEngageEnabled
    {
        get => _isEngageEnabled;
        set => Set(ref _isEngageEnabled, value);
    }
    private bool _isZoneEnabled;
    public bool IsZoneEnabled
    {
        get => _isZoneEnabled;
        set => Set(ref _isZoneEnabled, value);
    }
    private bool _isSpeedtrapEnabled;

    public bool IsSpeedtrapEnabled
    {
        get => _isSpeedtrapEnabled;
        set => Set(ref _isSpeedtrapEnabled, value);
    }
    private string _raceBin;
    public string RaceBin
    {
        get => _raceBin;
        set => Set(ref _raceBin, value);
    }
    
    private string _engageType;
    public string EngageType
    {
        get => _engageType;
        set => Set(ref _engageType, value);
    }
    private EngageEntity _engage;

    public EngageEntity Engage
    {
        get => _engage;
        set => Set(ref _engage, value);
    }
    private ZoneEntity _zone;
    public ZoneEntity Zone
    {
        get => _zone;
        set => Set(ref _zone, value);
    }
    private EngageEntity _speedtrap;
    public EngageEntity  Speedtrap  
    {
        get => _speedtrap;
        set => Set(ref _speedtrap, value);
    }
}