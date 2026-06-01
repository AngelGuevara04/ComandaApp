using Supabase.Realtime;
using Supabase.Realtime.PostgresChanges;

namespace ComandaApp.Services;

public class RealtimeService
{
    private readonly SupabaseService _supabaseService;
    private readonly Dictionary<string, List<RealtimeChannel>> _channels = new();
    private readonly HashSet<string> _refreshing = new();
    private readonly object _lock = new();

    public RealtimeService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task SuscribirseAsync(string subscriptionKey, string[] tables, Func<Task> onChanged)
    {
        if (string.IsNullOrWhiteSpace(subscriptionKey))
        {
            return;
        }

        if (tables.Length == 0)
        {
            return;
        }

        lock (_lock)
        {
            if (_channels.ContainsKey(subscriptionKey))
            {
                return;
            }

            _channels[subscriptionKey] = new List<RealtimeChannel>();
        }

        var client = await _supabaseService.GetClientAsync();

        foreach (var table in tables)
        {
            var channelName = $"{subscriptionKey}_{table}_{Guid.NewGuid():N}";
            var channel = client.Realtime.Channel(channelName);

            channel.Register(new PostgresChangesOptions(
                schema: "public",
                table: table,
                eventType: PostgresChangesOptions.ListenType.All));

            channel.AddPostgresChangeHandler(
                PostgresChangesOptions.ListenType.All,
                (_, _) =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await EjecutarCallbackSeguroAsync(subscriptionKey, onChanged);
                    });
                });

            await channel.Subscribe();

            lock (_lock)
            {
                if (_channels.TryGetValue(subscriptionKey, out var channelList))
                {
                    channelList.Add(channel);
                }
            }
        }
    }

    public void DetenerSuscripcion(string subscriptionKey)
    {
        if (string.IsNullOrWhiteSpace(subscriptionKey))
        {
            return;
        }

        List<RealtimeChannel>? channels;

        lock (_lock)
        {
            if (!_channels.TryGetValue(subscriptionKey, out channels))
            {
                return;
            }

            _channels.Remove(subscriptionKey);
            _refreshing.Remove(subscriptionKey);
        }

        foreach (var channel in channels)
        {
            try
            {
                channel.Unsubscribe();
            }
            catch
            {
            }
        }
    }

    private async Task EjecutarCallbackSeguroAsync(string subscriptionKey, Func<Task> callback)
    {
        lock (_lock)
        {
            if (_refreshing.Contains(subscriptionKey))
            {
                return;
            }

            _refreshing.Add(subscriptionKey);
        }

        try
        {
            await Task.Delay(300);
            await callback();
        }
        catch
        {
        }
        finally
        {
            lock (_lock)
            {
                _refreshing.Remove(subscriptionKey);
            }
        }
    }
}