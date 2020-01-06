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
            _database.CreateTable<TaskList_TaskItem>();
            _database.CreateTable<TaskItemHistory>();
        }

        internal IEnumerable<TaskList> GetTaskLists()
        {
            var lists = _database.Query<TaskList>("SELECT [Id],[Name],[Created],[Updated],[IsDeleted] FROM [TaskList] WHERE [IsDeleted] = 0 ORDER BY [Created]");
            return lists;
        }

        internal TaskList CreateTaskList()
        {
            var newTaskList = new TaskList
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Name = "<New Task List>",
            };
            _database.Insert(newTaskList);

            return GetTaskListById(newTaskList.Id);
        }

        internal TaskList GetTaskListById(Guid id)
        {
            var taskList = _database.Query<TaskList>("SELECT [Id],[Name],[Created],[Updated],[IsDeleted] FROM [TaskList] WHERE [Id] = @Id",
                new Dictionary<string, object>
                {
                    {"Id", id }
                })
                .FirstOrDefault();

            if (taskList == null)
            {
                return null;
            }

            var tasksForList = GetTasksForList(id);

            taskList.Tasks = tasksForList.ToList();


            return taskList;
        }

        internal IEnumerable<TaskItem> GetTasksForList(Guid listId)
        {
            var taskIds = _database.Query<TaskList_TaskItem>(@"SELECT [ListId],[TaskId] FROM [TaskList_TaskItem] WHERE [ListId] = @ListId",
                    new Dictionary<string, object> {
                        {"ListId", listId}
                    }
                )
                .Select(x => x.TaskId);

            return GetTasksByIds(taskIds);
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

        internal TaskItem CreateTask(Guid parentListId)
        {
            var taskItem = new TaskItem
            {
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Id = Guid.NewGuid(),
            };
            _database.Insert(taskItem);
            AddTaskToList(taskItem.Id, parentListId);

            return UpdateTaskItem(taskItem, "[Task created]", "[Task created]", TaskState.New);
        }

        private TaskList_TaskItem AddTaskToList(Guid taskId, Guid listId)
        {
            _database.Insert(new TaskList_TaskItem
            {
                TaskId = taskId,
                ListId = listId
            });

            return _database.Query<TaskList_TaskItem>("SELECT [TaskId],[ListId] FROM [TaskList_TaskItem] WHERE [TaskId] = @TaskId AND [ListId] = @ListId",
                new Dictionary<string, object> {
                    {"TaskId", taskId},
                    {"ListId", listId}
                }
            ).FirstOrDefault();
        }

        internal TaskItem GetTaskItemById(Guid id)
        {
            var taskItem = _database.Query<TaskItem>("SELECT [Id],[Name],[Content],[Created],[Updated],[IsDeleted],[State] FROM [TaskItem] WHERE [Id] = @Id",
                new Dictionary<string, object>
                {
                    {"Id", id }
                })
                .FirstOrDefault();

            taskItem.Revisions = GetTaskHistory(id).OrderByDescending(x => x.Timestamp).ToList();

            return taskItem;
        }

        /// <summary>
        /// Adds a history item to the given task by id, then returns the updated task
        /// </summary>
        /// <param name="backingTaskItem"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        internal TaskItem UpdateTaskItem(TaskItem backingTaskItem, string name, string content, TaskState state)
        {
            // TODO: change TaskItem param to Guid of its Id


            var historyEntry = new TaskItemHistory
            {
                Content = content,
                Name = name,
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                TaskId = backingTaskItem.Id,
                State = state
            };

            _database.Insert(historyEntry);

            // use serialisers for datetime
            var tableMappings = _database.GetColumnMappings<TaskItem>();
            var serialisedUpdatedTime = _database.UseTableColumnSerialiser(new TaskItem(), tableMappings["Updated"], DateTime.UtcNow);

            var updateParams = new Dictionary<string, object>()
            {
                {"Updated", serialisedUpdatedTime},
                /*
                {"Name", name},
                {"Content", content},
                */
                {"State", state}
            };

            // Only update content if its changed and isn't empty
            if (string.IsNullOrWhiteSpace(name) == false && backingTaskItem.Name != name)
            {
                updateParams.Add("Name", name);
            }

            if (string.IsNullOrWhiteSpace(content) == false && backingTaskItem.Content != content)
            {
                updateParams.Add("Content", content);
            }

            var updateQueryBody = string.Join(",", updateParams.Select(x => $"[{x.Key}] = @{x.Key}"));
            updateParams.Add("Id", backingTaskItem.Id);

            _database.Update<TaskItem>($"UPDATE [TaskItem] SET {updateQueryBody} WHERE [Id] = @Id", updateParams);

            var taskItem = GetTaskItemById(backingTaskItem.Id);

            return taskItem;
        }

        internal IEnumerable<TaskItem> GetTasksByIds(IEnumerable<Guid> taskIds)
        {
            var queryParams = taskIds.Select((x, i) => new { k = $"@Id{i}", v = x })
                .ToDictionary(x => x.k, x => (object)x.v);

            var tasks = _database.Query<TaskItem>($@"SELECT [Id],[Name],[Created],[Updated],[IsDeleted],[State] 
FROM [TaskItem] 
WHERE [Id] IN ({string.Join(",", queryParams.Keys)})", queryParams);

            foreach (var task in tasks)
            {
                task.Revisions = GetTaskHistory(task.Id).OrderByDescending(x => x.Timestamp).ToList();
            }

            return tasks;
        }

        private IEnumerable<TaskItemHistory> GetTaskHistory(Guid taskId)
        {
            var history = _database.Query<TaskItemHistory>("SELECT [Id],[Name],[Timestamp],[Content],[TaskId] FROM [TaskItemHistory] WHERE [TaskId] = @TaskId",
                new Dictionary<string, object>
                {
                    {"TaskId",taskId }
                }
            );

            return history;
        }

        /// <summary>
        /// Soft deletes a task from the database, setting <see cref="TaskItem.IsDeleted"/> to true allowing recovery.
        /// <para>If <paramref name="hardDelete"/> is true, then the task is unrecoverably deleted, including all of its related item history</para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hardDelete"></param>
        internal void DeleteTask(Guid id, bool hardDelete = false)
        {
            if (hardDelete == true)
            {
                HardDeleteTaskById(id);
            }
            else
            {
                SoftDeleteTaskById(id);
            }
        }

        /// <summary>
        /// Soft deletes a task, setting <see cref="TaskItem.IsDeleted"/> to true.
        /// </summary>
        /// <param name="id"></param>
        private void SoftDeleteTaskById(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Hard deletes a task, removing all traces of the task by id including task history and references from lists.
        /// </summary>
        /// <param name="taskId"></param>
        private void HardDeleteTaskById(Guid taskId)
        {
            var parameterDict = new Dictionary<string, object> { { "TaskId", taskId } };
            _database.Delete<TaskItemHistory>("DELETE FROM [TaskItemHistory] WHERE TaskId = @TaskId", parameterDict);
            _database.Delete<TaskList_TaskItem>("DELETE FROM [TaskList_TaskItem] WHERE TaskId = @TaskId", parameterDict);
            _database.Delete<TaskItem>("DELETE FROM [TaskItem] WHERE Id = @TaskId", parameterDict);
        }
    }
}
