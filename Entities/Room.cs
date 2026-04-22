namespace Entities;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid(); //TODO PostgreSQL ID
    public string Name { get; set; }
}