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
        public async Task<IActionResult> GetAll()
        {
            ApplicationUser user =  await GetCurrentUserAsync();
            var all = await _db.Tasks.Where(t => t.User.Id == user.Id)
                .Include(t => t.TaskTags)
                .ThenInclude(t=>t.Tag)
                .OrderBy(d => d.DeadLine).ToListAsync();

            var ww = all;
            //_db.Tasks.Where(t => t.TaskTags)
            var tasksDtos = _mapper.Map<IList<TaskItemDto>>(all);
            return Ok(tasksDtos);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            ApplicationUser user = await GetCurrentUserAsync();
            var all = await _db.Tasks.Where(t => t.User.Id == user.Id && t.Name.ToLower().Contains(query)).OrderBy(d => d.DeadLine).ToListAsync();
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

        // POST: api/Task/Add
        [HttpPost]
        public async Task<IActionResult> Add([FromBody]TaskItemDto task)
        {
            ApplicationUser user = await GetCurrentUserAsync();
            TaskItem ts = new TaskItem { Name = task.Name, Description = task.Description, IsDone = false, DeadLine = task.DeadLine, User = user };
            IList<TaskTag> taskTagList = new List<TaskTag>();
            foreach(var s in _db.Tags)
            {

                for(int i =0; i<task.Tags.Count; i++)
                {
                    if(s.Name == task.Tags[i])
                    {
                        TaskTag tasktag = new TaskTag { Task = ts, Tag = _db.Tags.First(t => t.Name == task.Tags[i]) }; taskTagList.Add(tasktag);
                        task.Tags.RemoveAt(i);
                    }
                    
                }
            }
            if (task.Tags.Count != 0)
            {
                for (int i = 0; i < task.Tags.Count; i++)
                {
                    _db.Tags.Add(new Tag { Name = task.Tags[i] });
                    await _db.SaveChangesAsync();
                    Tag tg = new Tag { TagId = _db.Tags.First(t => t.Name == task.Tags[i]).TagId, Name = task.Tags[i] };
                    TaskTag tasktag2 = new TaskTag { Task = ts, Tag = tg };
                    taskTagList.Add(tasktag2);
                }
            }
            ts.TaskTags = taskTagList;
            _db.Tasks.Add(ts);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // PUT: api/Task/update
        [HttpPut]
        public async Task<IActionResult> Update([FromBody]TaskItemDto task)
        {
            // map dto to entity and set id
            var item = _mapper.Map<TaskItem>(task);
            //item.Id = id;
            var updItem = _db.Tasks.Find(task.TaskId);
            // update user properties
            updItem.Name = item.Name;
            updItem.Description = item.Description;
            //updItem.Tag = item.Tag;
            updItem.IsDone = item.IsDone;
            updItem.DeadLine = item.DeadLine;
            //updItem.TaskTags.Select(t => t.Tag.TagId) = new Tag { TagId = 1, Name = "www" };
            var ww = _db.Tasks.Include(t => t.TaskTags).ThenInclude(t => t.Tag);
            
            _db.Tasks.Update(updItem);
            _db.Entry(updItem).State = EntityState.Modified;
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
            var item = _mapper.Map<TaskItem>(task);
            var updItem = _db.Tasks.Find(item.TaskId);
            updItem.IsDone = item.IsDone;
            _db.Tasks.Update(updItem);
            _db.Entry(updItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}