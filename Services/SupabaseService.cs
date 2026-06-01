using Supabase;

namespace ComandaApp.Services;

public class SupabaseService
{
    private Supabase.Client? _client;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _initialized;

    public async Task<Supabase.Client> GetClientAsync()
    {
        if (_initialized && _client is not null)
            return _client;

        await _lock.WaitAsync();
        try
        {
            if (_initialized && _client is not null)
                return _client;

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            _client = new Supabase.Client(AppConfig.SupabaseUrl, AppConfig.SupabaseKey, options);
            await _client.InitializeAsync();
            _initialized = true;

            return _client;
        }
        finally
        {
            _lock.Release();
        }
    }
}