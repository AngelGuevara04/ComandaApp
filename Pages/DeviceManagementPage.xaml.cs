using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class DeviceManagementPage : ContentPage
{
    public DeviceManagementPage(DeviceManagementPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DeviceManagementPageModel viewModel)
        {
            await viewModel.CargarDispositivosAsync();
        }
    }
}