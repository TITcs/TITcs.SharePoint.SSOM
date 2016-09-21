using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Test
{
    [TestClass]
    public class PagingTest
    {
        private readonly string URL = "http://gcspgc.dev.titcs.com.br/";

        [TestMethod]
        public void Conectar_No_Site()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var sitePages = new SitePagesRepository(web);
                    Assert.IsTrue(!String.IsNullOrEmpty(sitePages.Title));
                }
            }
        }

        [TestMethod]
        public void Retornar_Consulta_Paginada()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var sitePages = new SitePagesRepository(web);
                    var pageddata = sitePages.GetAll(string.Empty, string.Empty);
                    Assert.IsTrue(pageddata is SharePointPagedData<Item>);
                }
            }
        }
    }
}
