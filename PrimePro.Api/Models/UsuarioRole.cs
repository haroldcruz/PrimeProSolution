namespace PrimePro.Api.Models
{
 public class UsuarioRole
 {
 public int UsuarioId { get; set; }
 public Usuario Usuario { get; set; } = null!;
 public int RoleId { get; set; }
 public Role Role { get; set; } = null!;
 }
}