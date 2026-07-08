using ItemsWorkService.Models;

namespace ItemsWorkService.Interfaces
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

        /// <summary>
        /// Determina si un usuario está saturado según la regla de negocio:
        /// un usuario con más de 3 ítems de alta relevancia pendientes se considera saturado
        /// y no debe recibir nuevas asignaciones.
        /// </summary>
        /// <param name="username">El nombre del usuario a evaluar.</param>
        /// <returns>True si el usuario está saturado, false en caso contrario.</returns>
        bool IsUserSaturated(string username);
    }
}