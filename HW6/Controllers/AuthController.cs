using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HW6.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly TokenStorage _tokenStorage;

    public AuthController(IConfiguration config, TokenStorage tokenStorage)
    {
        _config = config;
        _tokenStorage = tokenStorage;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        if (model.Username == "Test" && model.Password == "1")
        {
            var accessToken = GenerateToken(model.Username, 1); // 1 phút
            var refreshToken = GenerateToken(model.Username, 525600); // 1 năm (525600 phút)

            _tokenStorage.Tokens[model.Username] = (refreshToken, DateTime.UtcNow.AddYears(1));

            return Ok(new
            {
                accessToken,
                refreshToken
            });
        }

        return Unauthorized("Sai username hoặc password");
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        if (_tokenStorage.Tokens.TryGetValue(request.Username, out var tokenInfo))
        {
            if (tokenInfo.RefreshToken == request.RefreshToken && tokenInfo.Expiry > DateTime.UtcNow)
            {
                var newAccessToken = GenerateToken(request.Username, 1); // new access token 1 phút
                return Ok(new { accessToken = newAccessToken });
            }
        }
        return Unauthorized("Refresh token không hợp lệ hoặc hết hạn");
    }

    private string GenerateToken(string username, int expireMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RefreshRequest
{
    public string Username { get; set; }
    public string RefreshToken { get; set; }
}
