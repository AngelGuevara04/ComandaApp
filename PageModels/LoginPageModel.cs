using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Services;
using ComandaApp.Pages;

namespace ComandaApp.PageModels;

public partial class LoginPageModel : ObservableObject
{
    private readonly AuthService _authService;

    public LoginPageModel(AuthService authService)
    {
        _authService = authService;
    }

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool showRegisterForm;

    [ObservableProperty]
    private int selectedLoginMode;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isPasswordVisible;

    [ObservableProperty]
    private string qrToken = string.Empty;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    public bool IsCredentialsMode => SelectedLoginMode == 0;
    public bool IsQrMode => SelectedLoginMode == 1;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    partial void OnSelectedLoginModeChanged(int value)
    {
        OnPropertyChanged(nameof(IsCredentialsMode));
        OnPropertyChanged(nameof(IsQrMode));
        ErrorMessage = string.Empty;
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    public void Initialize()
    {
        ShowRegisterForm = false;
        SelectedLoginMode = 0;
        ErrorMessage = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        DisplayName = string.Empty;
        QrToken = string.Empty;
    }

    [RelayCommand]
    private void SelectCredentials()
    {
        SelectedLoginMode = 0;
    }

    [RelayCommand]
    private void SelectQr()
    {
        SelectedLoginMode = 1;
    }

    [RelayCommand]
    private void ShowRegister()
    {
        ErrorMessage = string.Empty;
        ShowRegisterForm = true;
        SelectedLoginMode = 0;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    [RelayCommand]
    private void ShowLogin()
    {
        ErrorMessage = string.Empty;
        ShowRegisterForm = false;
        SelectedLoginMode = 0;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    [RelayCommand]
    private async Task AbrirScanner()
    {
        ErrorMessage = string.Empty;
        await Shell.Current.GoToAsync(nameof(QrScannerPage));
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        var result = await _authService.LoginAsync(Email, Password);

        IsLoading = false;

        if (!result.Success)
        {
            ErrorMessage = result.Error;
            return;
        }

        await Shell.Current.GoToAsync("//admin_dashboard");
    }

    [RelayCommand]
    private async Task LoginWithQrAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        var result = await _authService.LoginWithQrAsync(QrToken);

        IsLoading = false;

        if (!result.Success)
        {
            ErrorMessage = result.Error;
            return;
        }

        var user = _authService.CurrentUser;
        var rol = user?.Role.Trim().ToLowerInvariant();

        switch (rol)
        {
            case "cliente":
                await Shell.Current.GoToAsync($"//cliente_menu?numeroMesa={Uri.EscapeDataString(user?.Extra ?? string.Empty)}");
                break;

            case "cocina":
                await Shell.Current.GoToAsync("//kitchen_dashboard");
                break;

            case "caja":
                await Shell.Current.GoToAsync("//caja_dashboard");
                break;

            default:
                await Shell.Current.GoToAsync("//admin_dashboard");
                break;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(DisplayName) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Todos los campos son obligatorios.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Las contrasenas no coinciden.";
            return;
        }

        IsLoading = true;

        var registerResult = await _authService.RegisterAdminAsync(Email, Password, DisplayName);

        if (!registerResult.Success)
        {
            IsLoading = false;
            ErrorMessage = registerResult.Error;
            return;
        }

        var loginResult = await _authService.LoginAsync(Email, Password);

        IsLoading = false;

        if (loginResult.Success)
        {
            await Shell.Current.GoToAsync("//admin_dashboard");
            return;
        }

        ShowRegisterForm = false;
        ConfirmPassword = string.Empty;
        Password = string.Empty;
        ErrorMessage = "Cuenta creada. Ahora inicia sesion.";
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }
}