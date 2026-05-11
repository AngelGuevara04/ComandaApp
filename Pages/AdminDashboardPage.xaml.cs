using ComandaApp.PageModels;

namespace ComandaApp.Pages; // <-- Verifica que esto coincida con tu carpeta

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage(AdminDashboardPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}