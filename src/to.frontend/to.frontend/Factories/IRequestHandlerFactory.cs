namespace to.frontend.Factories
{
    using backlogrepo;

    using contracts;

    using Microsoft.Extensions.Configuration;

    using permissionrepo;

    using requesthandler;

    using security;

    using totalorder;

    using userrepo;

    public interface IRequestHandlerFactory
    {
        IRequestHandler GetHandler();
    }

    public class RequestHandlerFactory : IRequestHandlerFactory
    {
        private readonly IConfiguration configuration;

        public RequestHandlerFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IRequestHandler GetHandler()
        {
            var rootPath = this.configuration.GetValue<string>("App:DataRootPath");
            return new RequestHandler(
                new BacklogRepo(rootPath),
                new TotalOrder(),
                new UserRepo(rootPath, "users.json"),
                new Security(),
                new PermissionRepo(rootPath, "permissions.json"));
        }
    }
}