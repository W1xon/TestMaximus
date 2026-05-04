using System.Collections.ObjectModel;

namespace Maximus.Models;

public class EngageConfig : ObservableObject
{
    public ObservableCollection<string> EngageTypes { get; } = new()
    {
        "Engage",
        "Zone",
        "Speedtrap"
    };
    
    private string _engageType;
    public string EngageType
    {
        get => _engageType;
        set => Set(ref _engageType, value);
    }
}