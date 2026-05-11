using ComandaApp.Models;
using ComandaApp.PageModels;

namespace ComandaApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}