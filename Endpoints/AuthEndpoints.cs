using GameServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using GameServer.DTO.Auth;

namespace GameServer.Endpoints;

public static class AuthEndpoints
{
    public record ApiError(string Error);


    public static void ConfigureAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/signUp", SignUp).WithName("SignUp").Accepts<AuthDto.SignUpRequest>("application/json")
            .Produces<AuthDto.SignUpResponse>(201).Produces<ApiError>(400);
        app.MapPost("/api/auth/signIn", SignIn).WithName("SignIn").Accepts<AuthDto.SignInRequest>("application/json")
            .Produces<AuthDto.SignInResponse>(200).Produces<ApiError>(400);
    }

    private async static Task<IResult> SignUp(IAuthService authRepo,
        [FromBody] AuthDto.SignUpRequest model)
    {
        try
        {
            AuthDto.SignUpResponse signUpResponse = await authRepo.SignUp(model);

            if (signUpResponse == null || string.IsNullOrEmpty(signUpResponse.Email))
            {
                throw new Exception("Player was not created");
            }

            return Results.Ok(signUpResponse);
        }
        catch (Exception e)
        {
            return Results.BadRequest(new ApiError(e.Message));
        }
    }

    private async static Task<IResult> SignIn(IAuthService authRepo,
        [FromBody] AuthDto.SignInRequest model)
    {
        try
        {
            AuthDto.SignInResponse signInResponse = await authRepo.SignIn(model);

            if (signInResponse == null || string.IsNullOrEmpty(signInResponse.Email))
            {
                throw new Exception("Incorrect email or password");
            }

            return Results.Ok(signInResponse);
        }
        catch (Exception e)
        {
            return Results.BadRequest(new ApiError(e.Message));
        }
    }
}