namespace to.contracts
{
    using System.Collections.Generic;

    using data.domain;
    using data.result;

    public interface IPermissionRepo
    {
        (Status, IEnumerable<Permission>) LoadPermissions(UserRole role);
    }
}