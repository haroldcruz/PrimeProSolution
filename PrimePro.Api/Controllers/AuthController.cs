using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PrimePro.Api.Data;
using PrimePro.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace PrimePro.Api.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class AuthController : ControllerBase
 {
 private readonly AppDbContext _db;
 private readonly IConfiguration _config;
 private readonly ILogger<AuthController> _logger;
 private readonly IWebHostEnvironment _env;

 public AuthController(AppDbContext db, IConfiguration config, ILogger<AuthController> logger, IWebHostEnvironment env)
 {
 _db = db;
 _config = config;
 _logger = logger;
 _env = env;
 }

 public class LoginRequest
 {
 public string Email { get; set; } = string.Empty;
 public string Contraseña { get; set; } = string.Empty;
 }

 public class RegisterRequest
 {
 public string Nombre { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string Contraseña { get; set; } = string.Empty;
 }

 private void AddCorsHeaders()
 {
 var origin = Request.Headers["Origin"].ToString();
 if (!string.IsNullOrWhiteSpace(origin))
 {
 Response.Headers["Access-Control-Allow-Origin"] = origin;
 }
 else
 {
 Response.Headers["Access-Control-Allow-Origin"] = "*";
 }
 Response.Headers["Access-Control-Allow-Credentials"] = "true";
 }

 /// <summary>
 /// Maneja preflight CORS para cualquier ruta bajo /api/auth.
 /// Esto agrega los encabezados necesarios para que los navegadores permitan las solicitudes desde el cliente de desarrollo.
 /// </summary>
 [HttpOptions("{*any}")]
 [AllowAnonymous]
 public IActionResult Options()
 {
 // Permitir el origen que hizo la petición si está presente; de lo contrario permitir cualquier origen.
 var origin = Request.Headers["Origin"].ToString();
 if (!string.IsNullOrWhiteSpace(origin))
 {
 Response.Headers["Access-Control-Allow-Origin"] = origin;
 }
 else
 {
 Response.Headers["Access-Control-Allow-Origin"] = "*";
 }

 Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS";
 Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
 Response.Headers["Access-Control-Allow-Credentials"] = "true";
 Response.Headers["Access-Control-Max-Age"] = "86400"; //1 day

 return NoContent();
 }

 /// <summary>
 /// Autentica un usuario y devuelve un token JWT.
 /// </summary>
 [HttpPost("login")]
 [Consumes("application/json")]
 public async Task<IActionResult> Login([FromBody] LoginRequest request)
 {
 AddCorsHeaders();
 _logger.LogInformation("Login attempt for {Email}", request.Email);
 var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
 if (user == null)
 {
 _logger.LogWarning("Login failed: user not found {Email}", request.Email);
 return Unauthorized();
 }

 // Verificar hash del lado servidor: la base guarda ClaveHash; comparamos hash(Contraseña)
 var hash = ComputeSha256Hash(request.Contraseña);
 if (user.ClaveHash != hash)
 {
 _logger.LogWarning("Login failed: invalid password for {Email}", request.Email);
 return Unauthorized();
 }

 var token = GenerateJwtToken(user);
 _logger.LogInformation("Login successful for {Email}", request.Email);
 return Ok(new { token });
 }

 /// <summary>
 /// Registra un nuevo usuario.
 /// Recibe { nombre, email, contraseña }. Calcula SHA256, evita duplicados por email y guarda Usuario.
 /// </summary>
 [HttpPost("register")]
 [Consumes("application/json")]
 public async Task<IActionResult> Register([FromBody] RegisterRequest request)
 {
 AddCorsHeaders();
 if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Contraseña) || string.IsNullOrWhiteSpace(request.Nombre))
 {
 return BadRequest(new { error = "Nombre, email y contraseña son requeridos." });
 }

 try
 {
 var exists = await _db.Usuarios.AnyAsync(u => u.Email == request.Email);
 if (exists)
 {
 return BadRequest(new { error = "El email ya está en uso." });
 }

 var user = new Usuario
 {
 Nombre = request.Nombre,
 Email = request.Email,
 ClaveHash = ComputeSha256Hash(request.Contraseña)
 };

 _db.Usuarios.Add(user);
 await _db.SaveChangesAsync();

 return Ok(new { message = "Usuario registrado correctamente." });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error registrando usuario {Email}", request.Email);
 return StatusCode(500, new { error = "Error interno al crear usuario." });
 }
 }

 /// <summary>
 /// Endpoint para poblar un usuario de prueba. SOLO disponible en Development.
 /// Crea usuario "harold@test.com" con contraseña "MiPass123!" si no existe.
 /// </summary>
 [HttpPost("seed")]
 public async Task<IActionResult> Seed()
 {
 AddCorsHeaders();
 if (!_env.IsDevelopment())
 {
 return Forbid();
 }

 var email = "harold@test.com";
 var defaultPassword = "MiPass123!";

 try
 {
 var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
 if (user != null)
 {
 return Ok(new { message = "Usuario ya existe." });
 }

 user = new Usuario
 {
 Nombre = "Harold",
 Email = email,
 ClaveHash = ComputeSha256Hash(defaultPassword)
 };

 _db.Usuarios.Add(user);
 await _db.SaveChangesAsync();

 return Ok(new { message = "Usuario seed creado.", email, password = defaultPassword });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error creando seed de usuario");
 return StatusCode(500, new { error = "Error interno al crear seed." });
 }
 }

 private string ComputeSha256Hash(string raw)
 {
 using var sha = SHA256.Create();
 var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
 return Convert.ToHexString(bytes);
 }

 private string GenerateJwtToken(Usuario user)
 {
 var key = _config["Jwt:Key"] ?? string.Empty;
 var issuer = _config["Jwt:Issuer"] ?? string.Empty;
 var audience = _config["Jwt:Audience"] ?? string.Empty;

 var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
 var cred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

 var claims = new[] {
 new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
 new Claim(JwtRegisteredClaimNames.Email, user.Email),
 new Claim("nombre", user.Nombre)
 };

 var token = new JwtSecurityToken(
 issuer: issuer,
 audience: audience,
 claims: claims,
 expires: DateTime.UtcNow.AddHours(1),
 signingCredentials: cred
 );

 return new JwtSecurityTokenHandler().WriteToken(token);
 }
 }
}
