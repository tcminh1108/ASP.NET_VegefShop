using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Web_RauCu.Startup))]
namespace Web_RauCu
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
