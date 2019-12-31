using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Nulah.Everythinger.Plugins.Core;
using Nulah.Everythinger.Plugins.Core.Models;
using Nulah.Everythinger.Plugins.Tasks.Data.Models;
using Nulah.WPF.Toolbox.Utilities;

namespace Nulah.Everythinger.Plugins.Tasks.Models
{
    public class TaskListViewModel : ObservableViewObject
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChangedEvent("Name");
            }
        }

        private DateTime _createdDate;

        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }


        private ObservableCollection<TaskItemViewModel> _taskItems = new ObservableCollection<TaskItemViewModel>();
        public ObservableCollection<TaskItemViewModel> TaskItems
        {
            get { return _taskItems; }
            set { _taskItems = value; RaisePropertyChangedEvent("TaskItems"); }
        }
        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; RaisePropertyChangedEvent("IsExpanded"); }
        }

        private bool _isEdit;

        public bool IsEdit
        {
            get { return _isEdit; }
            set { _isEdit = value; RaisePropertyChangedEvent("IsEdit"); }
        }

        public Guid Id { get; set; }


        public ICommand ExpandListItem
        {
            get
            {
                return new DelegateCommand(Expand);
            }
        }

        public ICommand DeleteListItem
        {
            get
            {
                return new DelegateCommand(Delete);
            }
        }

        public ICommand EditListItem
        {
            get
            {
                return new DelegateCommand(Edit);
            }
        }

        public ICommand SaveTaskListEdit
        {
            get
            {
                return new DelegateCommand(Update);
            }
        }

        public ICommand CreateTaskItem
        {
            get
            {
                return new DelegateCommand<TaskListViewModel>(_viewModel.CreateTask);
            }
        }

        public ICommand SelectTaskItem
        {
            get
            {
                return new DelegateCommand<TaskItemViewModel>(Select);
            }
        }

        public void a(object sender, object b)
        {

        }

        private readonly TaskControlViewModel _viewModel;


        public TaskListViewModel()
        {
            _viewModel = ViewManager.GetView<TaskControlViewModel>();
        }

        /// <summary>
        /// The TaskList that generated the view model
        /// </summary>
        private TaskList _backingTask;

        public TaskListViewModel(TaskList taskList) : this()
        {
            UpdateView(taskList);
        }

        /// <summary>
        /// Updates the state of the view with the given taskList
        /// </summary>
        /// <param name="taskList"></param>
        private void UpdateView(TaskList taskList)
        {
            Id = taskList.Id;
            Name = taskList.Name;
            CreatedDate = taskList.Created;
            _backingTask = taskList;
        }

        /// <summary>
        /// Toggles the expanded state of the task item
        /// </summary>
        private void Expand()
        {
            if (_viewModel.CurrentTaskListState == TaskListState.Edit)
            {
                return;
            }

            _viewModel.SetActiveTaskList(this);
            IsExpanded = !IsExpanded;
        }

        private void Edit()
        {
            if (_viewModel.CurrentTaskListState == TaskListState.Edit)
            {
                return;
            }

            _viewModel.SetActiveTaskList(this);
            IsEdit = true;
        }

        private void Delete()
        {
            // Prevent delete if task items exist
            if (_taskItems.Count != 0 || _viewModel.CurrentTaskListState != TaskListState.Edit)
            {
                return;
            }

            // Unlikely, but prevent deleting if this task isn't the active one
            if (_viewModel.GetCurrentActiveList() != this)
            {
                return;
            }

            var confirm = MessageBox.Show($"[To be made into better confirm?] Delete List '{Name}'? This will also delete all tasks contained below.", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (confirm == MessageBoxResult.Yes)
            {
                _viewModel.RemoveTaskList(this);
            }
        }


        private void Select(TaskItemViewModel taskItem)
        {
            _viewModel.SetActiveTaskList(this);

            // Prevent changing the selected item if the task is unsaved or the list is in edit mode
            if (_viewModel.CurrentTaskItemState == TaskItemState.Edit || _viewModel.CurrentTaskListState == TaskListState.Edit)
            {
                return;
            }

            _viewModel.SetActiveTaskItem(taskItem);
        }

        private void Update()
        {
            if (string.IsNullOrWhiteSpace(Name) == false)
            {
                SaveChanges();
                IsEdit = false;
            }
        }

        /// <summary>
        /// Takes any non-view specific properties and updates the database as needed
        /// </summary>
        private void SaveChanges()
        {
            // Only commit a change if the name loaded differs from the given name
            if (_backingTask.Name != Name)
            {
                _backingTask.Name = Name;
                var updatedEntry = _viewModel.TaskListManager.UpdateTaskListEntry(_backingTask);
                UpdateView(updatedEntry);
            }
        }

        /// <summary>
        /// Used after creating a new list, expands the created item and puts it into an edit state
        /// </summary>
        public void ExpandAndEdit()
        {
            Expand();
            Edit();
        }
    }
}
