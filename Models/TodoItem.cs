using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        
        public required string Title { get; set; }

        public bool IsComplete { get; set; }
    }
}
