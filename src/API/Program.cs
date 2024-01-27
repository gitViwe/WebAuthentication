using API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAuthn.Net.Models.Protocol.Json.AuthenticationCeremony.VerifyAssertion;
using WebAuthn.Net.Models.Protocol.Json.RegistrationCeremony.CreateCredential;
using WebAuthn.Net.Storage.SqlServer.Configuration.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseSqlServer(Environment.GetEnvironmentVariable("MSSQL_CONNECTION_STRING")!);
});
builder.Services.AddWebAuthnSqlServer(
    configureSqlServer: sqlServer =>
    {
        sqlServer.ConnectionString = Environment.GetEnvironmentVariable("MSSQL_CONNECTION_STRING")!;
    });
builder.Services.AddMemoryCache();
builder.Services.AddTransient<WebAuthentication>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("https://localhost:7167;http://localhost:5136".Split(';'))
            .AllowCredentials();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.RunScript();

app.MapGet("/registration-options", async ([FromServices] WebAuthentication auth, HttpContext context) =>
{
    string userId = Utility.GenerateHexString(16);
    var options = await auth.GetRegistrationOptionsAsync(context, userId);

    return Results.Ok(options);
})
.WithName("Registration Options")
.WithOpenApi();

app.MapPost("/complete-registration", async ([FromServices] WebAuthentication auth, HttpContext context, [FromBody] RegistrationResponseJSON json) =>
{
    byte[] userHandle = await auth.CompleteRegistrationAsync(context, json);

    string userId = Utility.ByteArrayToString(userHandle);

    return Results.Ok(userId);
})
.WithName("Complete Registration")
.WithOpenApi();

app.MapGet("/authentication-options", async ([FromServices] WebAuthentication auth, HttpContext context) =>
{
    //string userId = Utility.GenerateHexString(16);
    var options = await auth.GetAuthenticationOptionsAsync(context, null);

    return Results.Ok(options);
})
.WithName("Authentication Options")
.WithOpenApi();

app.MapPost("/complete-authentication", async ([FromServices] WebAuthentication auth, HttpContext context, [FromBody] AuthenticationResponseJSON json) =>
{
    byte[] userHandle = await auth.CompleteAuthenticationAsync(context, json);

    string userId = Utility.ByteArrayToString(userHandle);

    return Results.Ok(userId);
})
.WithName("Complete Authentication")
.WithOpenApi();

app.Run();
