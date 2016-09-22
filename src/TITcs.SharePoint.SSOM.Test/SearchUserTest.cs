using System;
using System.Text;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TITcs.SharePoint.SSOM.ActiveDirectory;

namespace TITcs.SharePoint.SSOM.Test
{
    /// <summary>
    /// Summary description for SearchUser
    /// </summary>
    [TestClass]
    public class SearchUserTest
    {
        [TestMethod]
        public void GetUser()
        {
            var search = new Search(new []
            {
                new PrincipalContext(ContextType.Domain, "df.titcs.local", "OU=Funcionários,OU=Usuários,DC=df,DC=titcs,DC=local", "devsp.admin", "P@ssw0rd4Dev") 
            });


            var user = search.GetUser("stiven.camara");

            Assert.IsTrue(user != null);
        }
    }
}
