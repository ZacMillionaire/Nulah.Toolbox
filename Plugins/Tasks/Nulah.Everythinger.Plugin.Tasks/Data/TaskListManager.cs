using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<TaskList> GetTaskLists()
        {
            var lists = _database.Query<TaskList>("SELECT [Id],[Created],[Name] FROM [TaskList] ORDER BY [Name]");

            return lists;
        }

        public TaskList CreateTaskList()
        {
            var newTaskList = new TaskList
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Name = "<New Task List>"
            };
            _database.Insert(newTaskList);
            var createdTaskList = _database.Query<TaskList>("SELECT [Id],[Created],[Name] FROM [TaskList] WHERE [Id] = @Id", new Dictionary<string, object>
            {
                {"@Id", newTaskList.Id }
            });

            return createdTaskList.FirstOrDefault();
        }

        internal void DeleteTaskList(Guid id)
        {
            _database.Delete<TaskList>("DELETE FROM [TaskList] WHERE [Id] = @Id", new Dictionary<string, object>{
                {
                "@Id",id
                }
            });
        }
    }
}
