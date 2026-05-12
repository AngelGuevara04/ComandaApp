using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class AddDevicePage : ContentPage
{
    public AddDevicePage(AddDevicePageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}