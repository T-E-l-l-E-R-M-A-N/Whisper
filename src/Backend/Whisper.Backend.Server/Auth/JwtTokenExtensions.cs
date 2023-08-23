using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Whisper.Backend.ChatModels;

namespace Whisper.Backend.Server;

public static class JwtTokenExtensions
{
    public static async Task<string> GenerateJwtToken(this MessengerUserModel user,
        TokenParameters tokenParams,
        RoleManager<IdentityRole> roleManager,
        UserManager<MessengerUserModel> userManager)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                
            new(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                
            new(ClaimTypes.NameIdentifier, user.Id),
        };

        var userRoles = await userManager.GetRolesAsync(user);

        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
            var role = await roleManager.FindByNameAsync(userRole);

            if (role != null)
            {
                var roleClaims = await roleManager.GetClaimsAsync(role);

                claims.AddRange(roleClaims);
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenParams.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
          
        var token = new JwtSecurityToken(
            tokenParams.Issuer,
            tokenParams.Audience,
            claims,
            expires: tokenParams.Expiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}