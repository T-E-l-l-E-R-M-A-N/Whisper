using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Whisper.Frontend.Avalonia.Core.Database;
using Whisper.Frontend.Shared.Core;
using Whisper.Shared.Protos;

namespace Whisper.Frontend.Avalonia.Core;

public sealed class MessengerApiHelper
{
    private readonly FrontendDbContext _frontendDbContext;
    
    private Account.AccountClient _accountClient;
    private Messenger.MessengerClient _messengerClient;
    private string _baseUri;
    private string _token;

    public UserViewModel CurrentUser { get; set; }

    public MessengerApiHelper(FrontendDbContext frontendDbContext)
    {
        _frontendDbContext = frontendDbContext;
    }

    public async Task Init()
    {
        var baseUriItem = await _frontendDbContext.Items.FirstOrDefaultAsync(x=>x.Name == "BaseUri");

        if (baseUriItem == null)
        {
            await _frontendDbContext.Items.AddAsync(new (){ Name = "BaseUri", Value = "https://localhost:7068"});
            await _frontendDbContext.SaveChangesAsync();
        }
        
        _baseUri = baseUriItem.Value;

        _accountClient = GetAccountClient(_baseUri);
    }

    public async Task<bool> CheckTokenAndLogin()
    {
        var token = await _frontendDbContext.Items.FirstOrDefaultAsync(x=>x.Name == "Token");

        if (token == null)
            return false;

        var valid = await IsTokenValid(token.Value);

        if (valid)
        {
            var state = Jwt.GetStateFromJwt(token.Value);
            var user = state.User;

            if (user.Identity is {IsAuthenticated: false })
            {
                return false;
            }
            
            return true;
        }
        
        return false;
    }
    public async Task<bool> AuthorizeAsync(string[] strings)
    {
        LoginResultResponse loginResponse = null!;
        if (strings.Length == 2)
        {
            loginResponse = await _accountClient.LoginAsync(new LoginRequest()
            {
                Login = strings[0],
                Password = strings[1]
            });
        }
        else
        {
            loginResponse = await _accountClient.RegisterAsync(new RegisterRequest()
            {
                UserName = strings[0],
                LoginParams = new LoginRequest()
                {
                    Login = strings[1],
                    Password = strings[2]
                }
            });
        }

        if (loginResponse.ResultCase == LoginResultResponse.ResultOneofCase.Error)
        {
            return false;
        }

        var userInfo = loginResponse.User;

        _token = userInfo.Token;

        await _frontendDbContext.Items.AddAsync(new ()
        {
            Name = "AccessToken", 
            Value = _token
        });
        await _frontendDbContext.SaveChangesAsync();

        CurrentUser = new UserViewModel()
        {
            Name = userInfo.User.Name,
            Online = userInfo.User.Online
        };
        
        return true;
    }
    
    
    private Messenger.MessengerClient GetMessengerClientClient(string baseUri, string token)
        => new(GetAuthChannel(baseUri, token));

    private GrpcChannel GetAuthChannel(string baseUri, string token) =>
        new HttpClient().ToAuthChannel(baseUri, token);
    
    private Account.AccountClient GetAccountClient(string baseUri)
    {
        var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler()
        });

        return new Account.AccountClient(channel);
    }
    
    private async Task<bool> IsTokenValid(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var authUser =
                await _accountClient.GetUserInfoAsync(new LoginByTokenRequest()
                {
                    Token = token
                });

            if (authUser.User is { } user)
            {
                _token = token;
                CurrentUser = new UserViewModel()
                {
                    Name = user.Name,
                    Online = user.Online
                };
                return true;
            }
                
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }
}