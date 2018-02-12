using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using taskCoreId.Data;
using taskCoreId.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using Newtonsoft.Json;

namespace taskCoreId.Controllers
{
    [Produces("application/json")]
    [Route("Task/[action]")]
    public class TaskController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        ApplicationDbContext _db;
        private IMapper _mapper;
        public TaskController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _db = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPaged(int page)
        {
            ApplicationUser user =  await GetCurrentUserAsync();
            var all = await _db.Tasks.Where(t => t.User.Id == user.Id)
                .Include(t => t.TaskTags)
                .ThenInclude(t=>t.Tag)
                .OrderBy(d => d.DeadLine).ToListAsync();
            double count = all.Count;
            int TotalPages = (int)Math.Ceiling(count / (double)8);
            var items = all.Skip((page - 1) * 8).Take(8).ToList();
           
            // Object which we are going to send in header   
            var paginationMetadata = new
            {
                totalCount = count,
                pageSize = 8,
                currentPage = page,
                totalPages = TotalPages
            };
            // Setting Header  
            Request.HttpContext.Response.Headers.Add("Page-Headers", JsonConvert.SerializeObject(paginationMetadata));
            var tasksDtos = _mapper.Map<IList<TaskItemDto>>(items);
            var result = new TaskItemDtoPagedModel
            {
                TaskItemDtos = tasksDtos,
                PageCount = TotalPages
            };
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTags()
        {
            ApplicationUser user = await GetCurrentUserAsync();
            var all = await _db.Tags.Include(t => t.TaskTags).ThenInclude(t => t.Task).Where(t=>t.TaskTags.Any(d=>d.Task.User==user)).ToListAsync();
            var tagDtos = _mapper.Map<IList<TagDto>>(all);
            return Ok(tagDtos);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            ApplicationUser user = await GetCurrentUserAsync();
            var all = await _db.Tasks.Where(t => t.User.Id == user.Id && t.Name.ToLower().Contains(query)).OrderBy(d => d.DeadLine).ToListAsync();
            var tasksDtos = _mapper.Map<IList<TaskItemDto>>(all);
            return Ok(tasksDtos);
        }
        [HttpGet]
        public async Task<IActionResult> SearchByTag(string tag)
        {
            ApplicationUser user = await GetCurrentUserAsync();
            //var tag = _mapper.Map<IList<Tag>>(tagDto);
            var all = await _db.Tasks.Where(t => t.User.Id == user.Id && t.TaskTags.Any(d=>d.Tag.Name==tag)).Include(t => t.TaskTags)
                .ThenInclude(t => t.Tag).OrderBy(d => d.DeadLine).ToListAsync();
            var tasksDtos = _mapper.Map<IList<TaskItemDto>>(all);
            return Ok(tasksDtos);
        }
        // GET: api/Task/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<IActionResult> Get(int id)
        {
            ApplicationUser user = await GetCurrentUserAsync();
            var item = await _db.Tasks.Where(t => t.User.Id == user.Id).ToListAsync();
            var itemDto = _mapper.Map<IList<TaskItemDto>>(item);
            return Ok(itemDto);
        }
        private IList<TaskTag> TagFilter(TaskItem ts, List<string> tags)
        {
            IList<TaskTag> taskTagList = new List<TaskTag>();
            List<string> newTags = new List<string>();
            for (int i = 0; i < tags.Count; i++)
            {
                var temp = _db.Tags.SingleOrDefault(t => t.Name == tags[i]);
                if (temp != null)
                {
                    TaskTag tasktag = new TaskTag { Task = ts, Tag = temp };
                    taskTagList.Add(tasktag);
                }
                else
                {
                    newTags.Add(tags[i]);
                }
            }
            if (newTags.Count != 0)
            {
                for (int i = 0; i < newTags.Count; i++)
                {
                    var tag = new Tag { Name = newTags[i] };
                    _db.Tags.Add(tag);

                    TaskTag tasktag2 = new TaskTag { Task = ts, Tag = tag };
                    taskTagList.Add(tasktag2);
                }
            }
            return taskTagList;
        }

        // POST: api/Task/Add
        [HttpPost]
        public async Task<IActionResult> Add([FromBody]TaskItemDto task)
        {
            ApplicationUser user = await GetCurrentUserAsync();
            TaskItem ts = new TaskItem { Name = task.Name, Description = task.Description, IsDone = false, DeadLine = task.DeadLine, User = user };
            ts.TaskTags = TagFilter(ts, task.Tags);
            _db.Tasks.Add(ts);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // PUT: api/Task/update
        [HttpPut]
        public async Task<IActionResult> Update([FromBody]TaskItemDto task)
        {
            TaskItem updItem = _db.Tasks.Find(task.TaskId);
            // update user properties
            updItem.Name = task.Name;
            updItem.Description = task.Description;
            updItem.IsDone = task.IsDone;
            updItem.DeadLine = task.DeadLine;
            _db.TaskTag.RemoveRange(_db.TaskTag.Where(t => t.TaskId == task.TaskId));
            _db.SaveChanges();
            //updItem.TaskTags.Clear();
             updItem.TaskTags = TagFilter(updItem, task.Tags);
            _db.Tasks.Update(updItem);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/Task/delete
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var delItem = _db.Tasks.Find(id);
            if (delItem != null)
            {
                _db.Tasks.Remove(delItem);
                _db.SaveChanges();
            }
            return Ok();
        }
        public async Task<IActionResult> OppositeMark([FromBody]TaskItemDto task)
        {
            //var item = _mapper.Map<TaskItem>(task);
            var updItem = _db.Tasks.Find(task.TaskId);
            updItem.IsDone = task.IsDone;
            _db.Tasks.Update(updItem);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}