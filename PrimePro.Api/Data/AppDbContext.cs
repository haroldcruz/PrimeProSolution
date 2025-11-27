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
 public DbSet<Role> Roles { get; set; } = null!;
 public DbSet<UsuarioRole> UsuarioRoles { get; set; } = null!;

 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
 base.OnModelCreating(modelBuilder);

 modelBuilder.Entity<UsuarioRole>()
 .HasKey(ur => new { ur.UsuarioId, ur.RoleId });

 modelBuilder.Entity<UsuarioRole>()
 .HasOne(ur => ur.Usuario)
 .WithMany(u => u.UsuarioRoles)
 .HasForeignKey(ur => ur.UsuarioId);

 modelBuilder.Entity<UsuarioRole>()
 .HasOne(ur => ur.Role)
 .WithMany(r => r.UsuarioRoles)
 .HasForeignKey(ur => ur.RoleId);
 }
 }
}
