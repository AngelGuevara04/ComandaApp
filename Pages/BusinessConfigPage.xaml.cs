using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class BusinessConfigPage : ContentPage
{
    public BusinessConfigPage(BusinessConfigPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}