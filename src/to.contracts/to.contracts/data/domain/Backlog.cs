namespace to.contracts.data.domain
{
    public class Backlog  
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string[] UserStories { get; set; }
        public bool OneVotePerUser { get; set; }
    }
}