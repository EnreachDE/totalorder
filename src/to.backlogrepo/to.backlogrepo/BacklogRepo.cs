// some .cs file included in your project
using System.Runtime.CompilerServices;
using to.contracts.data.domain;

[assembly: InternalsVisibleTo("to.backlogrepo.test")]

namespace to.backlogrepo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
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

        public string CreateBacklog(Backlog backlog)
        {
            var id = guidGenerator().ToString();
            SaveBacklog(backlog, id);
            return id;
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

        public Submission[] ReadSubmissions(string id)
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

            return tempList.ToArray();
        }

        public Backlog ReadBacklog(string id)
        {
            var backlogDirectory = Path.Combine(this.rootpath, id);
            var jsonString = File.ReadAllText(Path.Combine(backlogDirectory,"Backlog.json"));
            return JsonConvert.DeserializeObject<Backlog>(jsonString);
        }

        public void WriteSubmission(string id, Submission submission)
        {
            var submissionPath = Path.Combine(this.rootpath, id);
            var fileName = new StringBuilder();
            fileName.Append("Submission-");
            fileName.Append(guidGenerator().ToString());
            fileName.Append(".json");
            var jsonString = JsonConvert.SerializeObject(submission);
            File.WriteAllText(Path.Combine(submissionPath, fileName.ToString()), jsonString);
        }

        public List<Backlog> GetAll()
        {
            var backlogs = new List<Backlog>();

            var backlogDirs = Directory.GetDirectories(this.rootpath);
            foreach (var backlogDir in backlogDirs)
            {
                var jsonString = File.ReadAllText(Path.Combine(backlogDir, "Backlog.json"));
                backlogs.Add(JsonConvert.DeserializeObject<Backlog>(jsonString));
            }

            return backlogs;
        }

        public void DeleteBacklog(string id)
        {
            string path = Path.Combine(this.rootpath, id);
            Directory.Delete(path, true);
        }

        public List<Backlog> GetUserBacklogs(int userId)
        {
            return new List<Backlog>();
        }
    }
}
