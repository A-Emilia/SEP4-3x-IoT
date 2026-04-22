namespace Entities;

public class Measurement
{
    public Guid Id { get; set; } = Guid.NewGuid(); //TODO MongoDB ID
    public DateTime TimestampUtc { get; set; }

    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public int Light { get; set; }
}