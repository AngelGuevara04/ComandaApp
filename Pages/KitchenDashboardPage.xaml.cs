using ComandaApp.PageModels;
using ComandaApp.Services;

namespace ComandaApp.Pages;

public partial class KitchenDashboardPage : ContentPage
{
    private readonly KitchenDashboardPageModel _viewModel;
    private readonly RealtimeService _realtimeService;
    private bool _suscrito;

    public KitchenDashboardPage(KitchenDashboardPageModel viewModel, RealtimeService realtimeService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _realtimeService = realtimeService;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.CargarDatosAsync();

        if (!_suscrito)
        {
            try
            {
                await _realtimeService.SuscribirseAsync(
                    "cocina",
                    "ordenes",
                    async () => await _viewModel.CargarDatosAsync());

                await _realtimeService.SuscribirseAsync(
                    "cocina",
                    "detalles_pedido",
                    async () => await _viewModel.CargarDatosAsync());

                _suscrito = true;
            }
            catch { }
        }
    }
}