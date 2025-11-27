using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PrimePro.Api.Controllers
{
 /// <summary>
 /// Controlador de prueba para endpoints protegidos.
 /// </summary>
 [ApiController]
 [Route("api/[controller]")]
 public class TestController : ControllerBase
 {
 /// <summary>
 /// Endpoint privado que requiere autorización.
 /// </summary>
 [HttpGet("privado")]
 [Authorize]
 public IActionResult Privado()
 {
 return Ok("Acceso autorizado");
 }
 }
}
