namespace PrimePro.Api.Models
{
 /// <summary>
 /// Representa un usuario del sistema.
 /// </summary>
 public class Usuario
 {
 /// <summary>
 /// Clave primaria del usuario.
 /// </summary>
 public int Id { get; set; }

 /// <summary>
 /// Nombre completo del usuario.
 /// </summary>
 public string Nombre { get; set; } = string.Empty;

 /// <summary>
 /// Correo electrónico del usuario.
 /// </summary>
 public string Email { get; set; } = string.Empty;

 /// <summary>
 /// Hash de la contraseña.
 /// </summary>
 public string ClaveHash { get; set; } = string.Empty;
 }
}
