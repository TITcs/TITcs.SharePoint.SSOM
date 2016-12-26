using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace TITcs.SharePoint.SSOM.Test
{
    [TestClass]
    public class MelhoresPraticasNotificacaoTest
    {
        private string siteurl = "http://conecte2016.hom.titcs.com.br/";


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

        [TestMethod]
        public void ColunaHistoricoCurtidasPermiteMultiplosValroes()
        {
            using (var site = new SPSite(siteurl))
            {
                using (var web = site.OpenWeb())
                {
                    var melhoresPraticasNotificacao = web.Lists.TryGetList("Melhores Práticas Notificação");
                    if (melhoresPraticasNotificacao != null)
                    {
                        var historicoCurtidas = melhoresPraticasNotificacao.Fields.GetFieldByInternalName("HistoricoCurtidas") as SPFieldLookup;
                        if (historicoCurtidas != null)
                        {
                            Assert.IsTrue(historicoCurtidas.AllowMultipleValues);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void NotificacaoDoSegundoTesteDoDia()
        {
            using (var site = new SPSite(siteurl))
            {
                using (var web = site.OpenWeb())
                {
                    var melhoresPraticasNotificacao = web.Lists.TryGetList("Melhores Práticas Notificação");
                    if (melhoresPraticasNotificacao != null)
                    {
                        var caml = string.Format("<Where><And><Eq><FieldRef Name=\"Title\"/><Value Type=\"Text\">{0}</Value></Eq><Eq><FieldRef Name=\"Tipo\"/><Value Type=\"Choice\">{1}</Value></Eq></And></Where>", "Terceira Melhor Prática de Sábado", "Curtida");
                        var segundoTeste = melhoresPraticasNotificacao.GetItems(new SPQuery { Query = caml }).Cast<SPListItem>().FirstOrDefault();
                        if (segundoTeste != null)
                        {
                            var curtidas = segundoTeste["HistoricoCurtidas"] as SPFieldUserValueCollection;
                            if (curtidas != null)
                            {
                                var antigoCount = curtidas.Count;

                                // adicionar uma curtida e salvar
                                curtidas.Add(new SPFieldUserValue(web, 30, "sp.admin")); // ldap sp.admin 30
                                segundoTeste["HistoricoCurtidas"] = curtidas;
                                segundoTeste.Update();

                                var terceiroTeste = melhoresPraticasNotificacao.GetItems(new SPQuery { Query = caml }).Cast<SPListItem>().FirstOrDefault();
                                if (terceiroTeste != null)
                                {
                                    var novasCurtidas = segundoTeste["HistoricoCurtidas"] as SPFieldUserValueCollection;
                                    if (novasCurtidas != null)
                                    {
                                        var novoCount = novasCurtidas.Count;

                                        Assert.IsTrue(antigoCount < novoCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
