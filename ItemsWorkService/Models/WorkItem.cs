using ItemsWorkService.Enums;

namespace ItemsWorkService.Models
{
    /// <summary>
    /// Representa un ítem de trabajo dentro del sistema.
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// Identificador único del ítem.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Título descriptivo del ítem.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Fecha límite de entrega.
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// Nivel de relevancia o prioridad.
        /// </summary>
        public Relevance Relevance { get; set; }

        /// <summary>
        /// Estado actual del ítem.
        /// </summary>
        public ItemStatus Status { get; set; }

        /// <summary>
        /// Nombre del usuario al que fue asignado el ítem.
        /// </summary>
        public string AssignedUsername { get; set; } = string.Empty;
    }
}
