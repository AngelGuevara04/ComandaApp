using System.Collections.ObjectModel;
using ComandaApp.Models;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels;

public partial class CorteCajaPageModel : ObservableObject
{
    private readonly OrdenService _ordenService;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private DateTime fechaCorte = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<OrdenMesa> ordenesDelCorte = new();

    public int CantidadOrdenes => OrdenesDelCorte.Count;

    public double TotalVendido => OrdenesDelCorte.Sum(o => o.TotalCuenta);

    public double TicketPromedio => CantidadOrdenes == 0 ? 0 : TotalVendido / CantidadOrdenes;

    public bool HayOrdenes => OrdenesDelCorte.Count > 0;

    public bool NoHayOrdenes => !HayOrdenes;

    public CorteCajaPageModel(OrdenService ordenService)
    {
        _ordenService = ordenService;
    }

    [RelayCommand]
    public async Task CargarCorteAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var ordenes = await _ordenService.GetOrdenesPagadasPorFechaAsync(FechaCorte);

            OrdenesDelCorte = new ObservableCollection<OrdenMesa>(ordenes);

            RefrescarResumen();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo cargar el corte: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RefrescarResumen()
    {
        OnPropertyChanged(nameof(CantidadOrdenes));
        OnPropertyChanged(nameof(TotalVendido));
        OnPropertyChanged(nameof(TicketPromedio));
        OnPropertyChanged(nameof(HayOrdenes));
        OnPropertyChanged(nameof(NoHayOrdenes));
    }
}