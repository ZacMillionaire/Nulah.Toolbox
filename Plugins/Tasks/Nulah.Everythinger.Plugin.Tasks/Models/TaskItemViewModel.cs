using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Nulah.Everythinger.Plugins.Core;
using Nulah.WPF.Toolbox.Utilities;

namespace Nulah.Everythinger.Plugins.Tasks.Models
{
    public enum TaskItemStates
    {
        Default,
        InProgress,
        Complete,
        Cancelled
    }

    public class TaskItemViewModel : ObservableViewObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChangedEvent("Name");
            }
        }

        private DateTime _updated;
        public DateTime Updated
        {
            get => _updated;
            set
            {
                _updated = value;
                RaisePropertyChangedEvent("LastActionDate");
            }
        }

        private DateTime _created;
        public DateTime CreatedDate
        {
            get => _created;
            set
            {
                _created = value;
            }
        }

        private bool _isSelected;

        public string LastActionDate
        {
            get
            {
                var timeDiff = DateTime.UtcNow - Updated;
                if (timeDiff.Days < 1)
                {
                    return Updated.ToLocalTime().ToString("HH:mm");
                }
                else if (timeDiff.Days < 7)
                {
                    return Updated.ToLocalTime().ToString("ddd, HH:mm");
                }
                return Updated.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChangedEvent("IsSelected"); }
        }

        private TaskItemStates _taskState;

        public TaskItemStates TaskState
        {
            get { return _taskState; }
            set { _taskState = value; RaisePropertyChangedEvent("TaskState"); }
        }

        public static List<TaskItemStates> AvailableTaskStates
        {
            get
            {
                return Enum.GetValues(typeof(TaskItemStates))
                    .Cast<TaskItemStates>()
                    .Where(x => x != TaskItemStates.Default)
                    .ToList();
            }
        }

        private bool _isNew;

        /// <summary>
        /// True if the item has been changed while in an edit mode
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
            set { _isNew = value; RaisePropertyChangedEvent("IsNew"); }
        }

        private bool _isInEditMode;

        public bool IsEditMode
        {
            get { return _isInEditMode; }
            set { _isInEditMode = value; RaisePropertyChangedEvent("IsEditMode"); }
        }

        private string _content;

        public string Content
        {
            get { return _content; }
            set { _content = value; RaisePropertyChangedEvent("Content"); }
        }
        private string _editContent;

        public string EditContent
        {
            get { return _editContent; }
            set { _editContent = value; RaisePropertyChangedEvent("EditContent"); }
        }

        private string _editName;

        public string EditName
        {
            get { return _editName; }
            set { _editName = value; RaisePropertyChangedEvent("EditName"); }
        }


        /// <summary>
        /// Id of the task
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Id of the list the task is contained in
        /// </summary>
        public Guid ListId { get; set; }

        public void SaveChanges(bool discardChanges = false)
        {
            IsEditMode = false;
            // If the item was added, create its entry
            if (IsNew)
            {
                IsNew = false;
                TaskState = TaskItemStates.InProgress;
            }
            if (discardChanges == false)
            {
                Content = EditContent;
                Name = EditName;
            }
        }

        public void Edit()
        {
            IsEditMode = true;
            EditContent = Content;
            EditName = Name;
        }

        // example refactor for moving commands to their appropriate view models
        public ICommand EditTaskItem
        {
            get
            {
                return new DelegateCommand<TaskItemViewModel>(_viewModel.EditTask);
            }
        }

        public void CreateNew(Guid parentListId)
        {
            IsNew = true;
            Name = "<New Item>";
            Content = string.Empty;
            CreatedDate = DateTime.UtcNow;
            ListId = parentListId;
            Edit();
        }

        private readonly TaskControlViewModel _viewModel;

        public TaskItemViewModel()
        {
            Name = "Design mode name";
            Updated = DateTime.UtcNow;
            CreatedDate = DateTime.UtcNow;
            _viewModel = ViewManager.GetView<TaskControlViewModel>();
        }
    }
}