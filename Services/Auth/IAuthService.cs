using GameServer.DTO.Auth;

namespace GameServer.Services.Auth;

public interface IAuthService
{
    Task<AuthDto.SignUpResponse> SignUp(AuthDto.SignUpRequest signUpRequest);
    Task<AuthDto.SignInResponse> SignIn(AuthDto.SignInRequest signInRequest);
}