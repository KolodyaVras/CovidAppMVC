using Microsoft.Owin;
using Owin;
[assembly: OwinStartup(typeof(CovidAppMVC.Startup))]

namespace CovidAppMVC
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}