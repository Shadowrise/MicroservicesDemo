namespace CommandService.Models;

public class Platform
{
  public required int Id { get; set; }
  public required int ExternalID { get; set; }
  public required string Name { get; set; }
  public required ICollection<Command> Commands { get; set; }
}
