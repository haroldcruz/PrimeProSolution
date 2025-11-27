using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PrimePro.Services
{
 /// <summary>
 /// Message handler que añade el token JWT en la cabecera Authorization cuando existe.
 /// </summary>
 public class AuthMessageHandler : DelegatingHandler
 {
 private readonly TokenStorageService _tokenStorage;

 public AuthMessageHandler(TokenStorageService tokenStorage)
 {
 _tokenStorage = tokenStorage;
 }

 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
 {
 var token = await _tokenStorage.GetTokenAsync();
 if (!string.IsNullOrWhiteSpace(token))
 {
 // Añadir cabecera Authorization: Bearer <token>
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
 }

 return await base.SendAsync(request, cancellationToken);
 }
 }
}
