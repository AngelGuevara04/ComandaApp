using ComandaApp.PageModels;
using ComandaApp.Services;

namespace ComandaApp.Pages;

public partial class ClienteMenuPage : ContentPage
{
    private readonly ClienteMenuPageModel _pageModel;
    private readonly RealtimeService _realtimeService;

    private string _subscriptionKey = string.Empty;

    public ClienteMenuPage( 
        ClienteMenuPageModel pageModel,
        RealtimeService realtimeService)
    {
        InitializeComponent();

        _pageModel = pageModel;
        _realtimeService = realtimeService;
        BindingContext = pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _pageModel.InicializarAsync();

            _subscriptionKey = ObtenerSubscriptionKey();

            await _realtimeService.SuscribirseAsync(
                _subscriptionKey,
                new[] { "ordenes", "detalles_pedido", "platillos" },
                async () => await _pageModel.ActualizarDatosAsync());
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error de Conexión", $"No se pudo establecer conexión en tiempo real. {ex.Message}", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (!string.IsNullOrWhiteSpace(_subscriptionKey))
        {
            _realtimeService.DetenerSuscripcion(_subscriptionKey);
        }
    }

    private string ObtenerSubscriptionKey()
    {
        if (string.IsNullOrWhiteSpace(_pageModel.NumeroMesa))
        {
            return $"cliente_menu_{Guid.NewGuid():N}";
        }

        return $"cliente_menu_{_pageModel.NumeroMesa}";
    }
}