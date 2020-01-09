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
        public TaskListManager TaskListManager
        {
            get
            {
                return _taskListManager;
            }
        }

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

        public ICommand CreateTaskList
        {
            get
            {
                return new DelegateCommand(CreateNewTaskList);
            }
        }

        /// <summary>
        /// Updates the active task list to the given TaskListViewModel
        /// </summary>
        /// <param name="taskListViewModel"></param>
        internal void SetActiveTaskList(TaskListViewModel taskListViewModel)
        {
            if (_activeTaskList != null)
            {
                _activeTaskList.IsActive = false;
            }
            _activeTaskList = taskListViewModel;
            if (_activeTaskList != null)
            {
                _activeTaskList.IsActive = true;
            }
        }

        /// <summary>
        /// Returns the current active task list, if any
        /// </summary>
        /// <returns></returns>
        internal TaskListViewModel GetCurrentActiveList()
        {
            return _activeTaskList;
        }

        internal TaskItemViewModel GetCurrentActiveTask()
        {
            return _selectedTaskListItem;
        }

        /// <summary>
        /// Removes the given task, and sets the active task list to null
        /// </summary>
        /// <param name="taskListViewModel"></param>
        internal void RemoveTaskList(TaskListViewModel taskListViewModel)
        {
            _taskListManager.DeleteTaskList(taskListViewModel.Id);
            TaskLists.Remove(taskListViewModel);
            SetActiveTaskList(null);
        }

        /// <summary>
        /// Returns if the task list has an entry in an edit state, which should prevent
        /// any other actions from happening
        /// </summary>
        /// <returns></returns>
        internal ActiveTaskListState CurrentTaskListState
        {
            get
            {
                if (_activeTaskList != null && _activeTaskList.IsEdit == true)
                {
                    return ActiveTaskListState.Edit;
                }

                return ActiveTaskListState.Ready;
            }
        }

        /// <summary>
        /// Updates the current active task item with the given task item, additionally sets the active list to the list that
        /// contains the task item
        /// </summary>
        /// <param name="taskItem"></param>
        internal void SetActiveTaskItem(TaskItemViewModel taskItem)
        {
            if (SelectedTaskListItem != null)
            {
                SetActiveTaskList(SelectedTaskListItem.ParentList);
                SelectedTaskListItem.IsSelected = false;
            }

            SelectedTaskListItem = taskItem;

            if (SelectedTaskListItem != null)
            {
                SetActiveTaskList(SelectedTaskListItem.ParentList);
                SelectedTaskListItem.IsSelected = true;
            }
        }

        //internal void EditTask(TaskItemViewModel taskItem)
        //{
        //    // Set the active list to the parent task item if none currently active
        //    if (_activeTaskList == null)
        //    {
        //        _activeTaskList = TaskLists.First(x => x.Id == taskItem.ListId);
        //    }

        //    if (_activeTaskList.IsEdit == false)
        //    {
        //        taskItem.Edit();
        //    }
        //}

        /// <summary>
        /// Creates a new task list item, adds it to the list of task lists, and puts it into edit mode
        /// </summary>
        private void CreateNewTaskList()
        {
            // Prevent creating a new list if we're already in a blocking edit state (another task list item is in an edit state)
            if (CurrentTaskItemState == ActiveTaskItemState.Edit || CurrentTaskListState == ActiveTaskListState.Edit)
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

        /// <summary>
        /// Removes the given task from the list it is a parent of
        /// </summary>
        /// <param name="taskItem"></param>
        internal void RemoveTask(TaskItemViewModel taskItem)
        {
            SetActiveTaskItem(null);
            taskItem.ParentList.TaskItems.Remove(taskItem);
        }


        /// <summary>
        /// Returns the state of the current selected task item
        /// </summary>
        internal ActiveTaskItemState CurrentTaskItemState
        {
            get
            {
                if (SelectedTaskListItem != null && SelectedTaskListItem.IsEditMode == true)
                {
                    return ActiveTaskItemState.Edit;
                }

                return ActiveTaskItemState.Ready;
            }
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
    /// <summary>
    /// State of the task list
    /// </summary>
    public enum ActiveTaskListState
    {
        Ready,
        Edit
    }
    // TODO: Move this
    /// <summary>
    /// State of the current selected task item
    /// </summary>
    public enum ActiveTaskItemState
    {
        Ready,
        Edit
    }
}
