namespace to.permissionrepo
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using contracts;
    using contracts.data.domain;
    using contracts.data.result;

    using Newtonsoft.Json;

    public class PermissionRepo : IPermissionRepo
    {
        private readonly string fileName = "permissions.json";
        private readonly string rootPath;

        public PermissionRepo()
        {
            this.rootPath = Environment.CurrentDirectory;
        }

        public PermissionRepo(string rootPath, string fileName)
        {
            this.rootPath = rootPath;
            this.fileName = fileName;
        }

        public (Status, IEnumerable<Permission>) LoadPermissions(UserRole role)
        {
            var dict = ReadPermissionDictionary();
            return RetrieveRolePermissions(role, dict);
        }

        private static (Status, IEnumerable<Permission>) RetrieveRolePermissions(UserRole role,
            Dictionary<UserRole, List<Permission>> dict)
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
            var jsonString = File.ReadAllText(Path.Combine(this.rootPath, this.fileName));
            return JsonConvert.DeserializeObject<Dictionary<UserRole, List<Permission>>>(jsonString);
        }
    }
}