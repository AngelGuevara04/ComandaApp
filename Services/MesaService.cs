using ComandaApp.Models;
using ComandaApp.Models.Records;

namespace ComandaApp.Services;

public class MesaService
{
    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    public MesaService(SupabaseService supabaseService, AuthService authService)
    {
        _supabaseService = supabaseService;
        _authService = authService;
    }

    public async Task<List<Mesa>> GetAllAsync()
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var result = await client.From<MesaRecord>().Get();

        return result.Models
            .Where(r => r.NegocioId == negocioId)
            .OrderBy(r => r.NumeroMesa)
            .Select(r => r.ToModel())
            .ToList();
    }

    public async Task AddAsync(Mesa mesa)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        await client.From<MesaRecord>()
            .Insert(mesa.ToRecord(negocioId));
    }

    public async Task DeleteAsync(string numeroMesa)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var result = await client.From<MesaRecord>().Get();

        var mesa = result.Models.FirstOrDefault(r =>
            r.NegocioId == negocioId &&
            NormalizarMesa(r.NumeroMesa) == NormalizarMesa(numeroMesa));

        if (mesa == null)
        {
            return;
        }

        await client.From<MesaRecord>()
            .Where(r => r.Id == mesa.Id)
            .Delete();
    }

    public async Task ActualizarEstadoAsync(string numeroMesa, bool ocupada)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var result = await client.From<MesaRecord>().Get();

        var mesa = result.Models.FirstOrDefault(r =>
            r.NegocioId == negocioId &&
            NormalizarMesa(r.NumeroMesa) == NormalizarMesa(numeroMesa));

        if (mesa == null)
        {
            return;
        }

        await client.From<MesaRecord>()
            .Where(r => r.Id == mesa.Id)
            .Set(r => r.EstaOcupada, ocupada)
            .Update();
    }

    private static string NormalizarMesa(string value)
    {
        return value
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .ToLowerInvariant();
    }
}