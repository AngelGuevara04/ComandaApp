namespace ComandaApp.Models;

public enum EstadoPedido
{
    Pendiente,       // El cliente lo acaba de pedir. Se puede cancelar/modificar.
    EnPreparacion,   // La cocina ya lo está haciendo. Bloqueado para el cliente.
    Listo,           // Listo para llevar a la mesa.
    Entregado,       // El mesero ya lo dejó en la mesa.
    Rechazado        // La cocina lo canceló (ej. falta de ingredientes).
}