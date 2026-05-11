using CommunityToolkit.Mvvm.ComponentModel;

namespace ComandaApp.Models;

public partial class Mesa : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string numeroMesa = string.Empty;

    [ObservableProperty]
    private int capacidad;

    [ObservableProperty]
    private string area = "General";

    [ObservableProperty]
    private string qrCodeData = string.Empty;

    [ObservableProperty]
    private bool estaOcupada;
}