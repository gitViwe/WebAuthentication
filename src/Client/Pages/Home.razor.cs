using Client.Manager;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Shared;

namespace Client.Pages;

public partial class Home
{
    [Inject] public required WebAuthenticationManager WebAuthenticationManager { get; set; }
    [Inject] public required KanBanManager KanBanManager { get; set; }
	[Inject] public required IDialogService DialogService { get; set; }
    public UserData Model { get; set; } = new(string.Empty, string.Empty);

    private bool _processing = false;
    private string _userId = string.Empty;
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

        _userId = await WebAuthenticationManager.ProcessRegistrationAsync(registrationId: Guid.NewGuid().ToString(), Model.RegisteredUserName);

        _processing = false;
    }

    private async Task AuthenticateAsync()
    {
        _processing = true;

        _userId = await WebAuthenticationManager.ProcessAuthenticationAsync(authenticationId: Guid.NewGuid().ToString());

        _processing = false;

        await ShowDialog();

	}

    private async Task ShowDialog()
    {
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = true };

		var data = await KanBanManager.GetUserDetailAsKanBanDialogDataAsync(_userId);

        if (data is not null)
        {
            var parameters = new DialogParameters<KanBanDialog>
            {
                { x => x.Model, data }
            };

            var dialogReference = DialogService.Show<KanBanDialog>($"Welcome {data.UserName}", parameters, options);
            var result = await dialogReference.Result;

            if (false == result.Canceled)
            {
                await KanBanManager.UpdateKanBanDataAsync((KanBanDialogData)result.Data);
            } 
        }
    }
}

public class UserData(string RegisterUserName, string AuthenticateUserName)
{
    public string RegisteredUserName { get; set; } = RegisterUserName;
    public string AuthenticateUserName { get; set; } = AuthenticateUserName;
}


