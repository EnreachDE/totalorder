using System.Collections.Generic;
using to.contracts.data.domain;
using to.contracts.data.result;

namespace to.contracts
{
    public interface IPermissionRepo
    {
        (Status, IEnumerable<Permission>) LoadPermissions(UserRole role);
        Status Initialise();
    }
}
