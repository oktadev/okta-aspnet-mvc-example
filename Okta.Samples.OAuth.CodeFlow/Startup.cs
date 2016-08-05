using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Okta.Samples.OAuth.CodeFlow.Startup))]

namespace Okta.Samples.OAuth.CodeFlow
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
