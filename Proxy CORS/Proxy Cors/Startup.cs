using Owin;
using System.Web.Http;

namespace CorsProxy
{
    class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "CatchAll",
                routeTemplate: "{*url}",
                defaults: new { controller = "Main" });

            appBuilder.UseWebApi(config);


        }
    }
}
