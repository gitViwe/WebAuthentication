namespace Shared;

public class KanBanDialogData
{
	public int Id { get; set; }
	public string UserName { get; set; } = string.Empty;
	public IEnumerable<KanBanSectionDTO> KanBanSections { get; set; } = Enumerable.Empty<KanBanSectionDTO>();
	public IEnumerable<KanBanTaskItemDTO> KanBanTaskItems { get; set; } = Enumerable.Empty<KanBanTaskItemDTO>();
}
