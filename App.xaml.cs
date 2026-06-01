using ComandaApp.Services;

namespace ComandaApp;

public partial class App : Application
{
    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    public App(SupabaseService supabaseService, AuthService authService)
    {
        InitializeComponent();

        _supabaseService = supabaseService;
        _authService = authService;

        _ = Task.Run(_supabaseService.GetClientAsync);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();
        var window = new Window(shell);

        _ = Task.Run(async () =>
        {
            await Task.Delay(600);

            var restored = await _authService.RestoreSessionAsync();

            if (restored)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await shell.GoToAsync("//admin_dashboard");
                });
            }
        });

        return window;
    }
}