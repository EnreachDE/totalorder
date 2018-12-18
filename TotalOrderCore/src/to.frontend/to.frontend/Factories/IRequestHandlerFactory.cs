using to.backlogrepo;
using to.contracts;
using to.requesthandler;
using to.security;
using to.totalorder;
using to.userrepo;
using to.permissionrepo;

namespace to.frontend.Factories
{
    public interface IRequestHandlerFactory
    {
        IRequestHandler GetHandler();
    }

    public class RequestHandlerFactory : IRequestHandlerFactory
    {
        public IRequestHandler GetHandler()
            => new RequestHandler(
                new BacklogRepo(@"/TotalOrder"),
                new TotalOrder(),
                new UserRepo(@"/TotalOrder", "users.json"),
                new Security(),
                new PermissionRepo(@"/TotalOrder", "permissions.json"));
    }
}
