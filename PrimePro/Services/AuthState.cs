using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrimePro.Services
{
 /// <summary>
 /// Representa el estado de autenticación del cliente leyendo el token desde localStorage.
 /// </summary>
 public class AuthState
 {
 private readonly TokenStorageService _tokenStorage;

 /// <summary>
 /// Evento que se dispara cuando cambia el estado de autenticación.
 /// </summary>
 public event Action? OnChange;

 /// <summary>
 /// Indica si existe un token válido (no se valida criptográficamente aquí).
 /// </summary>
 public bool IsAuthenticated { get; private set; }

 /// <summary>
 /// Email obtenido del claim del token.
 /// </summary>
 public string? Email { get; private set; }

 /// <summary>
 /// Nombre obtenido del claim del token.
 /// </summary>
 public string? Name { get; private set; }

 public AuthState(TokenStorageService tokenStorage)
 {
 _tokenStorage = tokenStorage;
 }

 /// <summary>
 /// Actualiza el estado leyendo y decodificando el JWT desde localStorage.
 /// Dispara el evento OnChange al finalizar.
 /// </summary>
 public async Task UpdateAsync()
 {
 IsAuthenticated = false;
 Email = null;
 Name = null;

 var token = await _tokenStorage.GetTokenAsync();
 if (string.IsNullOrWhiteSpace(token))
 {
 NotifyStateChanged();
 return;
 }

 var parts = token.Split('.');
 if (parts.Length <2)
 {
 NotifyStateChanged();
 return;
 }

 var payload = parts[1];
 try
 {
 var json = DecodeBase64Url(payload);
 using var doc = JsonDocument.Parse(json);
 var root = doc.RootElement;

 // Try common claim names for email and name
 if (root.TryGetProperty("email", out var emailProp))
 Email = emailProp.GetString();
 else if (root.TryGetProperty("Email", out var emailProp2))
 Email = emailProp2.GetString();

 if (root.TryGetProperty("name", out var nameProp))
 Name = nameProp.GetString();
 else if (root.TryGetProperty("nombre", out var nombreProp))
 Name = nombreProp.GetString();
 else if (root.TryGetProperty("Nombre", out var nombreProp2))
 Name = nombreProp2.GetString();

 IsAuthenticated = true;
 }
 catch
 {
 // Silenciar errores de parseo
 IsAuthenticated = false;
 Email = null;
 Name = null;
 }

 NotifyStateChanged();
 }

 private void NotifyStateChanged()
 {
 try
 {
 OnChange?.Invoke();
 }
 catch
 {
 // Ignorar errores en manejadores
 }
 }

 private static string DecodeBase64Url(string input)
 {
 string s = input.Replace('-', '+').Replace('_', '/');
 switch (s.Length %4)
 {
 case 2:
 s += "==";
 break;
 case 3:
 s += "=";
 break;
 default:
 break;
 }

 var bytes = Convert.FromBase64String(s);
 return Encoding.UTF8.GetString(bytes);
 }
 }
}
