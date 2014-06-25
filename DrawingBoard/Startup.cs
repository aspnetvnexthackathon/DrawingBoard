using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;

namespace DrawingBoard
{
    public class Startup
    {
        internal static string AppBasePath;

        public Startup(IApplicationEnvironment env)
        {
            AppBasePath = env.ApplicationBasePath;
        }

        public void Configure(IBuilder app)
        {
            app.UseServices(services =>
            {
                services.AddSignalR().SetupOptions(options =>
                {
                    options.Hubs.EnableDetailedErrors = true;
                });
            });

            app.UseSignalR();
            app.UseFileServer();
        }
    }
}
