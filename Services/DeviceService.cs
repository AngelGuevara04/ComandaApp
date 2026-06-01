using ComandaApp.Models;
using ComandaApp.Models.Records;

namespace ComandaApp.Services;

public class DeviceService
{
    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    public DeviceService(SupabaseService supabaseService, AuthService authService)
    {
        _supabaseService = supabaseService;
        _authService = authService;
    }

    public async Task<List<Dispositivo>> GetAllAsync()
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var result = await client.From<DispositivoRecord>().Get();

        return result.Models
            .Where(r => r.NegocioId == negocioId)
            .OrderBy(r => r.Nombre)
            .Select(r => r.ToModel())
            .ToList();
    }

    public async Task AddAsync(Dispositivo d)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        await client.From<DispositivoRecord>()
            .Insert(d.ToRecord(negocioId));
    }

    public async Task DeleteAsync(string id)
    {
        var client = await _supabaseService.GetClientAsync();

        await client.From<DispositivoRecord>()
            .Where(r => r.Id == id)
            .Delete();
    }

    public async Task DeleteByRolAsync(RolDispositivo rol)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var result = await client.From<DispositivoRecord>().Get();

        var dispositivos = result.Models
            .Where(r => r.NegocioId == negocioId && r.Rol == rol.ToString())
            .ToList();

        foreach (var dispositivo in dispositivos)
        {
            await client.From<DispositivoRecord>()
                .Where(r => r.Id == dispositivo.Id)
                .Delete();
        }
    }
}