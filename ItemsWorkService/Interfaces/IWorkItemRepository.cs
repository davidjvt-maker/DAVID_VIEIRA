using ItemsWorkService.Models;
using ItemsWorkService.Enums;

namespace ItemsWorkService.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de ítems de trabajo.
    /// </summary>
    public interface IWorkItemRepository
    {
        /// <summary>
        /// Agrega un nuevo ítem al repositorio.
        /// </summary>
        void Add(WorkItem item);

        /// <summary>
        /// Obtiene todos los ítems.
        /// </summary>
        List<WorkItem> GetAll();

        /// <summary>
        /// Obtiene un ítem por su ID.
        /// </summary>
        WorkItem? GetById(Guid id);

        /// <summary>
        /// Obtiene los ítems asignados a un usuario.
        /// </summary>
        List<WorkItem> GetByUser(string username);

        /// <summary>
        /// Obtiene los ítems filtrados por un estado específico.
        /// </summary>
        List<WorkItem> GetByStatus(ItemStatus status);
    }
}
