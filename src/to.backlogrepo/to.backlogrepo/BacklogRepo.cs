// some .cs file included in your project

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("to.backlogrepo.test")]

namespace to.backlogrepo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using contracts;
    using contracts.data.domain;

    using Newtonsoft.Json;

    public class BacklogRepo : IBacklogRepo
    {
        private const string BacklogsSubFolder = "Backlogs";

        private readonly Func<Guid> guidGenerator;
        private readonly string rootPath;

        public BacklogRepo()
        {
            this.rootPath = Path.Combine(Environment.CurrentDirectory, BacklogsSubFolder);
            this.guidGenerator = Guid.NewGuid;
        }

        public BacklogRepo(string rootPath)
        {
            this.rootPath = Path.Combine(rootPath, BacklogsSubFolder);
            this.guidGenerator = Guid.NewGuid;
        }

        internal BacklogRepo(string rootPath, Func<Guid> guidGenerator)
        {
            this.rootPath = Path.Combine(rootPath, BacklogsSubFolder);
            this.guidGenerator = guidGenerator;
        }

        public string CreateBacklog(Backlog backlog)
        {
            var id = this.guidGenerator().ToString();
            SaveBacklog(backlog, id);
            return id;
        }

        public Submission[] ReadSubmissions(string id)
        {
            var filePaths = Directory.GetFiles(Path.Combine(this.rootPath, id));
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

            return tempList.ToArray();
        }

        public Backlog ReadBacklog(string id)
        {
            var backlogDirectory = Path.Combine(this.rootPath, id);
            var jsonString = File.ReadAllText(Path.Combine(backlogDirectory, "Backlog.json"));
            return JsonConvert.DeserializeObject<Backlog>(jsonString);
        }

        public void WriteSubmission(string id, Submission submission)
        {
            var submissionPath = Path.Combine(this.rootPath, id);
            var fileName = new StringBuilder();
            fileName.Append("Submission-");
            fileName.Append(this.guidGenerator().ToString());
            fileName.Append(".json");
            var jsonString = JsonConvert.SerializeObject(submission);
            File.WriteAllText(Path.Combine(submissionPath, fileName.ToString()), jsonString);
        }

        public List<Backlog> GetAll()
        {
            var backlogs = new List<Backlog>();

            var backlogDirs = Directory.GetDirectories(this.rootPath);
            foreach (var backlogDir in backlogDirs)
            {
                var jsonString = File.ReadAllText(Path.Combine(backlogDir, "Backlog.json"));
                backlogs.Add(JsonConvert.DeserializeObject<Backlog>(jsonString));
            }

            return backlogs;
        }

        public void DeleteBacklog(string id)
        {
            var path = Path.Combine(this.rootPath, id);
            Directory.Delete(path, true);
        }

        public List<Backlog> GetBacklogsByIds(IEnumerable<string> ids)
        {
            return ids.Select(ReadBacklog).ToList();
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
            var backlogPath = Path.Combine(this.rootPath, id);
            Directory.CreateDirectory(backlogPath);
            return backlogPath;
        }
    }
}