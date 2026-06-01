using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class TableManagementPage : ContentPage
{
    private readonly TableManagementPageModel _pageModel;

    public TableManagementPage(TableManagementPageModel pageModel)
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