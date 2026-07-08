using ItemsWorkService.Models;
using ItemsWorkService.Services;
using ItemsWorkService.Interfaces;
using ItemsWorkService.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ItemsWorkService.Controllers
{
    /// <summary>
    /// Controlador para la gestión de ítems de trabajo y su distribución.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // La ruta será: api/workitems
    public class WorkItemsController : ControllerBase
    {
        private readonly IDistributionService _distributionService;
        private readonly IWorkItemRepository _repository;

        /// <summary>
        /// Constructor del controlador.
        /// </summary>
        public WorkItemsController(IDistributionService distributionService, IWorkItemRepository repository)
        {
            _distributionService = distributionService;
            _repository = repository;
        }

        /// <summary>
        /// Obtener todos los ítems almacenados en el sistema.
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        /// <summary>
        /// Obtener un ítem específico por su ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id)
        {
            var item = _repository.GetById(id);
            if (item == null) return NotFound($"No se encontró un ítem con ID {id}");
            return Ok(item);
        }

        /// <summary>
        /// Obtener ítems asignados a un usuario en particular.
        /// </summary>
        [HttpGet("user/{username}")]
        public IActionResult GetByUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return BadRequest("username is required");
            var items = _repository.GetByUser(username);
            return Ok(items);
        }

        /// <summary>
        /// Obtener ítems filtrados por un estado específico.
        /// </summary>
        [HttpGet("status/{status}")]
        public IActionResult GetByStatus(ItemStatus status)
        {
            var items = _repository.GetByStatus(status);
            return Ok(items);
        }

        /// <summary>
        /// Endpoint para crear y distribuir automáticamente un ítem de trabajo.
        /// Considera reglas de saturación y prioridad por fecha de entrega.
        /// </summary>
        [HttpPost("distribute")]
        public async Task<IActionResult> CreateAndDistribute([FromBody] WorkItem newItem)
        {
            if (newItem == null || string.IsNullOrWhiteSpace(newItem.Title))
            {
                return BadRequest(new { error = "El título del ítem es requerido." });
            }

            if (newItem.DeliveryDate == default)
            {
                return BadRequest(new { error = "La fecha de entrega es requerida." });
            }

            try
            {
                if (newItem.Id == Guid.Empty) 
                    newItem.Id = Guid.NewGuid();

                newItem.Status = ItemStatus.Pending;

                // Llamar al servicio de distribución que evalúa prioridades y saturación
                AssignmentResult result = await _distributionService.AssignItemAsync(newItem);

                // Guardar el ítem asignado en el repositorio
                if (result != null && result.Item != null)
                {
                    _repository.Add(result.Item);
                }

                return Ok(new 
                { 
                    message = "Asignado exitosamente", 
                    result = result 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error en la distribución: {ex.Message}" });
            }
        }
    }
}