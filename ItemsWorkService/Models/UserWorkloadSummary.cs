using ItemsWorkService.Enums;

namespace ItemsWorkService.Models
{
    /// <summary>
    /// Resumen de la carga de trabajo de un usuario, incluyendo la lógica de saturación.
    /// </summary>
    public class UserWorkloadSummary
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Lista de ítems asignados al usuario.
        /// </summary>
        public List<WorkItem> Items { get; set; } = new();

        /// <summary>
        /// Total de ítems asignados (completados o pendientes).
        /// </summary>
        public int TotalItems => Items.Count;

        /// <summary>
        /// Total de ítems en estado pendiente.
        /// </summary>
        public int PendingCount => Items.Count(i => i.Status == ItemStatus.Pending);

        /// <summary>
        /// Total de ítems de alta relevancia en estado pendiente.
        /// </summary>
        public int HighRelevanceCount => Items.Count(i => i.Relevance == Relevance.High && i.Status == ItemStatus.Pending);
        
        /// <summary>
        /// Indica si el usuario está saturado según la regla de negocio 
        /// (ningún usuario con más de 3 ítems altamente relevantes pendientes debe recibir asignaciones).
        /// </summary>
        public bool IsSaturated => HighRelevanceCount > 3;
    }
}
