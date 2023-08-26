using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Whisper.Backend.ChatModels;
using Whisper.Shared.Protos;

namespace Whisper.Backend.Server;

[Authorize]
public class AccountService : Account.AccountBase
{
    #region Private Fields

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<MessengerUserModel> _userManager;
    private readonly TokenParameters _tokenParameters;

    #endregion

    #region Constructor

    public AccountService(
        RoleManager<IdentityRole> roleManager,
        UserManager<MessengerUserModel> userManager,
        TokenParameters tokenParameters)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _tokenParameters = tokenParameters;
    }

    #endregion
    
    [AllowAnonymous]
    public override async Task<LoginResultResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByNameAsync(request.Login);

        if (user == null)
            return ErrorResponse("User not found");

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isValidPassword)
            return ErrorResponse("Password wrong");

        var response =
            TokenResponse(await user.GenerateJwtToken(_tokenParameters, _roleManager, _userManager));

        response.User.User = new MessengerUser()
        {
            Id = user.LongId,
            Name = user.ScreenName,
            Online = true
        };
        
        return response;
    }

    [AllowAnonymous]
    public override async Task<LoginResultResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.LoginParams.Login))
            return ErrorResponse("Login is not valid");

        MessengerUserModel user = new()
        {
            LongId = Random.Shared.NextInt64(),
            UserName = request.LoginParams.Login,
            ScreenName = request.UserName
        };

        var result = await _userManager.CreateAsync(user, request.LoginParams.Password);
            
        if (result.Succeeded)
        {
            var userIdentity = await _userManager.FindByNameAsync(user.UserName);

            var response =
                TokenResponse(await userIdentity.GenerateJwtToken(_tokenParameters, _roleManager, _userManager));

            response.User.User = new MessengerUser()
            {
                Id = userIdentity.LongId,
                Name = userIdentity.ScreenName,
                Online = true
            };
            
            return response;
        }

        return new()
        {
            Error = new ErrorResponse()
            {
                Message = result.Errors.FirstOrDefault()?.Description
            }
        };
    }

    public override async Task<UserInfoResponse> GetUserInfo(LoginByTokenRequest request, ServerCallContext context)
    {
        var user = await _userManager.GetUserAsync(context.GetHttpContext().User);

        if (user == null)
            return new UserInfoResponse()
            {
                Error = new ErrorResponse()
                {
                    Message = "No access"
                }
            };

        return new UserInfoResponse
        {
            Token = request.Token,
            User = new MessengerUser()
            {
                Id = user.LongId,
                Name = user.ScreenName,
                Online = true
            }
        };
    }

    private LoginResultResponse ErrorResponse(string errorMessage) =>
        new()
        {
            Error = new ErrorResponse()
            {
                Message = errorMessage
            }
        };

    private LoginResultResponse TokenResponse(string token) =>
        new()
        {
            User = new UserInfoResponse()
            {
                Token = token
            }
        };
}