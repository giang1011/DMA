namespace HW6.Services
{
    public class TokenStorage
    {
        public Dictionary<string, (string RefreshToken, DateTime Expiry)> Tokens { get; set; } = new();
    }

}
