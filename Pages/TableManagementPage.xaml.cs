using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class TableManagementPage : ContentPage
{
    // MAUI inyectará automáticamente el TableManagementPageModel aquí
    public TableManagementPage(TableManagementPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}