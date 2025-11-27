using Microsoft.EntityFrameworkCore;
using PrimePro.Api.Models;

namespace PrimePro.Api.Data
{
 public class AppDbContext : DbContext
 {
 public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
 {
 }

 /// <summary>
 /// Usuarios de la aplicación.
 /// </summary>
 public DbSet<Usuario> Usuarios { get; set; } = null!;
 }
}
