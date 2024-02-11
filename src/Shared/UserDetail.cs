namespace Shared;

public class UserDetail
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ICollection<KanBanSection> KanBanSections { get; set; } = new HashSet<KanBanSection>();
}
