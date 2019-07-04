namespace to.contracts
{
    using System.Collections.Generic;

    using data.domain;

    public interface IBacklogRepo
    {
        string CreateBacklog(Backlog backlog);
        Submission[] ReadSubmissions(string id);
        Backlog ReadBacklog(string id);
        void WriteSubmission(string id, Submission submission);
        List<Backlog> GetAll();
        void DeleteBacklog(string id);
        List<Backlog> GetBacklogsByIds(IEnumerable<string> ids);
    }
}