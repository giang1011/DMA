using HW2.Data;
using HW2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DMAWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserDataService _userService;

        public AuthController(IConfiguration config, UserDataService userService)
        {
            _config = config;
            _userService = userService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserModel login)
        {
            var users = _userService.GetUsers();
            var user = users.FirstOrDefault(u => u.Username == login.Username);

            if (user == null)
                return BadRequest(new { message = "Người dùng không tồn tại" });

            if (user.RefreshToken == "CLEARED")
                return BadRequest(new { message = "Tài khoản đã bị khóa bởi quản trị viên" });

            if (user.Password != login.Password)
                return BadRequest(new { message = "Sai mật khẩu" });

            var tokens = GenerateTokens(user.Username);
            user.RefreshToken = tokens.RefreshToken;
            user.RefreshTokenExpiry = tokens.RefreshExpiry;
            _userService.SaveUsers(users);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                expiresAt = tokens.AccessExpiry
            });
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _userService.GetUsers()
                .Select(u => new
                {
                    u.Username,
                    u.Phone,
                    u.Verified,
                    u.RefreshToken,
                    u.RefreshTokenExpiry
                });
            return Ok(users);
        }

        [HttpPost("clear-token/{username}")]
        public IActionResult ClearToken(string username)
        {
            var users = _userService.GetUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            user.RefreshToken = "CLEARED";
            user.RefreshTokenExpiry = null;
            _userService.SaveUsers(users);

            return Ok(new { message = $"Token của {username} đã bị xóa." });
        }

        private (string AccessToken, DateTime AccessExpiry, string RefreshToken, DateTime RefreshExpiry) GenerateTokens(string username)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSection["Key"]);
            int accessMinutes = int.Parse(jwtSection["AccessTokenExpireMinutes"]);
            int refreshDays = int.Parse(jwtSection["RefreshTokenExpireDays"]);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
                Expires = DateTime.UtcNow.AddMinutes(accessMinutes),
                Issuer = jwtSection["Issuer"],
                Audience = jwtSection["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string accessToken = tokenHandler.WriteToken(token);

            string refreshToken = Guid.NewGuid().ToString();
            DateTime refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

            return (accessToken, tokenDescriptor.Expires ?? DateTime.UtcNow, refreshToken, refreshExpiry);
        }
    }
}
