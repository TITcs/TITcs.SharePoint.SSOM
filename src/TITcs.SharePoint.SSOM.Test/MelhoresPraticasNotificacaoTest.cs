using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TITcs.SharePoint.SSOM.Test
{
    [TestClass]
    public class MelhoresPraticasNotificacaoTest
    {
        private string siteurl = "http://pgc.214.dev/";


        [TestMethod]
        public void Testar()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new MelhoresPraticasNotificacaoRepository(web);
                var item = _repo.GetById(1);
                var item2 = _repo.GetById(2);

                Assert.IsTrue(item2.HistoricoCurtidas.OfType<Lookup>().ToList<Lookup>().Any(l => l.Id == 1));
            }
        }

        [TestMethod]
        public void Testar_Campo_Historico_Curtidas()
        {
            using (SPSite site = new SPSite(siteurl))
            {
                var web = site.OpenWeb();
                var _repo = new MelhoresPraticasNotificacaoRepository(web);
                MelhoresPraticasNotificacaoItem item = _repo.GetById(7);

                item.HistoricoCurtidas.Add(new Lookup(21, "Stiven Câmara"));

                _repo.Update(item);

                Assert.IsTrue(true);
            }
        }
    }
}
