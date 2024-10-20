
namespace Models;

public class Reading {
    public string ReadingId { get; set; }

    public long AccountId { get; set; }
    public string? MeterReadingDateTime { get; set; }   
    public long? MeterReadValue { get; set; }
}