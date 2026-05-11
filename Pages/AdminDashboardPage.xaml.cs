using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage(AdminDashboardPageModel viewModel)
    {
        InitializeComponent();

        // Esta línea es la magia que conecta los botones del XAML con las funciones del ViewModel
        BindingContext = viewModel;
    }
}