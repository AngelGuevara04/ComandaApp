namespace ComandaApp.Models;

public class Mesa
{
    public int Id { get; set; }
    public string NumeroMesa { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public string Area { get; set; } = "General";
    public string QrCodeData { get; set; } = string.Empty; // Aquí guardaremos el ID único del QR
    public bool EstaOcupada { get; set; }
}