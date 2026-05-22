namespace Entities.DTOs;

public class UserResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    public static UserResponse FromUser(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name
        };
    }
}