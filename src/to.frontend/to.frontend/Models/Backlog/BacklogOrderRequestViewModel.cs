using System.Collections.Generic;

namespace to.frontend.Models.Backlog
{
    public class BacklogOrderRequestViewModel
    {
        public string BacklogId { get; set; }
        public string UserStoryIndexes { get; set; }

        public int? UserId { get; set; }
    }
}