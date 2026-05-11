using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels;

public partial class AdminDashboardPageModel : ObservableObject
{
    [RelayCommand]
    private async Task GoToMesas() => await Shell.Current.GoToAsync("table_management");

    [RelayCommand]
    private async Task GoToMenu() => await Shell.Current.GoToAsync("menu_management");

    [RelayCommand]
    private async Task GoToDevices() => await Shell.Current.GoToAsync("device_management");

    [RelayCommand]
    private async Task GoToCorte() => await Shell.Current.GoToAsync("corte_caja");

    [RelayCommand]
    private async Task GoToHistory() => await Shell.Current.GoToAsync("order_history");

    [RelayCommand]
    private async Task GoToConfig() => await Shell.Current.GoToAsync("business_config");
}