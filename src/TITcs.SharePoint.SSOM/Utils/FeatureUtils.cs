using System;
using System.Linq;
using Microsoft.SharePoint;

namespace TITcs.SharePoint.SSOM.Utils
{
    public static class FeatureUtils
    {
        /// <summary>
        /// SharePoint Server Publishing Infrastructure
        /// </summary>
        /// <param name="site">SPSite</param>
        public static void ActivePublishInfrastructure(SPSite site)
        {
            Active(site, "f6924d36-2fa8-4f0b-b16d-06b7250180fa");
        }

        /// <summary>
        /// SharePoint Server Publishing
        /// </summary>
        /// <param name="site"></param>
        /// <param name="allWebsites"></param>
        public static void ActivePublishing(SPSite site, bool allWebsites = false)
        {
            ActivePublishInfrastructure(site);//Pré-requisito

            var guid = "94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb";

            Active(site, guid);

            if (allWebsites)
                foreach (SPWeb web in site.AllWebs)
                {
                    Active(web, guid);
                }
        }

        public static void Active(SPSite site, string guid)
        {
            Guid gui = new Guid(guid);

            if (site.Features.Cast<SPFeature>().Any(f => f.DefinitionId == gui))
                site.Features.Add(gui, true);
        }

        public static void Active(SPWeb web, string guid)
        {
            Guid gui = new Guid(guid);

            if (web.Features.Cast<SPFeature>().Any(f => f.DefinitionId == gui))
                web.Features.Add(gui, true);
        }
    }
}
