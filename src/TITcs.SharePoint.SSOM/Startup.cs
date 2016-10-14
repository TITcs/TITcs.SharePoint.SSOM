using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using TITcs.SharePoint.SSOM.Utils;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using TITcs.SharePoint.SSOM.Services;
using System.Reflection;
using System.Configuration;

[assembly: OwinStartup(typeof(TITcs.SharePoint.SSOM.Startup))]
namespace TITcs.SharePoint.SSOM
{
    public class Startup
    {
        #region fields and properties

        public const string PERSISTENT_CONNECTION_FULLNAME = "TITcs.SharePoint.SSOM.Services.ServiceConnection";
        public const string HUB_FULLNAME = "TITcs.SharePoint.SSOM.Services.ServiceHub";

        #endregion

        public void Configuration(IAppBuilder app)
        {
            // map SignalR hubs and connections
            ConfigureSignalR(app);
        }

        #region events and methods

        public void ConfigureSignalR(IAppBuilder app)
        {
            try
            {
                var signalrPath = ConfigurationManager.AppSettings[AppSettingsUtils.SIGNALR_PATH];
                var hubConfig = new HubConfiguration() { EnableDetailedErrors = true };
                if (!string.IsNullOrEmpty(signalrPath))
                {
                    app.MapSignalR(signalrPath, hubConfig);
                }
                else
                {
                    app.MapSignalR(hubConfig);
                }

                // set assembly locator
                GlobalHost.DependencyResolver.Register(typeof(IAssemblyLocator), () => new DefaultAssemblyLocator());

                // log end
                Logger.Logger.Information("Startup.ConfigureSignalR", "End");
            }
            catch (Exception ex)
            {
                Logger.Logger.Unexpected("Startup.ConfigureSignalR", ex.Message);
#if DEBUG
                if(ex.InnerException != null)
                    Logger.Logger.Unexpected("Startup.ConfigureSignalR", string.Format("InnerException: {0}", ex.InnerException.Message));
#endif
                throw ex;
            }
        }

        #endregion
    }
}
