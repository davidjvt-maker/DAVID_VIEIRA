using ItemsWorkService.Models;
using ItemsWorkService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItemsWorkService.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // La ruta será: api/workitems
    public class WorkItemsController : ControllerBase
    {
        private readonly IDistributionService _distributionService;

        // Usamos Inyección de Dependencias para traer el servicio de distribución
        public WorkItemsController(IDistributionService distributionService)
        {
            _distributionService = distributionService;
        }

        /// <summary>
        /// Endpoint para crear y distribuir automáticamente un ítem de trabajo.
        /// </summary>
        [HttpPost("distribute")]
        public async Task<IActionResult> CreateAndDistribute([FromBody] WorkItem newItem)
        {
            if (newItem == null || string.IsNullOrWhiteSpace(newItem.Title))
            {
                return BadRequest("Datos del ítem inválidos.");
            }

            try
            {
                if (newItem.Id == Guid.Empty) newItem.Id = Guid.NewGuid();
                newItem.Status = Enums.ItemStatus.Pending;

                // Ahora usamos await para la operación asíncrona
                WorkItem distributedItem = await _distributionService.AssignItemAsync(newItem);

                return Ok(new { Mensaje = "Asignado exitosamente", Item = distributedItem });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error en la distribución: {ex.Message}");
            }
        }
    }
}
