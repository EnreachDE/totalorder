namespace to.frontend.Models.Backlog
{
    using contracts;
    using contracts.data.result;

    public class BacklogShowViewModel
    {
        public Status Result { get; set; }
        public BacklogShowQueryResult Backlogs { get; set; }
    }
}