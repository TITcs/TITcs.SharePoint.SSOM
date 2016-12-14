using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using TITcs.SharePoint.SSOM.Security;
using TITcs.SharePoint.SSOM.Test.Models;
using TITcs.SharePoint.SSOM.Test.Repositories;
using TITcs.SharePoint.SSOM.Utils;

namespace TITcs.SharePoint.SSOM.Test
{
    [TestClass]
    public class RecursiveQueriesTest
    {
        private string siteurl = "http://conectesp2016.hom.titcs.com.br/";

        [TestMethod]
        public void DeveRetornarTodasAsPastasDaRaiz()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var pastas = _repo.GetAllRecursive(string.Empty);

                Assert.IsTrue(pastas.Count == 12);
            }
        }

        [TestMethod]
        public void DeveRetornarTodosOsItensDaPasta1()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var pastas = _repo.GetAllFromFolder("Pasta 1", string.Empty, (uint)2, string.Empty);

                Assert.IsTrue(pastas != null && pastas.Data != null && pastas.TotalItems == 8);
            }
        }

        [TestMethod]
        public void DeveRetornarTodosOsItensDaPasta1Subpasta10()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var pastas = _repo.GetAllFromFolder("Pasta 1/Subpasta 10", string.Empty, (uint)3, string.Empty);

                Assert.IsTrue(pastas != null && pastas.Data != null && pastas.TotalItems == 0);
            }
        }

        [TestMethod]
        public void DeveRetornarTodosOsItensDaPasta1Subpasta12()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var pastas = _repo.GetAllFromFolder("Pasta 1/Subpasta 12", string.Empty, (uint)10, string.Empty);

                Assert.IsTrue(pastas != null && pastas.Data != null && pastas.TotalItems == 0);
            }
        }

        [TestMethod]
        public void DeveRetornarTodosOsItensDaPasta1Subpasta13()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var pastas = _repo.GetAllFromFolder("Pasta 1/Subpasta 13", string.Empty, (uint)10, string.Empty);

                Assert.IsTrue(pastas != null && pastas.Data != null && pastas.TotalItems == 0);
            }
        }

        [TestMethod]
        public void DeveEncontrarAPasta1()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var pasta = _repo.FindFolder("Pasta 1");

                Assert.IsTrue(pasta != null && pasta.Name == "Pasta 1");
            }
        }

        [TestMethod]
        public void DeveEncontrarASubpasta11()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var pasta = _repo.FindFolder("Pasta 1/Subpasta 11");

                Assert.IsTrue(pasta != null && pasta.Name == "Subpasta 11");
            }
        }

        [TestMethod]
        public void NaoDeveEncontrarAPasta20()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var pasta = _repo.FindFolder("Pasta 20");

                Assert.IsTrue(pasta == null);
            }
        }

        [TestMethod]
        public void TesteDePaginacao()
        {
            //        1        ||       2       ||        3        ||      4
            // Pasta 1, Pasta 5, Pasta 6, Pasta 7, Pasta 8, Pasta 9, Pasta 10


            //<Where><Includes><FieldRef Name=\"PGCSCPublicoAlvoCentralArquivos\" LookupId=\"TRUE\" /><Value Type=\"Integer\">{0}</Value></Includes></Where>
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var pageIndex = 1; // baseado em 1
                var pageSize = 2;
                var publicoAlvoId = "1";
                var lista = web.Lists.TryGetList("Nova Central de Arquivos");
                SPListItemCollection results = default(SPListItemCollection);
                IEnumerable<SPListItem> coll = default(IEnumerable<SPListItem>);
                if (lista != null)
                {
                    var query = new SPQuery();
                    query.Query = string.Format(@"<Where><Includes><FieldRef Name='PGCSCPublicoAlvoCentralArquivos' LookupId='TRUE' /><Value Type='Integer'>{0}</Value></Includes></Where>", publicoAlvoId);

                    results = lista.GetItems(query);

                    coll = results.OfType<SPListItem>().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                }

                var el1 = coll.ElementAt(0);
                var el2 = coll.ElementAt(1);

                Assert.IsTrue(el1.Title == "Pasta 1" && el2.Title == "Pasta 5");
            }
        }

        [TestMethod]
        public void AoConsultarSemInformarPastaEComPublicoAlvoDeveRetornarTodasAsPastasRaiz()
        {
            //   0        1        2        3        4        5         6
            //        1                 ||              2           ||       3
            //      p_ID=1              ||            p_ID=11               p_ID=14
            //       p_ID=1    ||    p_ID=10     ||     p_ID=12    ||   p_ID=14
            //                p_ID=1            ||            p_ID=12
            // Pasta 1, Pasta 5, Pasta 6, Pasta 7, Pasta 8, Pasta 9, Pasta 10
            //<Where><Includes><FieldRef Name=\"PGCSCPublicoAlvoCentralArquivos\" LookupId=\"TRUE\" /><Value Type=\"Integer\">{0}</Value></Includes></Where>
            using (var site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var folder = "Pasta 1";
                var pagingInfo = string.Empty;
                var publicAlvo = "1";
                var pageSize = (uint)3;
                //var camlQuery = string.Empty;
                var camlQuery = string.Format("<Where><Includes><FieldRef Name='PGCSCPublicoAlvoCentralArquivos' LookupId='TRUE' /><Value Type='Integer'>{0}</Value></Includes></Where>", publicAlvo);

                var pasta = _repo.GetAllFromFolder(folder, pagingInfo, pageSize, camlQuery);
                ICollection<PGCSCItem> coll = pasta.Data;

                Assert.IsTrue(pasta != null);
                Assert.IsTrue(coll.ElementAt(0).Title == "Subpasta 11");
                Assert.IsTrue(coll.ElementAt(1).Title == "Subpasta 12");

                //pasta = _repo.GetAllFromFolder(folder, pasta.PagingInfos[2], pageSize, camlQuery);
                //coll = pasta.Data;

                //Assert.IsTrue(pasta != null);
                //Assert.IsTrue(coll.ElementAt(0).Title == "Pasta 3");
                //Assert.IsTrue(coll.ElementAt(1).Title == "Pasta 4");

                //pasta = _repo.GetAllFromFolder(folder, pasta.PagingInfos[3], pageSize, camlQuery);
                //coll = pasta.Data;

                //Assert.IsTrue(pasta != null);
                //Assert.IsTrue(coll.ElementAt(0).Title == "Pasta 5");
                //Assert.IsTrue(coll.ElementAt(1).Title == "Pasta 6");

                //pasta = _repo.GetAllFromFolder(folder, pasta.PagingInfos[4], pageSize, camlQuery);
                //coll = pasta.Data;

                //Assert.IsTrue(pasta != null);
                //Assert.IsTrue(coll.ElementAt(0).Title == "Pasta 7");
                //Assert.IsTrue(coll.ElementAt(1).Title == "Pasta 8");

                //pasta = _repo.GetAllFromFolder(folder, pasta.PagingInfos[5], pageSize, camlQuery);
                //coll = pasta.Data;

                //Assert.IsTrue(pasta != null);
                //Assert.IsTrue(coll.ElementAt(0).Title == "Pasta 9");
                //Assert.IsTrue(coll.ElementAt(1).Title == "Pasta 10");
            }
        }

        [TestMethod]
        public void DeveExecutarComUmaContaDiferente()
        {
            var account = "DMZ\\sp.admin";
            var password = "P@ssw0rd5Dev";

            ImpersonateUser.RunWithAccountAndPassword(() =>
            {
                var currentID = WindowsIdentity.GetCurrent();
                Assert.IsTrue(currentID != null);
            }, account, password);
        }

        [TestMethod]
        public void DeveTestarGetExtension()
        {
            var url = "\\\\DMZ-PGC-SP2016\\Public\\Pasta_1";
            var ext = Path.GetExtension(url);

            Assert.IsTrue(string.IsNullOrWhiteSpace(ext));
        }

        [TestMethod]
        public void DeveRetornarAPasta1()
        {
            using (var site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var caml = string.Format("<Where><Includes><FieldRef Name='PGCSCPessoaGrupo' LookupId='TRUE' /><Value Type='Integer'>{0}</Value></Includes></Where>", "1");

                var lista = web.Lists.TryGetList("Público Alvo");
                if (lista != null)
                {
                    var spQuery = new SPQuery();
                    spQuery.Query = caml;

                    var itens = lista.GetItems(spQuery);
                    Assert.IsNotNull(itens);
                    Assert.IsTrue(itens.Count == 1);
                }
            }
        }

        [TestMethod]
        public void DeveRetornarChaveDeRegistroNula()
        {
            var key = "HKEY_LOCAL_MACHINE\\SOFTWARE\\CAIXASEGURADORA\\CONECTE";
            var registro1 = RegistryUtils.Read(key, "nca:FileServer");
            var registro2 = RegistryUtils.Read(key, "nca:FileServerAccount");
            Assert.IsNotNull(registro1);
            Assert.IsNotNull(registro2);
        }

        [TestMethod]
        public void DeveExecutarComSucesso()
        {
            var key = "HKEY_LOCAL_MACHINE\\SOFTWARE\\CAIXASEGURADORA\\CONECTE";
            var registro1 = RegistryUtils.Read(key, "nca:FileServer");
            var registro2 = RegistryUtils.Read(key, "nca:FileServerAccount");
            Assert.IsNotNull(registro1);
            Assert.IsNotNull(registro2);
        }
    }
}
