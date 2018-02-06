using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taskCoreId.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public List<string> Tag { get; set; }
        public bool IsDone { get; set; }
        public DateTime? DeadLine { get; set; }
        public ApplicationUser User {get; set; }
    }
}
