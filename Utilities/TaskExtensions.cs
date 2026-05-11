namespace ComandaApp;

public static class TaskExtensions
{
    /// <summary>
    /// Ejecuta una tarea de forma segura sin esperar su resultado (Fire and Forget).
    /// </summary>
    public static async void FireAndForgetSafeAsync(this Task task)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en tarea asíncrona: {ex.Message}");
        }
    }
}