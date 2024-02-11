namespace Shared;

public class KanBanDialogData
{
	public string Id { get; set; } = string.Empty;
	public string UserName { get; set; } = string.Empty;
	public IEnumerable<KanBanSection> KanBanSections { get; set; } = Enumerable.Empty<KanBanSection>();
}
