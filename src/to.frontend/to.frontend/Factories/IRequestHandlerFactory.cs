using System;
using to.backlogrepo;
using to.contracts;
using to.requesthandler;
using to.security;
using to.totalorder;
using to.userrepo;
using to.permissionrepo;
using Microsoft.Extensions.Configuration;
using to.contracts.data.result;

namespace to.frontend.Factories
{
    public interface IRequestHandlerFactory
    {
        IRequestHandler GetHandler();
    }

    public class RequestHandlerFactory : IRequestHandlerFactory
    {
        private readonly Lazy<RequestHandler> _handler;

        public RequestHandlerFactory(IConfiguration configuration)
        {
            _handler = new Lazy<RequestHandler>(() =>
            {
                var rootPath = configuration.GetValue<string>("App:DataRootPath");
                var userRepo = new UserRepo(rootPath, "users.json");
                var backlogRepo = new BacklogRepo(rootPath);
                var permissionsRepo = new PermissionRepo(rootPath, "permissions.json");

                if (userRepo.Initialise() is Failure ||
                    backlogRepo.Initialise() is Failure ||
                    permissionsRepo.Initialise() is Failure)
                {
                    throw new FailedRepositoryCreationException();
                }

                return new RequestHandler(backlogRepo, new TotalOrder(), userRepo, new Security(), permissionsRepo);
            });
        }

        public IRequestHandler GetHandler()
        {
            return _handler.Value;
        }
    }

    public class FailedRepositoryCreationException : Exception
    {

    }
}
