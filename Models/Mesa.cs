namespace ComandaApp.Models;

public class Mesa
{
    public int Id { get; set; }
    public string NumeroMesa { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public string Area { get; set; } = "General"; // Ejemplo: Terraza, Planta Alta
    public string QrCodeData { get; set; } = string.Empty; // Identificador único para el QR
    public bool EstaOcupada { get; set; }
}