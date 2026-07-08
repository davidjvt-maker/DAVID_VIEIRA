using ItemsWorkService.Models;
using ItemsWorkService.Enums;

namespace ItemsWorkService.Services
{
    /// <summary>
    /// Implementación en memoria del repositorio de ítems, segura para acceso concurrente básico.
    /// </summary>
    public class InMemoryWorkItemRepository : IWorkItemRepository
    {
        private readonly List<WorkItem> _items = new();
        private readonly object _lock = new();

        /// <summary>
        /// Agrega un nuevo ítem de trabajo a la memoria.
        /// </summary>
        public void Add(WorkItem item)
        {
            lock (_lock)
            {
                _items.Add(item);
            }
        }

        /// <summary>
        /// Retorna todos los ítems de trabajo.
        /// </summary>
        public List<WorkItem> GetAll()
        {
            lock (_lock)
            {
                return _items.ToList();
            }
        }

        /// <summary>
        /// Retorna un ítem específico por su ID.
        /// </summary>
        public WorkItem? GetById(Guid id)
        {
            lock (_lock)
            {
                return _items.FirstOrDefault(i => i.Id == id);
            }
        }

        /// <summary>
        /// Retorna todos los ítems asignados a un usuario específico.
        /// </summary>
        public List<WorkItem> GetByUser(string username)
        {
            lock (_lock)
            {
                return _items.Where(i => string.Equals(i.AssignedUsername, username, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        /// <summary>
        /// Retorna todos los ítems con un estado específico.
        /// </summary>
        public List<WorkItem> GetByStatus(ItemStatus status)
        {
            lock (_lock)
            {
                return _items.Where(i => i.Status == status).ToList();
            }
        }
    }
}