using System.Collections.ObjectModel;
using ComandaApp.Models;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels;

public partial class HistorialPedidosPageModel : ObservableObject
{
    private readonly OrdenService _ordenService;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private DateTime fechaSeleccionada = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<OrdenMesa> ordenesPagadas = new();

    public int TotalOrdenes => OrdenesPagadas.Count;

    public double TotalDia => OrdenesPagadas.Sum(o => o.TotalCuenta);

    public bool HayOrdenes => OrdenesPagadas.Count > 0;

    public bool NoHayOrdenes => !HayOrdenes;

    public HistorialPedidosPageModel(OrdenService ordenService)
    {
        _ordenService = ordenService;
    }

    [RelayCommand]
    public async Task CargarHistorialAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var ordenes = await _ordenService.GetOrdenesPagadasPorFechaAsync(FechaSeleccionada);

            OrdenesPagadas = new ObservableCollection<OrdenMesa>(ordenes);

            RefrescarResumen();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo cargar el historial: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RefrescarResumen()
    {
        OnPropertyChanged(nameof(TotalOrdenes));
        OnPropertyChanged(nameof(TotalDia));
        OnPropertyChanged(nameof(HayOrdenes));
        OnPropertyChanged(nameof(NoHayOrdenes));
    }
}