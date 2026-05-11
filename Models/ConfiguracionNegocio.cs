using CommunityToolkit.Mvvm.ComponentModel;

namespace ComandaApp.Models;

public partial class ConfiguracionNegocio : ObservableObject
{
    [ObservableProperty]
    private string nombreRestaurante = string.Empty;

    [ObservableProperty]
    private string rfc = string.Empty;

    [ObservableProperty]
    private string direccion = string.Empty;

    [ObservableProperty]
    private string logoUrl = string.Empty;
}