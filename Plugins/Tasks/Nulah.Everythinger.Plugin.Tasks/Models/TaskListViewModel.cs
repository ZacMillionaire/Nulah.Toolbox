﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Nulah.Everythinger.Plugins.Core.Models;
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

        private ObservableCollection<TaskItemViewModel> _taskItems;
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

        public TaskListViewModel()
        {
            _taskItems = new ObservableCollection<TaskItemViewModel>();
        }
    }
}
