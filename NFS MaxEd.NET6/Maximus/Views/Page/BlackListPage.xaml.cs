using Maximus.Models;
using Maximus.Services;
using Maximus.ViewModels;

namespace Maximus.Views;

public partial class BlackListPage : Page, IGeneratable
{
    public BlackListPage() => InitializeComponent();

    public CodeInfo GenerateCode() => new()
    {
        Line = new BlacklistParser(MainViewModel.Instance.Blacklist, new BlacklistStore()).GenerateCode(),
        Name = nameof(BlackListPage)
    };
}