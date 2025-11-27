namespace PrimePro.Api.Models
{
 public class Role
 {
 public int Id { get; set; }
 public string Name { get; set; } = string.Empty;

 public ICollection<UsuarioRole> UsuarioRoles { get; set; } = new List<UsuarioRole>();
 }
}