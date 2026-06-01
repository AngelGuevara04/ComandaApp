using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class BusinessConfigPage : ContentPage
{
    private readonly BusinessConfigPageModel _pageModel;

    public BusinessConfigPage(BusinessConfigPageModel pageModel)
    {
        InitializeComponent();
        _pageModel = pageModel;
        BindingContext = pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _pageModel.CargarAsync();
    }
}