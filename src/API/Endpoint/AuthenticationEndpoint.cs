using Microsoft.AspNetCore.Mvc;
using WebAuthn.Net.Models.Protocol.Json.AuthenticationCeremony.CreateOptions;
using WebAuthn.Net.Models.Protocol.Json.AuthenticationCeremony.VerifyAssertion;

namespace API.Endpoint;

public static class AuthenticationEndpoint
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/authentication-options", GetAuthenticationOptionsAsync)
            .WithName("Authentication Options")
            .Produces<PublicKeyCredentialRequestOptionsJSON>()
            .WithOpenApi();

        app.MapPost("/complete-authentication", CompleteAuthenticationAsync)
            .WithName("Complete Authentication")
            .Produces<string>()
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetAuthenticationOptionsAsync(
        [FromServices] WebAuthentication auth,
        HttpContext context)
    {
        var options = await auth.GetAuthenticationOptionsAsync(context);

        return Results.Ok(options);
    }

    private static async Task<IResult> CompleteAuthenticationAsync(
        [FromServices] WebAuthentication auth,
        HttpContext context,
        [FromBody] AuthenticationResponseJSON json)
    {
        byte[] userHandle = await auth.CompleteAuthenticationAsync(context, json);

        string userId = Utility.ByteArrayToString(userHandle);

        return Results.Ok(userId);
    }
}
