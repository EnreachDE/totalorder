using System.Collections.Generic;

namespace to.frontend.Models.Backlog
{
    public class CreateBacklogViewModel
    {
        public string Title { get; set; }
        public ICollection<string> UserStories { get; set; } = new List<string> { "" };
    }
}