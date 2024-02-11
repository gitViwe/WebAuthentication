namespace Shared;

public class KanBanSection
{
    public int Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public bool NewTaskOpen { get; set; }
    public string NewTaskName { get; set; } = string.Empty;
    public UserDetail UserDetail { get; set; } = new();
    public ICollection<KanBanTaskItem> KanBanTaskItems { get; set; } = new HashSet<KanBanTaskItem>();
}
