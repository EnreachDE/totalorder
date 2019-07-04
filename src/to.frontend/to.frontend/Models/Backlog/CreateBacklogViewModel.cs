namespace to.frontend.Models.Backlog
{
    using System.Collections.Generic;

    public class CreateBacklogViewModel
    {
        public string Title { get; set; }
        public ICollection<string> UserStories { get; set; } = new List<string> {""};
    }
}