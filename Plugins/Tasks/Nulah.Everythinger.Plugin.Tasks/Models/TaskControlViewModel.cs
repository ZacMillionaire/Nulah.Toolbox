using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Nulah.Everythinger.Plugins.Core;
using Nulah.Everythinger.Plugins.Tasks.Data;
using Nulah.Everythinger.Plugins.Tasks.Data.Models;
using Nulah.WPF.Toolbox.Utilities;

namespace Nulah.Everythinger.Plugins.Tasks.Models
{

    public class TaskControlViewModel : ObservableViewObject
    {
        private readonly TaskListManager _taskListManager;

        private ObservableCollection<TaskListViewModel> _taskLists;

        public ObservableCollection<TaskListViewModel> TaskLists
        {
            get { return _taskLists; }
            set
            {
                _taskLists = value;
                RaisePropertyChangedEvent("TaskLists");
            }
        }

        private TaskItemViewModel _selectedTaskListItem;
        public TaskItemViewModel SelectedTaskListItem
        {
            get => _selectedTaskListItem;
            set
            {
                _selectedTaskListItem = value;
                RaisePropertyChangedEvent("SelectedTaskListItem");
            }
        }
        /// <summary>
        /// Keeps track of the current task list active - if a task is selected then it's parent list will
        /// be found here
        /// </summary>
        private TaskListViewModel _activeTaskList;

        public TaskControlViewModel()
        {

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                __DesignModeCtor();
            }
            else
            {
                ViewManager.RegisterView<TaskControlViewModel>(this);
                _taskListManager = new TaskListManager();

                UpdateTaskLists(_taskListManager.GetTaskLists());
            }
        }

        private void UpdateTaskLists(IEnumerable<TaskList> taskLists)
        {
            TaskLists = new ObservableCollection<TaskListViewModel>(taskLists.Select(x => new TaskListViewModel(x)));
        }

        public ICommand SaveTaskItem
        {
            get
            {
                return new DelegateCommand<TaskItemViewModel>(SaveTask);
            }
        }

        public ICommand CancelEditTaskItem
        {
            get
            {
                return new DelegateCommand<TaskItemViewModel>(CancelEditTask);
            }
        }

        public ICommand CreateTaskList
        {
            get
            {
                return new DelegateCommand(CreateNewTaskList);
            }
        }

        private void SaveTask(TaskItemViewModel taskItem)
        {
            // Set the active list to the parent task item if none currently active
            if (_activeTaskList == null)
            {
                _activeTaskList = TaskLists.First(x => x.Id == taskItem.ListId);
            }

            if (_activeTaskList.IsEdit == false)
            {
                taskItem.SaveChanges();
            }
        }

        private void CancelEditTask(TaskItemViewModel taskItem)
        {
            // Set the active list to the parent task item if none currently active
            if (_activeTaskList == null)
            {
                _activeTaskList = TaskLists.First(x => x.Id == taskItem.ListId);
            }

            if (taskItem.IsNew == false)
            {
                taskItem.SaveChanges(true);
            }
            else
            {
                _activeTaskList.TaskItems.Remove(taskItem);
                SelectedTaskListItem = null;
            }
        }

        /// <summary>
        /// Updates the active task list to the given TaskListViewModel
        /// </summary>
        /// <param name="taskListViewModel"></param>
        internal void SetActiveTaskList(TaskListViewModel taskListViewModel)
        {
            _activeTaskList = taskListViewModel;
        }

        /// <summary>
        /// Returns if the task list has an entry in an edit state, which should prevent
        /// any other actions from happening
        /// </summary>
        /// <returns></returns>
        internal TaskListState CurrentTaskListState
        {
            get
            {
                if (_activeTaskList != null && _activeTaskList.IsEdit == true)
                {
                    return TaskListState.Edit;
                }

                return TaskListState.Ready;
            }
        }

        internal void EditTask(TaskItemViewModel taskItem)
        {
            // Set the active list to the parent task item if none currently active
            if (_activeTaskList == null)
            {
                _activeTaskList = TaskLists.First(x => x.Id == taskItem.ListId);
            }

            if (_activeTaskList.IsEdit == false)
            {
                taskItem.Edit();
            }
        }

        //internal void ExpandItem(TaskListViewModel taskListItem)
        //{
        //    if (_activeTaskList != null && _activeTaskList.IsEdit == true)
        //    {
        //        return;
        //    }

        //    _activeTaskList = taskListItem;
        //    taskListItem.IsExpanded = !taskListItem.IsExpanded;
        //}

        //internal void EditListEntry(TaskListViewModel taskListItem)
        //{
        //    if (_activeTaskList != null && _activeTaskList.IsEdit == true)
        //    {
        //        return;
        //    }

