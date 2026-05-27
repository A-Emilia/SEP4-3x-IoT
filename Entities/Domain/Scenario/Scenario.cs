namespace Entities.Domain.Scenario;

public class Scenario
{
    public int Id { get; set; }
    public decimal PrefTemperature { get; set; }
    public decimal PrefHumidity { get; set; }
    public decimal ComfortScore { get; set; }
    public string Source { get; set; } = "";
    public bool Applied { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal CurrentTemperature { get; set; }
    public decimal CurrentHumidity { get; set; }
}