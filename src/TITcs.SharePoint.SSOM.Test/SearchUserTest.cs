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
                new PrincipalContext(ContextType.Domain) 
            });

            var user = search.GetUser("stiven.camara");

            Assert.IsTrue(user != null);
        }

        [TestMethod]
        public void GetUserByDisplayName()
        {
            var search = new Search(new[]
            {
                new PrincipalContext(ContextType.Domain)
            });

            var user = search.GetUserByDisplayName("Raul Fuentes");

            Assert.IsTrue(user != null);
        }

        [TestMethod]
        public void GetGroup()
        {
            var search = new Search(new[]
            {
                new PrincipalContext(ContextType.Domain)
            });

            var group = search.GetGroup("Administrators");

            Assert.IsTrue(group != null);
        }
    }
}
