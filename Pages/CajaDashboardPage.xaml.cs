using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class CajaDashboardPage : ContentPage
{
    private readonly CajaDashboardPageModel _viewModel;

    public CajaDashboardPage(CajaDashboardPageModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.CargarOrdenesAsync();
    }
}