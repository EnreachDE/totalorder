namespace to.contracts
{
    using System.Collections.Generic;

    using data.domain;
    using data.result;

    public interface IBacklogRepo
    {
        (Status, string) CreateBacklog(Backlog backlog);
        (Status, Submission[]) ReadSubmissions(string id);
        (Status, Backlog) ReadBacklog(string id);
        Status WriteSubmission(string backlogId, Submission submission);
        (Status, List<Backlog>) GetAll();
        Status DeleteBacklog(string id);
        (Status, List<Backlog>) GetBacklogsByIds(IEnumerable<string> ids);
        Status Initialise();
    }
}