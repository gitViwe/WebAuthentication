using Client.Json.AuthenticationCeremony.CreateOptions;
using Client.Json.AuthenticationCeremony.VerifyAssertion;
using Client.Json.RegistrationCeremony.CreateCredential;
using Client.Json.RegistrationCeremony.CreateOptions;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Client.Pages;

public partial class Home
{
    bool _processing = false;
    string _registrationId = string.Empty;
    string _authenticationId = string.Empty;
    private IJSObjectReference module;
    public UserData Model { get; set; } = new(string.Empty, string.Empty);
    private HttpClient Client { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/web_authentication.js");
            ArgumentNullException.ThrowIfNull(module, nameof(module));
        }
    }

    private async Task RegisterAsync()
    {
        _processing = true;
        _registrationId = Guid.NewGuid().ToString();
        Client = Factory.CreateClient("API");

        var options = await GetRegistrationOptionsAsync();

        var response = await JS.InvokeAsync<RegistrationResponseJSON>("ProcessRegistration", [options]);

        string userId = await CompleteRegistrationAsync(response);

        Model.RegisteredUserName = userId;

        _processing = false;
    }

    private async Task AuthenticateAsync()
    {
        _processing = true;
        _authenticationId = Guid.NewGuid().ToString();
        Client = Factory.CreateClient("API");

        var options = await GetAuthenticationOptionsAsync();

        var response = await JS.InvokeAsync<AuthenticationResponseJSON>("ProcessAuthentication", [options]);

        string userId = await CompleteAuthenticationAsync(response);

        Model.AuthenticateUserName = userId;

        _processing = false;
    }

    private async Task<PublicKeyCredentialCreationOptionsJSON> GetRegistrationOptionsAsync()
    {
        Client.DefaultRequestHeaders.Add("X-WebAuthn-Registration-Id", _registrationId);
        var options = await Client.GetFromJsonAsync<PublicKeyCredentialCreationOptionsJSON>("registration-options", CancellationToken.None);
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        return options;
    }

    private async Task<string> CompleteRegistrationAsync(RegistrationResponseJSON json)
    {
        var responseMessage = await Client.PostAsJsonAsync("complete-registration", json, CancellationToken.None);
        string userId = await responseMessage.Content.ReadAsStringAsync();
        return userId;
    }

    private async Task<PublicKeyCredentialRequestOptionsJSON> GetAuthenticationOptionsAsync()
    {
        Client.DefaultRequestHeaders.Add("X-WebAuthn-Authentication-Id", _authenticationId);
        var options = await Client.GetFromJsonAsync<PublicKeyCredentialRequestOptionsJSON>("authentication-options", CancellationToken.None);
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        return options;
    }

    private async Task<string> CompleteAuthenticationAsync(AuthenticationResponseJSON json)
    {
        var responseMessage = await Client.PostAsJsonAsync("complete-authentication", json, CancellationToken.None);
        string userId = await responseMessage.Content.ReadAsStringAsync();
        return userId;
    }
}

public class UserData(string RegisterUserName, string AuthenticateUserName)
{
    public string RegisteredUserName { get; set; } = RegisterUserName;
    public string AuthenticateUserName { get; set; } = AuthenticateUserName;
}
