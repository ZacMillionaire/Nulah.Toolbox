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

        public List<TaskItem> Tasks { get; set; }
    }
}
