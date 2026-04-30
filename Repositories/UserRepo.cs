using System.Text.Json;
using Entities;

namespace Repositories;

public class UserRepo
{
    private readonly string _filePath = "users.json";
    private readonly object _lock = new();

    public UserRepo()
    {
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    public List<User> GetAll()
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }
    }

    public User? GetById(string id)
    {
        return GetAll()
            .FirstOrDefault(user => user.Id.ToString().Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(User user)
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_filePath);
            var users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();

            if (users.Any(x => Equals(user.Id, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("User already exists.");
            }

            users.Add(user);

            var newJson = JsonSerializer.Serialize(users, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, newJson);
        }
    }
}