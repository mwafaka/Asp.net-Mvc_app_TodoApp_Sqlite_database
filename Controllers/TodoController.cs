using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models;
using TodoListApp.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TodoListApp.Controllers
{
    public class TodoController : Controller
    {
        private readonly TodoContext _context;

        public TodoController(TodoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var todoList = await _context.TodoItems.ToListAsync();
            return View(todoList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoItem todo)
        {
            if (ModelState.IsValid)
            {
                _context.TodoItems.Add(todo);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(todo);
        }

        public async Task<IActionResult> ToggleComplete(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem != null)
            {
                todoItem.IsComplete = !todoItem.IsComplete;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem != null)
            {
                _context.TodoItems.Remove(todoItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
