using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration.Claims;

//https://blog.mastykarz.nl/programmatically-converting-login-name-claim/

namespace TITcs.SharePoint.SSOM.Utils
{
    public class UserUtils
    {
        public static string GetClaims(string account)
        {
            string userName = null;
            SPClaimProviderManager mgr = SPClaimProviderManager.Local;
            if (mgr != null)
            {
                SPClaim claim = new SPClaim(SPClaimTypes.UserLogonName, account, "http://www.w3.org/2001/XMLSchema#string", SPOriginalIssuers.Format(SPOriginalIssuerType.Windows));
                userName = mgr.EncodeClaim(claim);
            }

            return userName;
            ;
        }

        public static SPUserToken GetToken(string login)
        {
            SPClaimProviderManager claimProviderManager = SPClaimProviderManager.Local;

            SPClaim claim = new SPClaim(SPClaimTypes.UserLogonName, login, "http://www.w3.org/2001/XMLSchema#string", SPOriginalIssuers.Format(SPOriginalIssuerType.Windows));
            var userName = claimProviderManager.EncodeClaim(claim);

            return SPContext.Current.Web.EnsureUser(userName).UserToken;
        }
    }
}
