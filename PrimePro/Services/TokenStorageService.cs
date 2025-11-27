using Microsoft.JSInterop;

namespace PrimePro.Services
{
 /// <summary>
 /// Servicio para almacenar, leer y eliminar el token JWT en localStorage.
 /// </summary>
 public class TokenStorageService
 {
 private const string TokenKey = "authToken";
 private readonly IJSRuntime _js;

 public TokenStorageService(IJSRuntime js)
 {
 _js = js;
 }

 /// <summary>
 /// Guarda el token en localStorage.
 /// </summary>
 public ValueTask SaveTokenAsync(string token)
 {
 return _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
 }

 /// <summary>
 /// Obtiene el token desde localStorage.
 /// </summary>
 public ValueTask<string?> GetTokenAsync()
 {
 return _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
 }

 /// <summary>
 /// Elimina el token de localStorage.
 /// </summary>
 public ValueTask ClearTokenAsync()
 {
 return _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
 }
 }
}
