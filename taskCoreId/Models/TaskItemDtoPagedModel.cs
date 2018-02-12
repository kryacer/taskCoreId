using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taskCoreId.Models
{
    public class TaskItemDtoPagedModel
    {
        public IList<TaskItemDto> TaskItemDtos { get; set; }
        public int PageCount { get; set; }
    }
}
