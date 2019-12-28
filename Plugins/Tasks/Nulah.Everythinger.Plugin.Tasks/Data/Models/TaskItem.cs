using System;
using System.Collections.Generic;
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
        public DateTime Created { get; set; }
        [NulahColumn]
        public string Content { get; set; }
    }
}
