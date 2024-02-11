using Microsoft.AspNetCore.Mvc;
using WebAuthn.Net.Models.Protocol.Json.RegistrationCeremony.CreateCredential;
using WebAuthn.Net.Models.Protocol.Json.RegistrationCeremony.CreateOptions;

namespace API.Endpoint;

public static class RegistrationEndpoint
{
    public static IEndpointRouteBuilder MapRegistrationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/registration-options", GetRegistrationOptionsAsync)
            .WithName("Registration Options")
            .Produces<PublicKeyCredentialCreationOptionsJSON>()
            .WithOpenApi();

        app.MapPost("/complete-registration", CompleteRegistrationAsync)
            .WithName("Complete Registration")
            .Produces<string>()
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetRegistrationOptionsAsync(
        [FromServices] WebAuthentication auth,
        HttpContext context)
    {
        string userId = Utility.GenerateHexString(16);
        var options = await auth.GetRegistrationOptionsAsync(context, userId);

        return Results.Ok(options);
    }

    private static async Task<IResult> CompleteRegistrationAsync(
        [FromServices] WebAuthentication auth,
        HttpContext context,
        [FromBody] RegistrationResponseJSON json)
    {
        byte[] userHandle = await auth.CompleteRegistrationAsync(context, json);

        string userId = Utility.ByteArrayToString(userHandle);

        return Results.Ok(userId);
    }
}
