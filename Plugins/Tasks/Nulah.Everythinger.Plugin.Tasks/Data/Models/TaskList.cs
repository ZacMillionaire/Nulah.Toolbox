using System;
using System.Collections.Generic;
using System.Text;
using Nulah.Data.Sqlite;

namespace Nulah.Everythinger.Plugins.Tasks.Data.Models
{
    [NulahTable]
    public class TaskList
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

        public List<TaskItem> Tasks { get; set; }
    }

    [NulahTable]
    public class TaskList_TaskItem
    {
        [NulahColumn]
        public Guid ListId { get; set; }
        [NulahColumn]
        public Guid TaskId { get; set; }
    }
}
