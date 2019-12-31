using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nulah.Data.Sqlite.Models;
using Nulah.Everythinger.Plugins.Tasks.Data.Models;
using Nulah.Everythinger.Plugins.Tasks.Models;
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
            var lists = _database.Query<TaskList>("SELECT [Id],[Name],[Created],[Updated],[IsDeleted] FROM [TaskList] WHERE [IsDeleted] = 0 ORDER BY [Created]");

            return lists;
        }

        public TaskList CreateTaskList()
        {
            var newTaskList = new TaskList
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Name = "<New Task List>"
            };
            _database.Insert(newTaskList);

            return GetTaskListById(newTaskList.Id);
        }

        public TaskList GetTaskListById(Guid id)
        {
            var taskList = _database.Query<TaskList>("SELECT [Id],[Name],[Created],[Updated],[IsDeleted] FROM [TaskList] WHERE [Id] = @Id", new Dictionary<string, object>
            {
                {"Id", id }
            });

            return taskList.FirstOrDefault();
        }

        internal void DeleteTaskList(Guid id)
        {
            _database.Update<TaskList>("UPDATE [TaskList] SET [IsDeleted] = 1 WHERE [Id] = @Id", new Dictionary<string, object>{
                {"Id",id}
            });
        }

        internal TaskList UpdateTaskListEntry(TaskList taskList)
        {
            var tableMappings = _database.GetColumnMappings<TaskList>();

            var updated = _database.UseTableColumnSerialiser(taskList, tableMappings["Updated"], DateTime.UtcNow);

            _database.Query<TaskList>("UPDATE [TaskList] SET [Name] = @Name, [Updated] = @Updated WHERE [Id] = @Id", new Dictionary<string, object>
            {
                { "Name",taskList.Name },
                { "Updated", updated },
                { "Id",taskList.Id }
            });

            return GetTaskListById(taskList.Id);
        }
    }
}
