using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskCoreId.Models;
using AutoMapper;
namespace taskCoreId.Data
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TaskItem, TaskItemDto>();
            CreateMap<TaskItemDto, TaskItem>();
        }


        
    }
}

