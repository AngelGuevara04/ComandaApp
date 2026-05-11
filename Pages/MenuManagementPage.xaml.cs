using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class MenuManagementPage : ContentPage
{
    public MenuManagementPage(MenuManagementPageModel viewModel)
    {
        InitializeComponent();

        // Asignamos el ViewModel inyectado como contexto de datos para la interfaz
        BindingContext = viewModel;
    }
}