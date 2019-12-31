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

        public void SaveChanges()
        {

        }

        public ICommand CreateTaskItem
        {
            get
            {
                return new DelegateCommand<TaskListViewModel>(_viewModel.CreateTask);
            }
        }
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
                return new DelegateCommand(_viewModel.UpdateListEntry);
            }
        }

        public ICommand SelectTaskItem
        {
            get
            {
                return new DelegateCommand<TaskItemViewModel>(_viewModel.SelectItem);
            }
        }

        private readonly TaskControlViewModel _viewModel;


        public TaskListViewModel()
        {
            _viewModel = ViewManager.GetView<TaskControlViewModel>();
        }

        public TaskListViewModel(TaskList x) : this()
        {
            this.Id = x.Id;
            this.Name = x.Name;
            this.CreatedDate = x.Created;
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
