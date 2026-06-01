using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class MenuManagementPage : ContentPage
{
    public MenuManagementPage(MenuManagementPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MenuManagementPageModel viewModel)
        {
            await viewModel.CargarMenuAsync();
        }
    }
}