using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels;

public partial class AdminDashboardPageModel : ObservableObject
{
    [RelayCommand]
    private async Task goToMesas() => await Shell.Current.GoToAsync("table_management");

    [RelayCommand]
    private async Task goToMenu() => await Shell.Current.GoToAsync("menu_management");

    [RelayCommand]
    private async Task goToDevices() => await Shell.Current.GoToAsync("device_management");

    [RelayCommand]
    private async Task goToCorte() => await Shell.Current.GoToAsync("corte_caja");

    [RelayCommand]
    private async Task goToHistory() => await Shell.Current.GoToAsync("order_history");

    [RelayCommand]
    private async Task goToConfig() => await Shell.Current.GoToAsync("business_config");
}