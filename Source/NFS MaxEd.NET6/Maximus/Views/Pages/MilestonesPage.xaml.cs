using System.Windows;
using Maximus.Services;
using Maximus.ViewModels;

namespace Maximus.Views;

public partial class MilestonesPage : Page, IGeneratable
{
    public MilestonesPage()
    {
        InitializeComponent();
    }

    public CodeInfo GenerateCode() => new CodeInfo()
    {
        Line = new MilestoneParser(MainViewModel.Instance.MilestoneConfig).GenerateCode(),
        Name = "MilestonesPage"
    } ;
}