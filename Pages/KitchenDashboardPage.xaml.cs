using ComandaApp.PageModels;

namespace ComandaApp.Pages;

public partial class KitchenDashboardPage : ContentPage
{
    public KitchenDashboardPage(KitchenDashboardPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}