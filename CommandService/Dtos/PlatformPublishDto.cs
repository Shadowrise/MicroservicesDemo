namespace CommandService.Dtos;

public class PlatformPublishDto : GenericEventDto
{
  public int Id { get; set; }
  public string Name { get; set; }
}