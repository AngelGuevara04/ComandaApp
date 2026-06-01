using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginPageModel _pageModel;

    public LoginPage(LoginPageModel pageModel)
    {
        InitializeComponent();
        _pageModel = pageModel;
        BindingContext = pageModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _pageModel.Initialize();
    }
}