using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using to.contracts;
using to.contracts.data.domain;
using to.contracts.data.result;

namespace to.permissionrepo
{
    public class PermissionRepo : IPermissionRepo
    {
        private readonly string _rootpath;
        private readonly string _fileName = "permissions.json";

        public PermissionRepo()
        {
            this._rootpath = Environment.CurrentDirectory;
        }

        public PermissionRepo(string rootpath, string fileName)
        {
            _rootpath = rootpath;
            _fileName = fileName;
        }

        public (Status, IEnumerable<Permission>) LoadPermissions(UserRole role)
        {
            var dict = ReadPermissionDictionary();
            return RetrieveRolePermissions(role, dict);
        }

        public Status Initialise()
        {
            var exists = File.Exists(Path.Combine(_rootpath, _fileName));

            if (!exists)
            {
                var dict = CreateDefaultPermissionDictionary();
                SerializeAndWriteFile(dict);
            }

            return new Success();
        }

        private void SerializeAndWriteFile(Dictionary<UserRole, List<Permission>> dict)
        {
            var serializedDict = JsonConvert.SerializeObject(dict);
            var path = Path.Combine(_rootpath, _fileName);
            File.WriteAllLines(path, new[] {serializedDict});
        }

        private static Dictionary<UserRole, List<Permission>> CreateDefaultPermissionDictionary()
        {
            var dict = new Dictionary<UserRole, List<Permission>>();

            dict[UserRole.Administrator] = new List<Permission>
            {
                Permission.CreateBacklog,
                Permission.CreateUser,
                Permission.DeleteBacklog,
                Permission.DeleteUser,
                Permission.EditUser,
                Permission.EvaluateBacklog,
                Permission.ListBacklog,
                Permission.ListUser,
                Permission.OrderBacklog
            };

            dict[UserRole.Guest] = new List<Permission>
            {
                Permission.ListBacklog,
            };

            dict[UserRole.ProductOwner] = new List<Permission>
            {
                Permission.CreateBacklog,
                Permission.DeleteBacklog,
                Permission.EvaluateBacklog,
                Permission.ListBacklog,
            };

            dict[UserRole.Developer] = new List<Permission>
            {
                Permission.EvaluateBacklog,
                Permission.ListBacklog,
                Permission.OrderBacklog
            };
            return dict;
        }

        private static (Status, IEnumerable<Permission>) RetrieveRolePermissions(UserRole role, Dictionary<UserRole, List<Permission>> dict)
        {
            dict.TryGetValue(role, out var permissions);
            if (permissions != null)
            {
                return (new Success(), permissions);
            }

            return (new Failure($"Not able to retrieve permissions for role {role}."), null);
        }

        private Dictionary<UserRole, List<Permission>> ReadPermissionDictionary()
        {
            var jsonString = File.ReadAllText(Path.Combine(this._rootpath, _fileName));
            return JsonConvert.DeserializeObject<Dictionary<UserRole, List<Permission>>>(jsonString);
        }

    }
}
