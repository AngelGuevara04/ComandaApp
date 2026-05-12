using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class DeviceManagementPage : ContentPage
{
    public DeviceManagementPage(DeviceManagementPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}