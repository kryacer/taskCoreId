﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace taskCoreId.Models
{
    public class TaskItem
    {
        [Key]
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<TaskTag> TaskTags { get; set; }
        public bool IsDone { get; set; }
        public DateTime? DeadLine { get; set; }
        public ApplicationUser User {get; set; }
    }
}
