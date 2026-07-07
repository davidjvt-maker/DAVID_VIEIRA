using Microsoft.AspNetCore.Mvc;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // La ruta será: api/users
    public class UsersController : ControllerBase
    {
        // Una lista fija que simula los usuarios existentes en el sistema
        private static readonly List<string> _existingUsers = new()
    {
        "usuario_juan",
        "usuario_maria",
        "usuario_pedro"
    };

        /// <summary>
        /// Endpoint para obtener la lista de usuarios disponibles para la distribución.
        /// </summary>
        [HttpGet]
        public IActionResult GetUsers()
        {
            // Retorna un código 200 OK con la lista de strings
            return Ok(_existingUsers);
        }
    }
}
