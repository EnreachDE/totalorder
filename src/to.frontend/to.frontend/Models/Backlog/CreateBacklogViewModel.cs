using System.Collections.Generic;

namespace to.frontend.Models.Backlog
{
    using System.ComponentModel.DataAnnotations;

    public class CreateBacklogViewModel
    {
        public string Title { get; set; }
        public ICollection<string> UserStories { get; set; } = new List<string> { "" };
        [Display(Name = "Allow only one vote per user")]
        public bool OneVotePerUser { get; set; }
        
    }
}