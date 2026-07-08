using ItemsWorkService.Models;

namespace ItemsWorkService.Services
{
    /// <summary>
    /// Servicio encargado de la distribución automática de ítems de trabajo.
    /// </summary>
    public interface IDistributionService
    {
        /// <summary>
        /// Recibe un ítem de trabajo sin asignar, aplica las reglas de negocio 
        /// (fechas, relevancia, saturación) y lo devuelve con el 'AssignedUsername' establecido, 
        /// junto con detalles de la asignación.
        /// </summary>
        /// <param name="item">El ítem de trabajo a distribuir.</param>
        /// <returns>El resultado de la asignación.</returns>
        Task<AssignmentResult> AssignItemAsync(WorkItem item);
    }
}