        //    _activeTaskList = taskListItem;
        //    _activeTaskList.IsEdit = true;
        //}
        internal void DeleteTaskListItem(TaskListViewModel taskListItem)
        {
            if (_activeTaskList != null && _activeTaskList.IsEdit == false)
            {
                return;
            }

            // Unlikely but prevent deleting a task list that isn't the active task list
            if (_activeTaskList != taskListItem)
            {
                return;
            }

            var confirm = MessageBox.Show($"[To be made into better confirm?] Delete List '{taskListItem.Name}'? This will also delete all tasks contained.", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (confirm == MessageBoxResult.Yes)
            {
                TaskLists.Remove(taskListItem);
                _activeTaskList = null;
            }
        }

        internal void UpdateListEntry()
        {
            _activeTaskList.SaveChanges();
            _activeTaskList.IsEdit = false;
        }

        /// <summary>
        /// Creates a new task list item, adds it to the list of task lists, and puts it into edit mode
        /// </summary>
        private void CreateNewTaskList()
        {
            // Prevent creating a new list if we're already in a blocking edit state (another task list item is in an edit state)
            if (CurrentTaskListState == TaskListState.Edit)
            {
                return;
            }

            // Create a new task list in the database
            var newTaskList = _taskListManager.CreateTaskList();
            // Create the view model for it
            var newTaskListItem = new TaskListViewModel(newTaskList);
            // Add it to the list and put it into edit mode
            TaskLists.Add(newTaskListItem);
            newTaskListItem.ExpandAndEdit();
        }

        internal void CreateTask(TaskListViewModel parentTaskList)
        {
            if (_activeTaskList == null)
            {
                _activeTaskList = parentTaskList;
            }

            // Prevent changing the selected item if the task is unsaved
            if (PendingItemExists() || _activeTaskList.IsEdit == true)
            {
                return;
            }
            _activeTaskList = parentTaskList;

            var newTask = new TaskItemViewModel();
            _activeTaskList.TaskItems.Add(newTask);
            SelectItem(newTask);
            newTask.CreateNew(_activeTaskList.Id);
        }

        internal void SelectItem(TaskItemViewModel selectedItem)
        {
            if (_activeTaskList == null)
            {
                _activeTaskList = TaskLists.First(x => x.Id == selectedItem.ListId);
            }

            // Prevent changing the selected item if the task is unsaved
            if (PendingItemExists() || _activeTaskList.IsEdit == true)
            {
                return;
            }
            if (SelectedTaskListItem != null)
            {
                SelectedTaskListItem.IsSelected = false;
            }

            SelectedTaskListItem = selectedItem;
            SelectedTaskListItem.IsSelected = true;
        }

        private bool PendingItemExists()
        {
            return (SelectedTaskListItem != null && SelectedTaskListItem.IsEditMode == true);
        }

        private void __DesignModeCtor()
        {

            var listGuids = new Guid[]
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
            };

            TaskLists = new ObservableCollection<TaskListViewModel>(new List<TaskListViewModel>
            {
                new TaskListViewModel
                {
                    Name = "Design Task List 1",
                    IsExpanded = true,
                    Id = listGuids[0],
                    TaskItems = new ObservableCollection<TaskItemViewModel>(new List<TaskItemViewModel>
                    {
                        new TaskItemViewModel
                        {
                            Name = "Design Task List Item 1",
                            Updated = DateTime.UtcNow,
                            Content = "Test content",
                            ListId = listGuids[0],
                            Id = Guid.NewGuid()
                        },
                        new TaskItemViewModel
                        {
                            Name = "Design Task List Item 2",
                            Updated = DateTime.UtcNow.AddDays(-1),
                            TaskState = TaskItemStates.InProgress,
                            ListId = listGuids[0],
                            Id = Guid.NewGuid()
                        },
                        new TaskItemViewModel
                        {
                            Name = "Design Task List Item 3",
                            Updated = DateTime.UtcNow.AddDays(-7),
                            TaskState = TaskItemStates.Complete,
                            ListId = listGuids[0],
                            Id = Guid.NewGuid()
                        },
                        new TaskItemViewModel
                        {
                            Name = "Design Task List Item 4",
                            Updated = DateTime.UtcNow.AddDays(-20),
                            TaskState = TaskItemStates.Cancelled,
                            ListId = listGuids[0],
                            Id = Guid.NewGuid()
                        },
                    })
                },
                new TaskListViewModel
                {
                    Name = "Design Task List 2",
                    Id = listGuids[1],
                    TaskItems = new ObservableCollection<TaskItemViewModel>(new List<TaskItemViewModel>
                    {
                        new TaskItemViewModel
                        {
                            Name = "Task List Item 1",
                            ListId = listGuids[1],
                            Id = Guid.NewGuid()
                        },
                        new TaskItemViewModel
                        {
                            Name = "Task List Item 2",
                            ListId = listGuids[1],
                            Id = Guid.NewGuid()
                        }
                    })
                },
                new TaskListViewModel
                {
                    Id = listGuids[2],
                    Name = "Design Task List 3"
                },
                new TaskListViewModel
                {
                    Id = listGuids[3],
                    Name = "Design Task List 4"
                },
            });
        }
    }

    // TODO: Move this
    public enum TaskListState
    {
        Ready,
        Edit
    }

}
