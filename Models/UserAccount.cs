namespace ComandaApp.Models;

public class UserAccount
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Admin";

    public string QrToken { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string Extra { get; set; } = string.Empty;

    public string NegocioId { get; set; } = string.Empty;
}