
namespace Models;

public class Summary {
    public int totalReadingsUploaded { get; set; }
    public int totalUniqueAccounts { get; set; }
    public int totalValidAccounts { get; set; }
    public int totalValidReadings  { get; set; }
    public int totalUnknownAccountReadings  { get; set; }
    public int totalInvalidReadings { get; set; }
    public List<Reading> validValueReadings { get; set; }
    public List<Reading> invalidValueReadings { get; set; }
    public List<Reading> unknownAccountReadings { get; set; }

}