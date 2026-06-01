using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class HistorialPedidosPage : ContentPage
{
    public HistorialPedidosPage(HistorialPedidosPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HistorialPedidosPageModel viewModel)
        {
            await viewModel.CargarHistorialAsync();
        }
    }
}