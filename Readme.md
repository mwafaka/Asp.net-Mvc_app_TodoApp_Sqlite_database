# Create TodoList with .net mvc app(.net 8) and sqlite database

##  - Step 1: Install .NET 8 SDK on Linux
- 01

```bash
sudo apt-get update
sudo apt-get install -y wget
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

```

- 02-Install the .NET SDK 8:
```bash
sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0

```

## Step 2: Create a New .NET 8 MVC Application

```bash
dotnet new mvc -n TodoListApp
```

## Step 3: Create the To-Do Model
- Inside the Models folder, create a file named TodoItem.cs:

```bash
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public bool IsComplete { get; set; }
    }
}

```

## Step 4: Create the To-Do List Controller
- Inside the Controllers folder, create a file named TodoController.cs:

```bash
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace TodoListApp.Controllers
{
    public class TodoController : Controller
    {
        private static List<TodoItem> _todoList = new List<TodoItem>();

        public IActionResult Index()
        {
            return View(_todoList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(TodoItem todo)
        {
            if (ModelState.IsValid)
            {
                todo.Id = _todoList.Count > 0 ? _todoList.Max(t => t.Id) + 1 : 1;
                _todoList.Add(todo);
                return RedirectToAction("Index");
            }
            return View(todo);
        }

        public IActionResult ToggleComplete(int id)
        {
            var todoItem = _todoList.FirstOrDefault(t => t.Id == id);
            if (todoItem != null)
            {
                todoItem.IsComplete = !todoItem.IsComplete;
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var todoItem = _todoList.FirstOrDefault(t => t.Id == id);
            if (todoItem != null)
            {
                _todoList.Remove(todoItem);
            }
            return RedirectToAction("Index");
        }
    }
}


```

## Step 5: Create the Views

- Inside the Views/Todo folder, create a file named Index.cshtml;

```bash
@model List<TodoListApp.Models.TodoItem>

<h2>Todo List</h2>

<table class="table">
    <thead>
        <tr>
            <th>Title</th>
            <th>Complete</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>@item.Title</td>
                <td>@item.IsComplete</td>
                <td>
                    <a href="/Todo/ToggleComplete/@item.Id" class="btn btn-primary">Toggle Complete</a>
                    <a href="/Todo/Delete/@item.Id" class="btn btn-danger">Delete</a>
                </td>
            </tr>
        }
    </tbody>
</table>

<a href="/Todo/Create" class="btn btn-success">Add New Todo</a>

```

- Create the Create View:

- Inside the Views/Todo folder, create a file named Create.cshtml:



```bash

@model TodoListApp.Models.TodoItem

<h2>Add New Todo</h2>

<form asp-action="Create" method="post">
    <div class="form-group">
        <label for="Title">Title</label>
        <input asp-for="Title" class="form-control" />
        <span asp-validation-for="Title" class="text-danger"></span>
    </div>
    <div class="form-group">
        <label for="IsComplete">Is Complete</label>
        <input asp-for="IsComplete" type="checkbox" />
    </div>
    <button type="submit" class="btn btn-primary">Save</button>
</form>

```

## Step 6: Configure Routing

- Ensure proper routing in Program.cs:


```bash
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Todo}/{action=Index}/{id?}");

app.Run();

```



#  Optional Step: Persist Data with a Database

## Step 1: Add EF Core and SQLite NuGet Packages


```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design

```

## 2-Create the TodoContext (DbContext)
In the Data folder, create a new class named TodoContext.cs. This class will represent the database context, which manages the entity classes and handles database communication.

```bash

using Microsoft.EntityFrameworkCore;

namespace TodoListApp.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }

        public DbSet<TodoItem> TodoItems { get; set; }
    }
}

```

## Step 3: Configure the Database Connection

- Open appsettings.json and add the SQLite connection string:

```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=todo.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}

```

## Step 4: Register the DbContext in Program.cs

 - In Program.cs and add the DbContext service in the builder.Services section. This will allow the application to inject the TodoContext whenever needed.

 ```bash
using Microsoft.EntityFrameworkCore;
using TodoListApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure the database context (SQLite)
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Other configurations (middleware, routing)
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Todo}/{action=Index}/{id?}");

app.Run();

 ```

 ## Step 5: Update the TodoController to Use the Database

 ```bash
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models;
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

 ```

 ## Step 6: Create and Apply Migrations

 - In the terminal, run the following command to create a migration, which will generate the database schema:

 ```bash
 dotnet ef migrations add InitialCreate

 ```

 - Apply the migration:
 - Run the following command to apply the migration and create the SQLite database (todo.db):

 ```bash
 dotnet ef database update

 ```

