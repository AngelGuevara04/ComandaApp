using ComandaApp.Models;
using ComandaApp.Models.Records;

namespace ComandaApp.Services;

public class MenuService
{
    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    public MenuService(SupabaseService supabaseService, AuthService authService)
    {
        _supabaseService = supabaseService;
        _authService = authService;
    }

    public async Task<List<Platillo>> GetAllAsync()
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var result = await client.From<PlatilloRecord>().Get();

        return result.Models
            .Where(r => r.NegocioId == negocioId)
            .OrderBy(r => r.Categoria)
            .ThenBy(r => r.Nombre)
            .Select(r => r.ToModel())
            .ToList();
    }

    public async Task<List<Platillo>> GetDisponiblesAsync()
    {
        var platillos = await GetAllAsync();

        return platillos
            .Where(p => p.EstaDisponible)
            .OrderBy(p => p.Categoria)
            .ThenBy(p => p.Nombre)
            .ToList();
    }

    public async Task AddAsync(Platillo platillo)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        await client.From<PlatilloRecord>()
            .Insert(platillo.ToRecord(negocioId));
    }

    public async Task UpdateAsync(Platillo platillo)
    {
        var client = await _supabaseService.GetClientAsync();

        await client.From<PlatilloRecord>()
            .Where(r => r.Id == platillo.Id)
            .Set(r => r.Nombre, platillo.Nombre)
            .Set(r => r.Descripcion, platillo.Descripcion)
            .Set(r => r.Precio, platillo.Precio)
            .Set(r => r.Categoria, platillo.Categoria)
            .Set(r => r.ImagenUrl, platillo.ImagenSource)
            .Set(r => r.Disponible, platillo.EstaDisponible)
            .Update();
    }

    public async Task DeleteAsync(string id)
    {
        var client = await _supabaseService.GetClientAsync();

        await client.From<PlatilloRecord>()
            .Where(r => r.Id == id)
            .Delete();
    }

    public async Task ActualizarDisponibilidadAsync(string id, bool disponible)
    {
        var client = await _supabaseService.GetClientAsync();

        await client.From<PlatilloRecord>()
            .Where(r => r.Id == id)
            .Set(r => r.Disponible, disponible)
            .Update();
    }
}