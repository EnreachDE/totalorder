using to.contracts;
using to.contracts.data.result;

namespace to.frontend.Models.Backlog
{
    public class BacklogShowViewModel
    {
        public Status Result { get; set; }
        public BacklogShowQueryResult Backlogs { get; set; }
    }
}
