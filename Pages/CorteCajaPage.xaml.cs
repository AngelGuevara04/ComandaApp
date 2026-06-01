using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class CorteCajaPage : ContentPage
{
    public CorteCajaPage(CorteCajaPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CorteCajaPageModel viewModel)
        {
            await viewModel.CargarCorteAsync();
        }
    }
}