using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;

namespace DrawingBoard
{
    public class Startup
    {
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
