using System.Collections.Generic;

namespace to.frontend.Models.Backlog
{
    public class BacklogEvalViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> UserStories { get; set; } = new List<string>();
        public int NumberOfSubmissions { get; set; }
        public bool OneVotePerUser { get; set; }
    }
}
