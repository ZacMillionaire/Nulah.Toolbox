using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Nulah.Everythinger.Plugins.Core;
using Nulah.Everythinger.Plugins.Tasks.Data.Models;
using Nulah.WPF.Toolbox.Utilities;

namespace Nulah.Everythinger.Plugins.Tasks.Models
{
    // TODO: Keeping this in sync with the data.models enum is going to be dumb in the future
    public enum TaskItemStates
    {
        New,
        NotStarted,
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
            set
            {
                _taskState = value;
                RaisePropertyChangedEvent("TaskState");
                // This really shouldn't be here but for now to get it working it is
                if (_isInit == true && _backingTaskItem != null && ModelStateToDatabaseState(value) != _backingTaskItem.State)
                {
                    _viewModel.TaskListManager.UpdateTaskItem(_backingTaskItem, _backingTaskItem.Name, _backingTaskItem.Content, ModelStateToDatabaseState(_taskState));
                }
            }
        }

        // Quick hack to use as a flag to indicate data is loaded
        private bool _isInit { get; set; }

        public static List<TaskItemStates> AvailableTaskStates
        {
            get
            {
                return Enum.GetValues(typeof(TaskItemStates))
                    .Cast<TaskItemStates>()
                    .ToList();
            }
        }

        /// <summary>
        /// Helper method to return the database enum for task states.
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        private TaskState ModelStateToDatabaseState(TaskItemStates modelState)
        {
            // This is a bit of a hack to get around the disconnect between the database enum and the view model enum.
            // I'll probably just tie the view model to the database model eventually
            return (TaskState)Enum.ToObject(typeof(TaskState), modelState);
        }

        /// <summary>
        /// Helper method to return the database enum for task states.
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        private TaskItemStates DatabaseStateToModelState(TaskState modelState)
        {
            // This is a bit of a hack to get around the disconnect between the database enum and the view model enum.
            // I'll probably just tie the view model to the database model eventually
            return (TaskItemStates)Enum.ToObject(typeof(TaskItemStates), modelState);
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

        private void Save(bool discardChanges = false)
        {
            IsEditMode = false;
            // Discard changes will be true if an edit is cancelled
            if (discardChanges == false)
            {
                // Really need to make a better sync for TaskStates
                var updatedTask = _viewModel.TaskListManager.UpdateTaskItem(_backingTaskItem, EditName, EditContent,
                    ModelStateToDatabaseState(IsNew ? TaskItemStates.NotStarted : TaskState)
                );
                Content = updatedTask.Content;
                Name = updatedTask.Name;
                TaskState = DatabaseStateToModelState(updatedTask.State);
                // Update the backing task
                _backingTaskItem = updatedTask;
            }
            // If the item was added, remove the IsNew flag
            if (IsNew)
            {
                IsNew = false;
            }
        }

        private void UpdateTask()
        {

        }

        public ICommand EditTaskItem
        {
            get
            {
                return new DelegateCommand(Edit);
            }
        }

        public ICommand SelectTaskItem
        {
            get
            {
                return new DelegateCommand(SelectTask);
            }
        }
        public ICommand SaveTaskItem
        {
            get
            {
                return new DelegateCommand(SaveTask);
            }
        }

        public ICommand CancelEditTaskItem
        {
            get
            {
                return new DelegateCommand(CancelEditTask);
            }
        }

        /// <summary>
        /// Selects a given task and sets it and this task list as active items
        /// </summary>
        /// <param name="taskItem"></param>
        private void SelectTask()
        {
            _viewModel.SetActiveTaskList(ParentList);

            // Prevent changing the selected item if the task is unsaved or the list is in edit mode
            if (_viewModel.CurrentTaskItemState == ActiveTaskItemState.Edit || _viewModel.CurrentTaskListState == ActiveTaskListState.Edit)
            {
                return;
            }

            _backingTaskItem = _viewModel.TaskListManager.GetTaskItemById(_backingTaskItem.Id);
            UpdateFromBackingTask();
            _viewModel.SetActiveTaskItem(this);
        }

        /// <summary>
        /// TaskItem from data source that the view was created from
        /// </summary>
        private TaskItem _backingTaskItem { get; set; }

        public void Create(TaskListViewModel parentTaskList)
        {
            ParentList = parentTaskList;

            _backingTaskItem = _viewModel.TaskListManager.CreateTask(parentTaskList.Id);
            SelectTask();

            IsNew = true;
            Name = _backingTaskItem.Name;
            Content = string.Empty;
            ListId = ParentList.Id;

            Edit();
        }

        private void Edit()
        {
            _viewModel.SetActiveTaskItem(this);

            // Only allow edit if the current task list is not being edited
            if (_viewModel.CurrentTaskListState == ActiveTaskListState.Ready)
            {
                IsEditMode = true;
                EditContent = _backingTaskItem.Content;
                EditName = _backingTaskItem.Name;
            }
        }

        private void CancelEditTask()
        {
            _viewModel.SetActiveTaskItem(this);

            if (IsNew == false)
            {
                Save(true);
            }
            else
            {
                // If editing is cancelled and the item was new (ie, a new task was never saved), remove it from the view
                _viewModel.RemoveTask(this);
                _viewModel.TaskListManager.DeleteTask(_backingTaskItem.Id, hardDelete: true);
            }
        }

        private void SaveTask()
        {
            _viewModel.SetActiveTaskItem(this);

            if (_viewModel.CurrentTaskListState == ActiveTaskListState.Ready)
            {
                // TODO: this should also be made into a binding for buttons so that it's only enabled if the content is valid
                if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditContent))
                {
                    MessageBox.Show("[TODO] Name or content cannot be empty");
                    return;
                }
                Save();
            }
        }

        private readonly TaskControlViewModel _viewModel;
        public TaskListViewModel ParentList { get; private set; }

        public TaskItemViewModel()
        {
            Name = "Design mode name";
            Updated = DateTime.UtcNow;
            CreatedDate = DateTime.UtcNow;
            _viewModel = ViewManager.GetView<TaskControlViewModel>();
        }

        private void UpdateFromBackingTask()
        {
            CreatedDate = _backingTaskItem.Created;
            Name = _backingTaskItem.Name;
            Content = _backingTaskItem.Content;
            Updated = _backingTaskItem.Updated;
            ListId = this.Id;
            Content = _backingTaskItem.Content;
            Id = _backingTaskItem.Id;
            TaskState = (TaskItemStates)_backingTaskItem.State;
        }

        public TaskItemViewModel(TaskItem task)
        {
            _viewModel = ViewManager.GetView<TaskControlViewModel>();
            _backingTaskItem = task;
            UpdateFromBackingTask();
            _isInit = true;
        }
    }
}