using ItemsWorkService.Models;

namespace ItemsWorkService.Services
{
    public interface IDistributionService
    {
        /// <summary>
        /// Recibe un ítem de trabajo sin asignar, aplica las reglas de negocio 
        /// (fechas, relevancia, saturación) y lo devuelve con el 'AssignedUsername' establecido.
        /// </summary>
        /// <param name="item">El ítem de trabajo a distribuir.</param>
        /// <returns>El ítem de trabajo modificado con su asignación correspondiente.</returns>
        Task<WorkItem> AssignItemAsync(WorkItem item);
    }
}
