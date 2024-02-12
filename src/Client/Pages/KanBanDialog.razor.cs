﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Shared;
using System.ComponentModel.DataAnnotations;

namespace Client.Pages;

public partial class KanBanDialog
{
	[CascadingParameter] MudDialogInstance MudDialog { get; set; }
	[Parameter] public KanBanDialogData Model { get; set; } = new();

	void Submit()
	{
		if (_sections.Count != 0)
		{
			Model.KanBanSections = _sections.Select(section => new KanBanSectionDTO
			{
				Name = section.Name,
				NewTaskName = section.NewTaskName,
				NewTaskOpen = section.NewTaskOpen,
			});

			Model.KanBanTaskItems = _tasks.Select(item => new KanBanTaskItemDTO { Name = item.Name, Status = item.Status });
		}
		
		MudDialog.Close(Model);
	}

	void Cancel() => MudDialog.Cancel();

	protected override void OnInitialized()
	{
		_sections = Model.KanBanSections.Select(x => new KanBanSections(x.Name, x.NewTaskOpen, x.NewTaskName)).ToList();
		_tasks = Model.KanBanTaskItems.Select(x => new KanbanTaskItem(x.Name, x.Status)).ToList();
	}

	private MudDropContainer<KanbanTaskItem> _dropContainer;

	private bool _addSectionOpen;
	/* handling board events */
	private void TaskUpdated(MudItemDropInfo<KanbanTaskItem> info)
	{
		info.Item.Status = info.DropzoneIdentifier;
	}

	/* Setup for board  */
	private List<KanBanSections> _sections = [];

	public class KanBanSections
	{
		public string Name { get; init; }
		public bool NewTaskOpen { get; set; }
		public string NewTaskName { get; set; }

		public KanBanSections(string name, bool newTaskOpen, string newTaskName)
		{
			Name = name;
			NewTaskOpen = newTaskOpen;
			NewTaskName = newTaskName;
		}
	}
	public class KanbanTaskItem
	{
		public string Name { get; init; }
		public string Status { get; set; }

		public KanbanTaskItem(string name, string status)
		{
			Name = name;
			Status = status;
		}
	}

	private List<KanbanTaskItem> _tasks = [];

	KanBanNewForm newSectionModel = new KanBanNewForm();

	public class KanBanNewForm
	{
		[Required]
		[StringLength(10, ErrorMessage = "Name length can't be more than 10.")]
		public string Name { get; set; }
	}

	private void OnValidSectionSubmit(EditContext context)
	{
		_sections.Add(new KanBanSections(newSectionModel.Name, false, String.Empty));
		newSectionModel.Name = string.Empty;
		_addSectionOpen = false;
	}

	private void OpenAddNewSection()
	{
		_addSectionOpen = true;
	}

	private void AddTask(KanBanSections section)
	{
		_tasks.Add(new KanbanTaskItem(section.NewTaskName, section.Name));
		section.NewTaskName = string.Empty;
		section.NewTaskOpen = false;
		_dropContainer.Refresh();
	}

	private void DeleteSection(KanBanSections section)
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
