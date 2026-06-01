using ComandaApp.Models;
using ComandaApp.Models.Records;
using Supabase.Gotrue;

namespace ComandaApp.Services;

public class AuthService
{
    private readonly SupabaseService _supabaseService;
    private UserAccount? _currentUser;

    private const string AdminSetupKey = "admin_setup_done";
    private const string SavedEmailKey = "saved_admin_email";
    private const string SavedPasswordKey = "saved_admin_password";

    public UserAccount? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser is not null;
    public string NegocioIdActual => _currentUser?.NegocioId ?? "default";

    public AuthService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public bool IsFirstRun()
    {
        return !Preferences.Default.Get(AdminSetupKey, false);
    }

    public async Task<bool> RestoreSessionAsync()
    {
        try
        {
            var email = await SecureStorage.Default.GetAsync(SavedEmailKey);
            var password = await SecureStorage.Default.GetAsync(SavedPasswordKey);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var result = await LoginAsync(email, password);

            if (!result.Success)
            {
                ClearSavedLogin();
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, string Error)> RegisterAdminAsync(
        string email,
        string password,
        string displayName)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "El correo y la contrasena son obligatorios.");
        }

        if (password.Length < 6)
        {
            return (false, "La contrasena debe tener al menos 6 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return (false, "El nombre es obligatorio.");
        }

        try
        {
            var cleanEmail = email.Trim().ToLowerInvariant();
            var negocioId = CrearNegocioId(cleanEmail);
            var client = await _supabaseService.GetClientAsync();

            var result = await client.Auth.SignUp(cleanEmail, password,
                new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "nombre", displayName.Trim() },
                        { "rol", "Admin" },
                        { "negocio_id", negocioId }
                    }
                });

            if (result?.User is null)
            {
                return (false, "No se pudo crear la cuenta. Intenta de nuevo.");
            }

            Preferences.Default.Set(AdminSetupKey, true);

            _currentUser = new UserAccount
            {
                Email = cleanEmail,
                DisplayName = displayName.Trim(),
                Role = "Admin",
                NegocioId = negocioId
            };

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("already registered", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Ya existe una cuenta con ese correo.");
            }

            return (false, $"Error al registrar: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Error)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Ingresa tu correo y contrasena.");
        }

        try
        {
            var cleanEmail = email.Trim().ToLowerInvariant();
            var negocioId = CrearNegocioId(cleanEmail);

            var client = await _supabaseService.GetClientAsync();
            var result = await client.Auth.SignIn(cleanEmail, password);

            if (result?.User is null)
            {
                return (false, "Correo o contrasena incorrectos.");
            }

            var nombre = result.User.UserMetadata?
                .GetValueOrDefault("nombre")?.ToString() ?? cleanEmail;

            var negocioDesdeMetadata = result.User.UserMetadata?
                .GetValueOrDefault("negocio_id")?.ToString();

            if (!string.IsNullOrWhiteSpace(negocioDesdeMetadata))
            {
                negocioId = negocioDesdeMetadata;
            }

            _currentUser = new UserAccount
            {
                Email = result.User.Email ?? cleanEmail,
                DisplayName = nombre,
                Role = "Admin",
                NegocioId = negocioId
            };

            await SaveLoginAsync(cleanEmail, password);

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("email_not_confirmed", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Email not confirmed", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "El correo no esta confirmado. Desactiva la confirmacion de email en Supabase.");
            }

            if (ex.Message.Contains("Invalid login", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Correo o contrasena incorrectos.");
            }

            return (false, $"Error al iniciar sesion: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Error)> LoginWithQrAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return (false, "El codigo no puede estar vacio.");
        }

        var cleanToken = token.Trim();

        var temporalResult = TryLoginWithTemporaryClientQr(cleanToken);

        if (temporalResult.Success)
        {
            return temporalResult;
        }

        try
        {
            var client = await _supabaseService.GetClientAsync();
            var tokenNormalizado = NormalizarCodigo(cleanToken);

            var dispositivosResult = await client.From<DispositivoRecord>().Get();

            var dispositivo = dispositivosResult.Models.FirstOrDefault(d =>
                NormalizarCodigo(d.QrCodeData) == tokenNormalizado);

            if (dispositivo != null)
            {
                var rolNormalizado = NormalizarRol(dispositivo.Rol);

                _currentUser = new UserAccount
                {
                    DisplayName = dispositivo.Nombre,
                    Role = rolNormalizado,
                    QrToken = cleanToken,
                    Extra = ObtenerExtraDispositivo(dispositivo, cleanToken, rolNormalizado),
                    NegocioId = string.IsNullOrWhiteSpace(dispositivo.NegocioId) ? "default" : dispositivo.NegocioId
                };

                return (true, string.Empty);
            }

            var mesasResult = await client.From<MesaRecord>().Get();

            var mesaPorQr = mesasResult.Models.FirstOrDefault(m =>
                NormalizarCodigo(m.QrCodeData) == tokenNormalizado);

            if (mesaPorQr != null)
            {
                _currentUser = new UserAccount
                {
                    DisplayName = "Cliente",
                    Role = "Cliente",
                    QrToken = cleanToken,
                    Extra = mesaPorQr.NumeroMesa,
                    NegocioId = string.IsNullOrWhiteSpace(mesaPorQr.NegocioId) ? "default" : mesaPorQr.NegocioId
                };

                return (true, string.Empty);
            }

            var numeroMesaDesdeQr = ObtenerMesaDesdeCodigo(cleanToken);

            if (!string.IsNullOrWhiteSpace(numeroMesaDesdeQr))
            {
                var mesaPorNombre = mesasResult.Models.FirstOrDefault(m =>
                    NormalizarMesa(m.NumeroMesa) == NormalizarMesa(numeroMesaDesdeQr));

                _currentUser = new UserAccount
                {
                    DisplayName = "Cliente",
                    Role = "Cliente",
                    QrToken = cleanToken,
                    Extra = mesaPorNombre?.NumeroMesa ?? numeroMesaDesdeQr,
                    NegocioId = mesaPorNombre?.NegocioId ?? "default"
                };

                return (true, string.Empty);
            }

            var rolDesdeQr = ObtenerRolDesdeCodigo(cleanToken);

            if (rolDesdeQr == "Cocina" || rolDesdeQr == "Caja")
            {
                _currentUser = new UserAccount
                {
                    DisplayName = rolDesdeQr,
                    Role = rolDesdeQr,
                    QrToken = cleanToken,
                    Extra = string.Empty,
                    NegocioId = "default"
                };

                return (true, string.Empty);
            }

            return (false, "Codigo QR invalido o no registrado.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al validar codigo: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            await client.Auth.SignOut();
        }
        catch
        {
        }
        finally
        {
            _currentUser = null;
            ClearSavedLogin();
        }
    }

    public void Logout()
    {
        _currentUser = null;
        ClearSavedLogin();
    }

    private (bool Success, string Error) TryLoginWithTemporaryClientQr(string token)
    {
        if (!token.StartsWith("CLIENTE_TEMP|", StringComparison.OrdinalIgnoreCase))
        {
            return (false, string.Empty);
        }

        var parts = token.Split('|');

        if (parts.Length < 4)
        {
            return (false, "Codigo temporal invalido.");
        }

        var negocioId = parts[1].Trim();
        var numeroMesa = parts[2].Trim();
        var nombreCliente = parts[3].Trim();

        if (string.IsNullOrWhiteSpace(negocioId))
        {
            negocioId = "default";
        }

        if (string.IsNullOrWhiteSpace(numeroMesa))
        {
            return (false, "El codigo temporal no contiene mesa.");
        }

        if (string.IsNullOrWhiteSpace(nombreCliente))
        {
            nombreCliente = "Cliente";
        }

        _currentUser = new UserAccount
        {
            DisplayName = nombreCliente,
            Role = "Cliente",
            QrToken = token,
            Extra = numeroMesa,
            NegocioId = negocioId
        };

        return (true, string.Empty);
    }

    private static string ObtenerExtraDispositivo(
        DispositivoRecord dispositivo,
        string token,
        string rolNormalizado)
    {
        if (rolNormalizado == "Cliente")
        {
            var mesaDesdeQr = ObtenerMesaDesdeCodigo(token);

            if (!string.IsNullOrWhiteSpace(mesaDesdeQr))
            {
                return mesaDesdeQr;
            }

            if (!string.IsNullOrWhiteSpace(dispositivo.DetalleExtra))
            {
                return dispositivo.DetalleExtra;
            }

            return dispositivo.Nombre;
        }

        return dispositivo.DetalleExtra;
    }

    private static string NormalizarRol(string rol)
    {
        return rol.Trim().ToLowerInvariant() switch
        {
            "caja" => "Caja",
            "cocina" => "Cocina",
            "cliente" => "Cliente",
            "mesa" => "Cliente",
            _ => "Admin"
        };
    }

    private static string NormalizarCodigo(string value)
    {
        return value
            .Trim()
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\t", string.Empty)
            .Replace(" ", string.Empty)
            .ToLowerInvariant();
    }

    private static string NormalizarMesa(string value)
    {
        return value
            .Trim()
            .Replace("_", " ")
            .ToLowerInvariant();
    }

    private static string ObtenerRolDesdeCodigo(string token)
    {
        var limpio = token.Trim().ToLowerInvariant();

        if (limpio.StartsWith("comanda_cocina_"))
        {
            return "Cocina";
        }

        if (limpio.StartsWith("comanda_caja_"))
        {
            return "Caja";
        }

        if (limpio.StartsWith("comanda_mesa_"))
        {
            return "Cliente";
        }

        return string.Empty;
    }

    private static string ObtenerMesaDesdeCodigo(string token)
    {
        var limpio = token.Trim();

        if (!limpio.StartsWith("comanda_mesa_", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var contenido = limpio["comanda_mesa_".Length..];
        var partes = contenido.Split('_', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 0)
        {
            return string.Empty;
        }

        if (partes.Length == 1)
        {
            return partes[0].Replace("_", " ").Trim();
        }

        var sinCodigoFinal = partes.Take(partes.Length - 1);

        return string.Join(" ", sinCodigoFinal).Trim();
    }

    public static string CrearNegocioId(string email)
    {
        return email
            .Trim()
            .ToLowerInvariant()
            .Replace("@", "_")
            .Replace(".", "_")
            .Replace("-", "_");
    }

    private static async Task SaveLoginAsync(string email, string password)
    {
        try
        {
            await SecureStorage.Default.SetAsync(SavedEmailKey, email);
            await SecureStorage.Default.SetAsync(SavedPasswordKey, password);
        }
        catch
        {
        }
    }

    private static void ClearSavedLogin()
    {
        try
        {
            SecureStorage.Default.Remove(SavedEmailKey);
            SecureStorage.Default.Remove(SavedPasswordKey);
        }
        catch
        {
        }
    }
}