using System.Collections.ObjectModel;

namespace Maximus.Models
{
    public class RaceBinData
    {
        public ObservableCollection<string> BossRaces { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Children { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> WorldRaces { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Milestones { get; set; } = new ObservableCollection<string>();
    }
}