using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PrimePro.Services
{
 /// <summary>
 /// Servicio para autenticación contra la API.
 /// </summary>
 public class AuthService
 {
 private readonly HttpClient _http;

 public AuthService(IHttpClientFactory httpFactory)
 {
 // Use named client "ApiClient" so AuthMessageHandler attaches token.
 _http = httpFactory.CreateClient("ApiClient");
 }

 /// <summary>
 /// Intenta iniciar sesión y devuelve el token JWT como string.
 /// Envía JSON con las claves exactas: "email" y "contraseña".
 /// </summary>
 public async Task<string?> Login(string email, string password)
 {
 var payload = new Dictionary<string, string>
 {
 ["email"] = email,
 ["contraseña"] = password
 };

 var response = await _http.PostAsJsonAsync("api/auth/login", payload);
 if (!response.IsSuccessStatusCode) return null;
 var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
 return content?.Token;
 }

 /// <summary>
 /// Registra un nuevo usuario en la API.
 /// Envía JSON con las claves: "nombre", "email", "contraseña".
 /// </summary>
 public async Task<bool> Register(string nombre, string email, string password)
 {
 var payload = new Dictionary<string, string>
 {
 ["nombre"] = nombre,
 ["email"] = email,
 ["contraseña"] = password
 };

 var response = await _http.PostAsJsonAsync("api/auth/register", payload);
 return response.IsSuccessStatusCode;
 }

 private class LoginResponse
 {
 public string? Token { get; set; }
 }
 }
}
