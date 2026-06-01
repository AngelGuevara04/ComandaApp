using System.Security.Cryptography;
using System.Text;
using ComandaApp.Models;
using ComandaApp.Models.Records;

namespace ComandaApp.Services;

public class ConfigService
{
    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    public ConfigService(SupabaseService supabaseService, AuthService authService)
    {
        _supabaseService = supabaseService;
        _authService = authService;
    }

    public async Task<ConfiguracionNegocio> GetAsync()
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            var negocioId = _authService.NegocioIdActual;

            var result = await client.From<ConfiguracionRecord>().Get();

            var config = result.Models.FirstOrDefault(r => r.NegocioId == negocioId);

            return config?.ToModel() ?? new ConfiguracionNegocio();
        }
        catch
        {
            return new ConfiguracionNegocio();
        }
    }

    public async Task SaveAsync(ConfiguracionNegocio config)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var result = await client.From<ConfiguracionRecord>().Get();

        var existente = result.Models.FirstOrDefault(r => r.NegocioId == negocioId);

        var id = existente?.Id ?? CrearIdConfiguracion(negocioId);

        await client.From<ConfiguracionRecord>()
            .Upsert(config.ToRecord(negocioId, id));
    }

    private static int CrearIdConfiguracion(string negocioId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(negocioId));
        var value = Math.Abs(BitConverter.ToInt32(bytes, 0));

        if (value == 0)
        {
            value = 1;
        }

        return value;
    }
}