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
            var all = await _db.Tasks.Where(t => t.User.Id == user.Id).ToListAsync();
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
            // map dto to entity
            var entity = _mapper.Map<TaskItem>(task);
            entity.User = user;
            entity.IsDone = false;
            _db.Tasks.Add(entity);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // PUT: api/Task/update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody]TaskItemDto task)
        {
            // map dto to entity and set id
            var item = _mapper.Map<TaskItem>(task);
            //item.Id = id;
            var updItem = _db.Tasks.Find(task.Id);
            // update user properties
            updItem.Name = item.Name;
            updItem.Description = item.Description;
            //updItem.Tag = item.Tag;
            updItem.IsDone = item.IsDone;
            updItem.DeadLine = item.DeadLine;
            _db.Tasks.Update(updItem);
            _db.Entry(updItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/Task/delete
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var item = _db.Tasks.Find(id);
            if (item != null)
            {
                _db.Tasks.Remove(item);
                _db.SaveChanges();
            }
            return Ok();
        }
        public async Task<IActionResult> OppositeMark([FromBody]TaskItemDto task)
        {
            var item = _mapper.Map<TaskItem>(task);
            var updItem = _db.Tasks.Find(item.Id);
            updItem.IsDone = item.IsDone;
            _db.Tasks.Update(updItem);
            _db.Entry(updItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}