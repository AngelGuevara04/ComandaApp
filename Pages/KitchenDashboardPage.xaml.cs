using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class KitchenDashboardPage : ContentPage
{
    private readonly KitchenDashboardPageModel _viewModel;

    public KitchenDashboardPage(KitchenDashboardPageModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.CargarDatosAsync();
    }
}