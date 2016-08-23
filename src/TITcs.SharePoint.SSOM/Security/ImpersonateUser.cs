using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TITcs.SharePoint.SOM.Security
{
    public class ImpersonateUser
    {
        public static void RunWithCurrentContextAndElevatedPrivilegesAndAccountSystem(Action<SPWeb> action)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                var currentSite = SPContext.Current.Site;
                SPUserToken systoken = currentSite.SystemAccount.UserToken;

                using (SPSite site = new SPSite(currentSite.Url, systoken))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        action(web);
                    }
                }

            });
        }

        public static void RunWithElevatedPrivilegesAndAccountSystem(HttpRequest request, Action<SPWeb> action)
        {
            var rootUrl = string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);

            Logger.Logger.Debug("ImpersonateUser.RunWithCurrentRequestAndElevatedPrivilegesAndAccountSystem", "RootUrl: {0}", rootUrl);

            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite currentSite = new SPSite(rootUrl))
                {
                    Logger.Logger.Debug("ImpersonateUser.RunWithCurrentRequestAndElevatedPrivilegesAndAccountSystem", "SystemAccount: {0}", currentSite.SystemAccount.LoginName);

                    SPUserToken systoken = currentSite.SystemAccount.UserToken;
                    using (SPSite site = new SPSite(currentSite.Url, systoken))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            action(web);
                        }
                    }
                }
            });
        }

        public static void RunWithCurrentRequestAndElevatedPrivilegesAndAccountSystem(Action<SPWeb> action)
        {
            var request = HttpContext.Current.Request;
            var rootUrl = string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);

            Logger.Logger.Debug("ImpersonateUser.RunWithCurrentRequestAndElevatedPrivilegesAndAccountSystem", "RootUrl: {0}", rootUrl);

            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite currentSite = new SPSite(rootUrl))
                {
                    Logger.Logger.Debug("ImpersonateUser.RunWithCurrentRequestAndElevatedPrivilegesAndAccountSystem", "SystemAccount: {0}", currentSite.SystemAccount.LoginName);

                    SPUserToken systoken = currentSite.SystemAccount.UserToken;
                    using (SPSite site = new SPSite(currentSite.Url, systoken))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            action(web);
                        }
                    }
                }
            });
        }

        public static void RunWithCurrentRequestAndElevatedPrivilegesAndAccountSystem(Action<SPSite> action)
        {
            var request = HttpContext.Current.Request;
            var rootUrl = string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);

            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite currentSite = new SPSite(rootUrl))
                {
                    SPUserToken systoken = currentSite.SystemAccount.UserToken;
                    using (SPSite site = new SPSite(currentSite.Url, systoken))
                    {
                        action(site);
                    }
                }
            });
        }

        public static void RunWithElevatedPrivilegesAndAccountSystem(SPSite currentSite, Action<SPWeb> action)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                RunWithAccountSystem(currentSite, action);
            });
        }

        public static void RunWithElevatedPrivilegesAndAccountSystem(SPSite currentSite, Action<SPSite> action)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                RunWithAccountSystem(currentSite, action);
            });
        }

        public static void RunWithAccountSystem(SPSite currentSite, Action<SPSite, SPWeb> action)
        {
            SPUserToken systoken = currentSite.SystemAccount.UserToken;
            using (SPSite site = new SPSite(currentSite.Url, systoken))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    action(site, web);
                }
            }
        }

        public static void RunWithAccountSystem(SPSite currentSite, Action<SPSite> action)
        {
            SPUserToken systoken = currentSite.SystemAccount.UserToken;
            using (SPSite site = new SPSite(currentSite.Url, systoken))
            {
                action(site);
            }
        }

        public static void RunWithAccountSystem(SPSite currentSite, Action<SPWeb> action)
        {
            SPUserToken systoken = currentSite.SystemAccount.UserToken;
            using (SPSite site = new SPSite(currentSite.Url, systoken))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    action(web);
                }
            }
        }

        public static void RunWithAccountSystem(Action<SPSite> action)
        {
            var currentSite = SPContext.Current.Site;
            SPUserToken systoken = currentSite.SystemAccount.UserToken;
            using (SPSite site = new SPSite(currentSite.Url, systoken))
            {
                action(site);
            }
        }

        public static void RunWithElevatedPrivilegesAndAccountSystem(Action<SPSite> action)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                RunWithAccountSystem(action);
            });
        }

        public static void RunWithAccountSystem(Action<SPWeb> action)
        {
            var currentSite = SPContext.Current.Site;
            SPUserToken systoken = currentSite.SystemAccount.UserToken;
            using (SPSite site = new SPSite(currentSite.Url, systoken))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    action(web);
                }
            }
        }

        public static void RunWithElevatedPrivilegesAndAccountSystem(Action<SPWeb> action)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                RunWithAccountSystem(action);
            });
        }

        public static void RunWithApplicationPool(Action action)
        {
            var impersonationContext = System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();

            action();

            impersonationContext.Undo();

        }
    }
}
