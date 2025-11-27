using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrimePro.Api.Data;
using PrimePro.Api.Models;

namespace PrimePro.Api.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class UsuariosController : ControllerBase
 {
 private readonly AppDbContext _db;
 private readonly ILogger<UsuariosController> _logger;

 public UsuariosController(AppDbContext db, ILogger<UsuariosController> logger)
 {
 _db = db;
 _logger = logger;
 }

 private static string ComputeSha256Hash(string raw)
 {
 raw = (raw ?? string.Empty).Trim();
 using var sha = SHA256.Create();
 var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
 return Convert.ToHexString(bytes);
 }

 // GET: api/usuarios
 [HttpGet]
 public async Task<IActionResult> GetAll()
 {
 var users = await _db.Usuarios
 .Select(u => new { u.Id, u.Nombre, u.Email })
 .ToListAsync();
 return Ok(users);
 }

 // GET: api/usuarios/{id}
 [HttpGet("{id:int}")]
 public async Task<IActionResult> GetById(int id)
 {
 var user = await _db.Usuarios.FindAsync(id);
 if (user == null) return NotFound();
 return Ok(new { user.Id, user.Nombre, user.Email });
 }

 public class CreateUsuarioRequest
 {
 public string Nombre { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string? Contraseña { get; set; }
 }

 // POST: api/usuarios
 [HttpPost]
 public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest req)
 {
 if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Email))
 return BadRequest(new { error = "Nombre y Email son requeridos." });

 var exists = await _db.Usuarios.AnyAsync(u => u.Email == req.Email);
 if (exists) return BadRequest(new { error = "Email ya en uso." });

 var user = new Usuario
 {
 Nombre = req.Nombre,
 Email = req.Email,
 ClaveHash = req.Contraseña != null ? ComputeSha256Hash(req.Contraseña) : string.Empty
 };

 _db.Usuarios.Add(user);
 await _db.SaveChangesAsync();

 return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { user.Id, user.Nombre, user.Email });
 }

 public class UpdateUsuarioRequest
 {
 public string Nombre { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string? Contraseña { get; set; }
 }

 // PUT: api/usuarios/{id}
 [HttpPut("{id:int}")]
 public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioRequest req)
 {
 var user = await _db.Usuarios.FindAsync(id);
 if (user == null) return NotFound();

 if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Email))
 return BadRequest(new { error = "Nombre y Email son requeridos." });

 // comprobar duplicado de email en otro usuario
 var other = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == req.Email && u.Id != id);
 if (other != null) return BadRequest(new { error = "Email ya en uso por otro usuario." });

 user.Nombre = req.Nombre;
 user.Email = req.Email;
 if (req.Contraseña != null)
 {
 user.ClaveHash = ComputeSha256Hash(req.Contraseña);
 }

 _db.Usuarios.Update(user);
 await _db.SaveChangesAsync();

 return NoContent();
 }

 // DELETE: api/usuarios/{id}
 [HttpDelete("{id:int}")]
 public async Task<IActionResult> Delete(int id)
 {
 var user = await _db.Usuarios.FindAsync(id);
 if (user == null) return NotFound();

 _db.Usuarios.Remove(user);
 await _db.SaveChangesAsync();

 return NoContent();
 }
 }
}
