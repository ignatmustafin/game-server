namespace GameServer.DTO.Auth;

public class AuthDto
{
    public record SignUpRequest(string Name, string Email, string Password);
    public record SignUpResponse(int Id, string Email, string Name);
    
    public record SignInRequest(string Email, string Password);

    public record SignInResponse(int Id, string Email, string Name);
}