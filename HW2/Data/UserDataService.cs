using HW2.Models;
using System.Text.Json;

namespace HW2.Data
{
    public class UserDataService
    {
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");

        public List<UserModel> GetUsers()
        {
            if (!File.Exists(_filePath))
                return new List<UserModel>();

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<UserModel>>(json) ?? new List<UserModel>();
        }

        public void SaveUsers(List<UserModel> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
