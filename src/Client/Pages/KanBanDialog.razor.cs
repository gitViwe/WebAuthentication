using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Shared;

namespace Client.Pages;

public partial class KanBanDialog
{
	[CascadingParameter] MudDialogInstance MudDialog { get; set; }
	[Parameter] public KanBanDialogData Model { get; set; } = new();

	void Submit()
	{
		if (_sections.Count != 0)
		{
			Model.KanBanSections = _sections.Select(section => new KanBanSection
			{
				Name = section.Name,
				NewTaskName = section.NewTaskName,
				NewTaskOpen = section.NewTaskOpen,
				KanBanTaskItems = section.KanBanTaskItems.Where(item => item.Status.Equals(section.Name))
												   .Select(item => new KanBanTaskItem { Name = item.Name, Status =  item.Status })
												   .ToList()
			});
		}
		
		MudDialog.Close(Model);
	}

	void Cancel() => MudDialog.Cancel();

	private MudDropContainer<KanBanTaskItem> _dropContainer;

	private bool _addSectionOpen;
	/* handling board events */
	private void TaskUpdated(MudItemDropInfo<KanBanTaskItem> info)
	{
		info.Item.Status = info.DropzoneIdentifier;
	}

	/* Setup for board  */
	private List<KanBanSection> _sections = [];

	private List<KanBanTaskItem> _tasks = [];

	KanBanNewForm newSectionModel = new();

	public class KanBanNewForm
	{
		[System.ComponentModel.DataAnnotations.Required]
		[System.ComponentModel.DataAnnotations.StringLength(10, ErrorMessage = "Name length can't be more than 10.")]
		public string Name { get; set; } = string.Empty;
	}

	protected override Task OnInitializedAsync()
	{
		_sections = Model.KanBanSections.Any()
			? Model.KanBanSections.ToList()
			: [
				new() { Name = "To Do", NewTaskOpen = false, NewTaskName = string.Empty },
				new() { Name = "In Process", NewTaskOpen = false, NewTaskName = string.Empty },
				new() { Name = "Done", NewTaskOpen = false, NewTaskName = string.Empty },
			];

		_tasks = Model.KanBanSections.SelectMany(x => x.KanBanTaskItems).Any()
			? Model.KanBanSections.SelectMany(x => x.KanBanTaskItems).ToList()
			: [
				new() { Name = "Write unit test", Status = "To Do" },
				new() { Name = "Some docu stuff", Status = "To Do" },
				new() { Name = "Walking the dog", Status = "To Do" },
			];

		return Task.CompletedTask;
	}

	private void OnValidSectionSubmit(EditContext context)
	{
		_sections.Add(new() { Name = newSectionModel.Name, NewTaskOpen = false, NewTaskName = string.Empty });
		newSectionModel.Name = string.Empty;
		_addSectionOpen = false;
	}

	private void OpenAddNewSection()
	{
		_addSectionOpen = true;
	}

	private void AddTask(KanBanSection section)
	{
		_tasks.Add(new() { Name = section.NewTaskName, Status = section.Name });
		section.NewTaskName = string.Empty;
		section.NewTaskOpen = false;
		_dropContainer.Refresh();
	}

	private void DeleteSection(KanBanSection section)
	{
		if (_sections.Count == 1)
		{
			_tasks.Clear();
			_sections.Clear();
		}
		else
		{
			int newIndex = _sections.IndexOf(section) - 1;
			if (newIndex < 0)
			{
				newIndex = 0;
			}

			_sections.Remove(section);

			var tasks = _tasks.Where(x => x.Status == section.Name);
			foreach (var item in tasks)
			{
				item.Status = _sections[newIndex].Name;
			}
		}
	}
}
