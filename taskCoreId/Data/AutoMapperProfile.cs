﻿using System;
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
            CreateMap<TaskItem, TaskItemDto>().ForMember(destination => destination.Tags, map => map.MapFrom(
                source => source.TaskTags.Select(t=>t.Tag.Name)));
            // CreateMap<TaskItemDto, TaskItem>().ForMember(destination =>destination.TaskTags.Select(t => t.Tag.Name), map=>map.MapFrom(source =>source.Tags));
            CreateMap<TaskTag, TagDto>().ForMember(destination => destination.Name, map =>map.MapFrom(source => source.Tag.Name));
        }


        
    }
}

