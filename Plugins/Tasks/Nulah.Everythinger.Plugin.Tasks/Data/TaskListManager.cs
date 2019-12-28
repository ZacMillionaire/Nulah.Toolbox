using System;
using System.Collections.Generic;
using System.Text;
using Nulah.Everythinger.Plugins.Tasks.Data.Models;
using Nulah.WPF.Toolbox.Utilities;

namespace Nulah.Everythinger.Plugins.Tasks.Data
{
    public class TaskListManager
    {
        private ApplicationDatabase _database;
        public TaskListManager()
        {
            _database = new ApplicationDatabase();
            CreateTables();
        }

        private void CreateTables()
        {
            _database.CreateTable<TaskList>();
            _database.CreateTable<TaskItem>();
        }

        public List<TaskList> GetTaskLists()
        {
            var lists = new List<TaskList>();

            return lists;
        }
    }
}
