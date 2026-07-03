using Fundo.Application.DTOs;

namespace Fundo.Application.Interfaces;

public interface IAuthService
{
    AuthResult? Authenticate(LoginRequest request);
}
