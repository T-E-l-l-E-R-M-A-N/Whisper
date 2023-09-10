using System.Runtime.InteropServices;
using Grpc.Core;
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
    public long UserId { get; set; }

    public event EventHandler<MessageViewModel> NewMessageSent;
    public MessengerApiHelper(FrontendDbContext frontendDbContext)
    {
        _frontendDbContext = frontendDbContext;
    }

    public async Task Init()
    {
        //var baseUriItem = await _frontendDbContext.Items.FirstOrDefaultAsync(x=>x.Name == "BaseUri");
        //
        //if (baseUriItem == null)
        //{
        //    await _frontendDbContext.Items.AddAsync(new (){ Name = "BaseUri", Value = "https://localhost:7068"});
        //    await _frontendDbContext.SaveChangesAsync();
        //    
        //    baseUriItem = await _frontendDbContext.Items.FirstOrDefaultAsync(x=>x.Name == "BaseUri");
        //}
        //
        //_baseUri = baseUriItem.Value;

        _baseUri = "https://localhost:7068";
    }

    public async Task<bool> CheckTokenAndLogin()
    {
        //var token = await _frontendDbContext.Items.FirstOrDefaultAsync(x => x.Name == "AccessToken");

        //if (token == null)
        //    return false;

        //_accountClient = GetAccountClientByToken(token.Value);

        //var valid = await IsTokenValid(token.Value);

        //if (valid)
        //{
        //    var state = Jwt.GetStateFromJwt(token.Value);
        //    var user = state.User;

        //    if (user.Identity is {IsAuthenticated: false })
        //    {
        //        return false;
        //    }

        //    _token = token.Value;
        //    _messengerClient = GetMessengerClient(_baseUri, token.Value);
        //    _messengerClient.ConnectAsync(new ConnectRequest()
        //    {
        //        UserId = UserId
        //    });
        //    return true;
        //}
        
        return false;
    }
    public async Task<bool> AuthorizeAsync(string[] strings)
    {
        _accountClient = GetAccountClient(_baseUri);

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

        //await _frontendDbContext.Items.AddAsync(new ()
        //{
        //    Name = "AccessToken", 
        //    Value = _token
        //});
        //await _frontendDbContext.SaveChangesAsync();

        CurrentUser = new UserViewModel()
        {
            Id = userInfo.User.Id,
            Name = userInfo.User.Name,
            Online = userInfo.User.Online
        };

        UserId = userInfo.User.Id;

        _messengerClient = GetMessengerClient(_baseUri, userInfo.Token);
        _messengerClient.ConnectAsync(new ConnectRequest()
        {
            UserId = UserId
        });

        return true;
    }

    public async Task SendMessageAsync(long targetId, string content)
    {
        if (string.IsNullOrEmpty(content))
            return;

        var msg = new MessengerMessage()
        {
            Id = Random.Shared.NextInt64(),
            Message = content,
            TargetId = targetId,
            SenderId = UserId
        };
        await _messengerClient.SendToAsync(new SendRequest()
        {
            Message = msg
        });

        var sentMsg = await _messengerClient.GetMessageAsync(new GetMessageRequest()
        {
            Id = msg.Id,
        });

        NewMessageSent?.Invoke(this, new MessageViewModel()
        {
            Id = sentMsg.Id,
            Sender = CurrentUser.Name,
            Text = sentMsg.Message,
            RoomId = sentMsg.RoomId
        });
    }

    public async Task<UserViewModel> GetUserByIdAsync(long id)
    {
        var user = await _messengerClient.GetUserAsync(new GetUserRequest()
        {
            UserId = id
        });

        UserViewModel userViewModel = new UserViewModel()
        {
            Id = user.Id,
            Name = user.Name,
            Online = user.Online
        };

        return userViewModel;
    }

    public async Task DisconnectAsync()
    {
        if (_messengerClient != null)
            await _messengerClient.DisconnectAsync(new ConnectRequest()
            {
                UserId = UserId
            });
    }

    public async Task<IEnumerable<UserViewModel>> GetUsersAsync()
    {
        List<UserViewModel> users = new();

        var response = _messengerClient.GetUsers(new ConnectRequest()
        {
            UserId = UserId
        });

        var stream = response.ResponseStream;

        await foreach (var usr in stream.ReadAllAsync())
        {
            users.Add(new UserViewModel()
            {
                Id = usr.Id,
                Name = usr.Name,
                Online = usr.Online
            });
        }

        return users;
    }

    public async Task<IEnumerable<RoomViewModel>> GetRoomsAsync()
    {
        List<RoomViewModel> rooms = new();

        var response = _messengerClient.GetRooms(new ConnectRequest()
        {
            UserId = UserId
        });

        var stream = response.ResponseStream;

        await foreach (var room in stream.ReadAllAsync())
        {
            rooms.Add(new RoomViewModel()
            {
                Id = room.Id,
                Name = room.Name

            });
        }

        return rooms;
    }

    public async Task<IEnumerable<MessageViewModel>> GetMessagesAsync(long roomId)
    {
        List<MessageViewModel> messages = new();

        var response = _messengerClient.GetMessages(new OpenRoomRequest()
        {
            RoomId = roomId
        });

        var stream = response.ResponseStream;

        await foreach (var msg in stream.ReadAllAsync())
        {
            messages.Add(new MessageViewModel()
            {
                Id = msg.Id,
                Text = msg.Message,
                Sender = (await GetUserByIdAsync(msg.SenderId)).Name,
                RoomId = msg.RoomId

            });
        }

        return messages;
    }

    public AsyncServerStreamingCall<MessengerMessage> GetMessagesDirect(OpenRoomRequest openRoomRequest) =>
        _messengerClient.GetMessages(openRoomRequest);
    private Messenger.MessengerClient GetMessengerClient(string baseUri, string token)
        => new(GetAuthChannel(baseUri, token));

    private GrpcChannel GetAuthChannel(string baseUri, string token) =>
        new HttpClient(new SocketsHttpHandler()).ToAuthChannel(baseUri, token);
    
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
                    Id = user.Id,
                    Name = user.Name,
                    Online = user.Online
                };
                UserId = authUser.User.Id;

                return true;
            }
                
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }

    private Account.AccountClient GetAccountClientByToken(string token)
    {
        var channel = GetAuthChannel(_baseUri, token);
        return new Account.AccountClient(channel);
    }

    private GrpcChannel GetAuthChannel(string token) =>
        new HttpClient(new SocketsHttpHandler())
            .ToAuthChannel(_baseUri, token);
}