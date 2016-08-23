using System;

using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TITcs.SharePoint.SOM.Test
{
    [TestClass]
    public class SitePagesTest
    {
        [TestMethod]
        public void Connect()
        {
            using (SPSite site = new SPSite("http://captime.dev.titcs.com.br"))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var sitePages = new SitePagesRepository(web);

                    sitePages.RowLimit = 2;

                    var item = sitePages.GetAll();

                    //var item1 = sitePages.GetAll();

                    //sitePages.LastPosition = null;

                    //var item2 = sitePages.GetAll();

                    Assert.IsTrue(item.Count > 0);
                }
            }



        }
    }
}
