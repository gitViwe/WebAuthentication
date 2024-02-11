using Client.Manager;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Shared;

namespace Client.Pages;

public partial class Home
{
    [Inject] public required WebAuthenticationManager WebAuthenticationManager { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }
    public UserData Model { get; set; } = new(string.Empty, string.Empty);

    private bool _processing = false;
    private IJSObjectReference module = default!;

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

        string userId = await WebAuthenticationManager.ProcessRegistrationAsync(registrationId: Guid.NewGuid().ToString());

        Model.RegisteredUserName = userId;

        _processing = false;
    }

    private async Task AuthenticateAsync()
    {
        _processing = true;

        string userId = await WebAuthenticationManager.ProcessAuthenticationAsync(authenticationId: Guid.NewGuid().ToString());

        Model.AuthenticateUserName = userId;

        _processing = false;
    }

    private async Task Show()
    {
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = true };
        var parameters = new DialogParameters<KanBanDialog>
		{
			{ x => x.Model, new KanBanDialogData() }
		};

        var dialogReference = DialogService.Show<KanBanDialog>("Welcom", parameters, options);
    }
}

public class UserData(string RegisterUserName, string AuthenticateUserName)
{
    public string RegisteredUserName { get; set; } = RegisterUserName;
    public string AuthenticateUserName { get; set; } = AuthenticateUserName;
}


