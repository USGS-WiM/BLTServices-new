using System;
using System.Linq;
using System.Security.Principal;
using OpenRasta.DI;
using OpenRasta.Security;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace BLTServices.Authentication
{
    public class BLTAuthorizerContributer:IPipelineContributor
    {
        private readonly IDependencyResolver _resolver;
        private IAuthenticationProvider _authentication;

        public BLTAuthorizerContributer(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public void Initialize(IPipeline pipelineRunner)
        {
            _authentication = _resolver.Resolve<IAuthenticationProvider>();
            pipelineRunner.Notify(ReadCredentials)
                .After<KnownStages.IBegin>()
                .And
                .Before<KnownStages.IHandlerSelection>();
        }

        public PipelineContinuation ReadCredentials(ICommunicationContext context)
        {
            if (_resolver.HasDependency(typeof(IAuthenticationProvider)))
            {
                _authentication = _resolver.Resolve<IAuthenticationProvider>();
                var header = ReadBasicAuthHeader(context);
                if (header != null)
                {
                    var credentials = _authentication.GetByUsername(header.Username);

                    if (_authentication.ValidatePassword(credentials, header.Password))
                    {
                        IIdentity id = new GenericIdentity(credentials.Username, "Basic");
                        context.User = new GenericPrincipal(id, credentials.Roles);
                    }
                }
            }

            return PipelineContinuation.Continue;
        }

        private static BasicAuthorizationHeader ReadBasicAuthHeader(ICommunicationContext context)
        {
            try
            {
                var header = context.Request.Headers["Authorization"];
                return string.IsNullOrEmpty(header) ? null : BasicAuthorizationHeader.Parse(header);
            } catch (ArgumentException ex)
            {
                return (null);
            }
        }
    }
}