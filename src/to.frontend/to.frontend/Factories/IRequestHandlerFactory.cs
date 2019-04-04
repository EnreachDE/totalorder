using to.backlogrepo;
using to.contracts;
using to.requesthandler;
using to.security;
using to.totalorder;
using to.userrepo;
using to.permissionrepo;
using Microsoft.Extensions.Configuration;

namespace to.frontend.Factories
{
    public interface IRequestHandlerFactory
    {
        IRequestHandler GetHandler();
    }

    public class RequestHandlerFactory : IRequestHandlerFactory
    {
        private IConfiguration _configuration;
        
        public RequestHandlerFactory(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        public IRequestHandler GetHandler() {
            string rootPath = this._configuration.GetValue<string>("App:DataRootPath");
            return new RequestHandler(
                new BacklogRepo(rootPath),
                new TotalOrder(),
                new UserRepo(rootPath, "users.json"),
                new Security(),
                new PermissionRepo(rootPath, "permissions.json"));
        }
    }
}
