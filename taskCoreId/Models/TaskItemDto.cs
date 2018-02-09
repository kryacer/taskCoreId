using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taskCoreId.Models
{
    public class TaskItemDto
    {
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public bool IsDone { get; set; }
        public DateTime? DeadLine { get; set; }

    }
}
