using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DontPanic.Admin.Startup))]
namespace DontPanic.Admin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
