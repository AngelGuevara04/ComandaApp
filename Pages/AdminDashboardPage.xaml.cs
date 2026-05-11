using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage(AdminDashboardPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}