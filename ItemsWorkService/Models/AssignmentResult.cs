using ItemsWorkService.Enums;

namespace ItemsWorkService.Models
{
    /// <summary>
    /// Modelo de respuesta que detalla el resultado de la asignación de un ítem.
    /// </summary>
    public class AssignmentResult
    {
        /// <summary>
        /// El ítem de trabajo que fue asignado.
        /// </summary>
        public WorkItem? Item { get; set; }

        /// <summary>
        /// La razón por la que se eligió este usuario.
        /// </summary>
        public string AssignmentReason { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el usuario elegido está al borde de la saturación (3 ítems de alta prioridad).
        /// </summary>
        public bool IsNearingSaturation { get; set; }
    }
}