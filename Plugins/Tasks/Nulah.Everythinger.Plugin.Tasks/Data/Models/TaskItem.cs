using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nulah.Data.Sqlite;

namespace Nulah.Everythinger.Plugins.Tasks.Data.Models
{
    [NulahTable]
    public class TaskItem
    {
        [NulahColumn, NulahIndex(Unique = true)]
        public Guid Id { get; set; }
        [NulahColumn]
        public string Name { get; set; }
        [NulahColumn]
        public DateTime Created { get; set; }
        [NulahColumn]
        public DateTime Updated { get; set; }
        [NulahColumn]
        public bool IsDeleted { get; set; }
        [NulahColumn]
        public TaskState State { get; set; }
        [NulahColumn]
        public string Content { get; set; }

        public Guid ListId { get; set; }

        public List<TaskItemHistory> Revisions { get; set; }
    }

    [NulahTable]
    public class TaskItemHistory
    {
        [NulahColumn, NulahIndex(Unique = true)]
        public Guid Id { get; set; }
        [NulahColumn]
        public string Name { get; set; }
        [NulahColumn]
        public DateTime Timestamp { get; set; }
        [NulahColumn]
        public string Content { get; set; }
        [NulahColumn]
        public Guid TaskId { get; set; }
        [NulahColumn]
        public TaskState State { get; set; }
    }

    public enum TaskState
    {
        New,
        NotStarted,
        InProgress,
        Complete,
        Cancelled,
    }
}
