using System.Collections.Generic;
using to.contracts.data.domain;

namespace to.contracts
{
    public interface IBacklogRepo
    {
        string CreateBacklog(Backlog backlog);
        Submission[] ReadSubmissions(string id);
        Backlog ReadBacklog(string id);
        void WriteSubmission(string id, Submission submission);
        List<Backlog> GetAll();
        void DeleteBacklog(string id);
    }
}
