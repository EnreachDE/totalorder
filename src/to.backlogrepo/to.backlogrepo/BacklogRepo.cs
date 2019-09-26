// some .cs file included in your project

using System.Linq;
using System.Runtime.CompilerServices;
using to.contracts.data.domain;

[assembly: InternalsVisibleTo("to.backlogrepo.test")]

namespace to.backlogrepo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using contracts.data.result;

    using to.contracts;
    using Newtonsoft.Json;

    public class BacklogRepo : IBacklogRepo
    {
        private readonly string rootpath;

        private readonly Func<Guid> guidGenerator;
        private const string _backlogsSubFolder = "Backlogs";

        public BacklogRepo()
        {
            this.rootpath = Path.Combine(Environment.CurrentDirectory, _backlogsSubFolder);
            var x = new Random();
            this.guidGenerator = Guid.NewGuid;
        }

        public BacklogRepo(string rootpath)
        {
            this.rootpath = Path.Combine(rootpath, _backlogsSubFolder);
            var x = new Random();
            this.guidGenerator = Guid.NewGuid;
        }

        internal BacklogRepo(string rootpath, Func<Guid> guidGenerator)
        {
            this.rootpath = Path.Combine(rootpath, _backlogsSubFolder);
            this.guidGenerator = guidGenerator;
        }

        public (Status, string) CreateBacklog(Backlog backlog)
        {
            var id = guidGenerator().ToString();
            SaveBacklog(backlog, id);
            return  (new Success(), id);
        }

        internal void SaveBacklog(Backlog backlog, string id)
        {
            backlog.Id = id;
            var jsonString = JsonConvert.SerializeObject(backlog);
            var backlogDirectory = CreateBacklogDirectory(id);
            File.WriteAllText(Path.Combine(backlogDirectory, "Backlog.json"), jsonString);
        }

        private string CreateBacklogDirectory(string id)
        {
            var backlogPath = Path.Combine(this.rootpath, id);
            Directory.CreateDirectory(backlogPath);
            return backlogPath;
        }

        public (Status, Submission[]) ReadSubmissions(string id)
        {
            var filePaths = Directory.GetFiles(Path.Combine(this.rootpath, id));
            var tempList = new List<Submission>();

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                if (!fileName.StartsWith("submission", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var jsonString = File.ReadAllText(filePath);
                var currentSubmission = JsonConvert.DeserializeObject<Submission>(jsonString);
                tempList.Add(currentSubmission);
            }

            return (new Success(), tempList.ToArray());
        }

        public (Status, Backlog) ReadBacklog(string id)
        {
            var backlogDirectory = Path.Combine(this.rootpath, id);
            var jsonString = File.ReadAllText(Path.Combine(backlogDirectory,"Backlog.json"));
            return (new Success(), JsonConvert.DeserializeObject<Backlog>(jsonString));
        }

        public Status WriteSubmission(string backlogId, Submission submission)
        {
            var (status, backlog) = this.ReadBacklog(backlogId);
            if (status is Failure) return status;

            return CheckBacklogTypeAndWriteSubmission(submission, backlog);
        }

        internal Status CheckBacklogTypeAndWriteSubmission(Submission submission, Backlog backlog)
        {
            bool userIdPresent = submission.UserId != null;
            bool authenticationRequired = backlog.OneVotePerUser;
            bool authenticationFailure = authenticationRequired && !userIdPresent;

            if (authenticationFailure)
            {
                return new Failure("Submissions to authenticated Backlogs require a UserId!");
            }

            var submissionPath = Path.Combine(this.rootpath, backlog.Id);

            var fileName = new StringBuilder();
            fileName.Append("Submission-");
            fileName.Append(authenticationRequired ? submission.UserId.Value.ToString() : guidGenerator().ToString());
            fileName.Append(".json");

            var jsonString = JsonConvert.SerializeObject(submission);

            var path = Path.Combine(submissionPath, fileName.ToString());
            File.WriteAllText(path, jsonString);

            return new Success();
        }

        public (Status, List<Backlog>) GetAll()
        {
            var backlogs = new List<Backlog>();

            var backlogDirs = Directory.GetDirectories(this.rootpath);
            foreach (var backlogDir in backlogDirs)
            {
                var jsonString = File.ReadAllText(Path.Combine(backlogDir, "Backlog.json"));
                backlogs.Add(JsonConvert.DeserializeObject<Backlog>(jsonString));
            }

            return (new Success(), backlogs);
        }

        public Status DeleteBacklog(string id)
        {
            string path = Path.Combine(this.rootpath, id);

            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception e) when (e is IOException || 
                                      e is UnauthorizedAccessException ||
                                      e is ArgumentNullException ||
                                      e is ArgumentException)
            {
                return new Failure(e.Message);
            }

            return new Success();
        }

        public (Status, List<Backlog>) GetBacklogsByIds(IEnumerable<string> ids)
        {
            var backlogs = new List<Backlog>();

            foreach (var id in ids)
            {
                var (status, backlog) = this.ReadBacklog(id);
                if (status is Failure) return (status, null);

                backlogs.Add(backlog);
            }
            
            return (new Success(), backlogs);
        }

        public Status Initialise()
        {
            if (!Directory.Exists(this.rootpath))
            {
                Directory.CreateDirectory(this.rootpath);
            }
            return new Success();
        }
    }
}
