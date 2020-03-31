using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(JC.Site.Startup))]
namespace JC.Site
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}