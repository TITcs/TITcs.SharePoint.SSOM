using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TITcs.SharePoint.SSOM.Security;

namespace TITcs.SharePoint.SSOM.Security
{
    /// <summary>
    /// Utility class to make impersonated calls to the SharePoint web application
    /// </summary>
    public class ImpersonateUser
    {
        /// <summary>
        /// Executes the delegate with Full Control rights even if the user does not otherwise have Full Control
        /// </summary>
        /// <param name="action">Action to execute under Full Control rights</param>
        public static void RunWithCurrentContextAndElevatedPrivilegesAndAccountSystem(Action<SPWeb> action)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                var currentSite = SPContext.Current.Site;
                var systoken = currentSite.SystemAccount.UserToken;
                using (var site = new SPSite(currentSite.Url, systoken))
                {
                    using (var web = site.OpenWeb())
                    {
                        action(web);
                    }
                }
            });
        }

        public static void RunWithElevatedPrivilegesAndAccountSystem(HttpRequest request, Action<SPWeb> action)
        {
            var rootUrl = $"{request.Url.Scheme}://{request.Url.Authority}";

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
            var rootUrl = $"{request.Url.Scheme}://{request.Url.Authority}";

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
            var rootUrl = $"{request.Url.Scheme}://{request.Url.Authority}";

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

        /// <summary>
        /// Executes the delegate with the rights of the specified account
        /// </summary>
        /// <param name="action">Action to execute under the user rights</param>
        /// <param name="account">Account to execute. Must be in the form of DOMAIN\USER </param>
        /// <param name="password">Password of the user</param>
        public static void RunWithAccountAndPassword(Action action, string account, string password)
        {
            // safe checks
            if (action == null) throw new ArgumentNullException("action");
            if (string.IsNullOrWhiteSpace(account)) throw new ArgumentNullException("account");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException("password");

            var split = account.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            if (split == null || split.Length != 2) throw new ArgumentException("Account parameter is not in the DOMAIN\\USERNAME format");

            var domain = split[0];
            var username = split[1];

            using (var windowsImpContext = new WindowsIdentityImpersonator(domain, username, password))
            {
                windowsImpContext.BeginImpersonate();

                action();

                windowsImpContext.EndImpersonate();
            }
        }
    }
}
