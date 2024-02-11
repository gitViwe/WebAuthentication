﻿using gitViwe.Shared.Cache;
using Microsoft.Extensions.DependencyInjection;
using WebAuthn.Net.Models.Protocol.Enums;
using WebAuthn.Net.Models.Protocol.Json.AuthenticationCeremony.CreateOptions;
using WebAuthn.Net.Models.Protocol.Json.AuthenticationCeremony.VerifyAssertion;
using WebAuthn.Net.Models.Protocol.Json.RegistrationCeremony.CreateCredential;
using WebAuthn.Net.Models.Protocol.Json.RegistrationCeremony.CreateOptions;
using WebAuthn.Net.Models.Protocol.RegistrationCeremony.CreateOptions;
using WebAuthn.Net.Services.AuthenticationCeremony;
using WebAuthn.Net.Services.AuthenticationCeremony.Models.CreateOptions;
using WebAuthn.Net.Services.AuthenticationCeremony.Models.VerifyAssertion;
using WebAuthn.Net.Services.RegistrationCeremony;
using WebAuthn.Net.Services.RegistrationCeremony.Models.CreateCredential;
using WebAuthn.Net.Services.RegistrationCeremony.Models.CreateOptions;
using WebAuthn.Net.Services.Serialization.Cose.Models.Enums;

namespace API;

public class WebAuthentication
{
    public WebAuthentication(
        IServiceProvider provider,
        IRegistrationCeremonyService registrationCeremonyService,
        IAuthenticationCeremonyService authenticationCeremonyService)
    {
        _registrationCeremonyService = registrationCeremonyService;
        _authenticationCeremonyService = authenticationCeremonyService;

        using var scope = provider.CreateAsyncScope();
        _cache = scope.ServiceProvider.GetRequiredService<IRedisDistributedCache>();
        _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    }

    private readonly IRedisDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IRegistrationCeremonyService _registrationCeremonyService;
    private readonly IAuthenticationCeremonyService _authenticationCeremonyService;
    private const string WEBAUTHN_REGISTRATION_HEADER = "X-WebAuthn-Registration-Id";
    private const string WEBAUTHN_AUTHENTICATION_HEADER = "X-WebAuthn-Authentication-Id";

    public async Task<PublicKeyCredentialCreationOptionsJSON> GetRegistrationOptionsAsync(HttpContext context, string userId)
    {
        byte[] userIdBytes = Utility.StringToByteArray(userId);

        var result = await _registrationCeremonyService.BeginCeremonyAsync(
            httpContext: context,
            request: new BeginRegistrationCeremonyRequest(
                origins: new RegistrationCeremonyOriginParameters(allowedOrigins: _configuration["AllowedOrigins"]!.Split(';')),
                topOrigins: null,
                rpDisplayName: "Passkeys demonstration",
                user: new PublicKeyCredentialUserEntity(
                    name: "Demonstration user",
                    id: userIdBytes,
                    displayName: "User Display Name"),
                challengeSize: 32,
                pubKeyCredParams:
                [
                    CoseAlgorithm.ES256,
                    CoseAlgorithm.ES384,
                    CoseAlgorithm.ES512,
                    CoseAlgorithm.RS256,
                    CoseAlgorithm.RS384,
                    CoseAlgorithm.RS512,
                    CoseAlgorithm.PS256,
                    CoseAlgorithm.PS384,
                    CoseAlgorithm.PS512,
                    CoseAlgorithm.EdDSA
                ],
                timeout: 300_000,
                excludeCredentials: RegistrationCeremonyExcludeCredentials.AllExisting(),
                authenticatorSelection: new AuthenticatorSelectionCriteria(
                    authenticatorAttachment: null,
                    residentKey: ResidentKeyRequirement.Required,
                    requireResidentKey: true,
                    userVerification: UserVerificationRequirement.Required),
                hints: null,
                attestation: null,
                attestationFormats: null,
                extensions: null),
            cancellationToken: CancellationToken.None);

        string cacheKey = context.Request.Headers[WEBAUTHN_REGISTRATION_HEADER]!;
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        _cache.Set(cacheKey, result.RegistrationCeremonyId);

        return result.Options;
    }

    public async Task<byte[]> CompleteRegistrationAsync(HttpContext context, RegistrationResponseJSON responseJSON)
    {
        string cacheKey = context.Request.Headers[WEBAUTHN_REGISTRATION_HEADER]!;
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        string? registrationCeremonyId = await _cache.GetAsync(cacheKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationCeremonyId, nameof(registrationCeremonyId));

        var result = await _registrationCeremonyService.CompleteCeremonyAsync(
            httpContext: context,
            request: new CompleteRegistrationCeremonyRequest(
                registrationCeremonyId: registrationCeremonyId.Replace("\"", string.Empty),
                description: "Web Authentication Demonstration",
                response: responseJSON),
            cancellationToken: CancellationToken.None);

        ArgumentNullException.ThrowIfNull(result, "CompleteRegistrationCeremonyResult");

        return result.HasError ? [] : result.Ok.UserHandle;
    }

    public async Task<PublicKeyCredentialRequestOptionsJSON> GetAuthenticationOptionsAsync(HttpContext context)
    {
        var result = await _authenticationCeremonyService.BeginCeremonyAsync(
            httpContext: context,
            request: new BeginAuthenticationCeremonyRequest(
                origins: new AuthenticationCeremonyOriginParameters(allowedOrigins: _configuration["AllowedOrigins"]!.Split(';')),
                topOrigins: null,
                userHandle: null,
                challengeSize: 32,
                timeout: 300_000,
                allowCredentials: AuthenticationCeremonyIncludeCredentials.AllExisting(),
                userVerification: UserVerificationRequirement.Required,
                hints: null,
                attestation: null,
                attestationFormats: null,
                extensions: null),
            cancellationToken: CancellationToken.None);

        string cacheKey = context.Request.Headers[WEBAUTHN_AUTHENTICATION_HEADER]!;
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        _cache.Set(cacheKey, result.AuthenticationCeremonyId);

        return result.Options;
    }

    public async Task<byte[]> CompleteAuthenticationAsync(HttpContext context, AuthenticationResponseJSON responseJSON)
    {
        string cacheKey = context.Request.Headers[WEBAUTHN_AUTHENTICATION_HEADER]!;
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        string? authenticationCeremonyId = await _cache.GetAsync(cacheKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(authenticationCeremonyId, nameof(authenticationCeremonyId));

        var result = await _authenticationCeremonyService.CompleteCeremonyAsync(
            httpContext: context,
            request: new CompleteAuthenticationCeremonyRequest(
                authenticationCeremonyId: authenticationCeremonyId.Replace("\"", string.Empty),
                response: responseJSON),
            cancellationToken: CancellationToken.None);

        ArgumentNullException.ThrowIfNull(result, "CompleteAuthenticationCeremonyResult");

        return result.HasError ? [] : result.Ok.UserHandle;
    }
}
