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
        private readonly string URL = "http://dmz-shs-05/";

        [TestMethod]
        public void Deve_Conectar_No_Site()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var repo = new ProjetosRepository(web);
                    Assert.IsTrue(!String.IsNullOrEmpty(repo.Title));
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_Deve_Retornar_Tipo_Paginado()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pagingInfo = string.Empty;
                    var pageSize = (uint)10;
                    var camlQuery = string.Empty;

                    var repo = new ProjetosRepository(web);
                    var data = repo.GetAll(pagingInfo, pageSize, camlQuery);
                    Assert.IsTrue(data is SharePointPagedData<Item>);
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_10_Iteracoes_Crescente_Da_Primeira_Pagina()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pagingInfo = string.Empty;
                    var pageSize = (uint)10;
                    var camlQuery = string.Empty;
                    SharePointPagedData<Item> coll;
                    var resultado = true;

                    var repo = new ProjetosRepository(web);

                    for (int i = 1, j = 10; i <= 10; i++, j--)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.NextPageQuery;
                        if(i == 1)
                        {
                            resultado = resultado && string.Compare(coll.NextPageQuery, string.Format("Paged=TRUE&p_ID={0}", i * pageSize)) == 0;
                            resultado = resultado && string.IsNullOrEmpty(coll.PreviousPageQuery);
                        }
                        else if(i == 10)
                        {
                            resultado = resultado && string.Compare(coll.PreviousPageQuery, string.Format("Paged=TRUE&PagedPrev=TRUE&p_ID={0}", ((i - 1) * pageSize) + 1)) == 0;
                        }
                        else
                        {
                            resultado = resultado && string.Compare(coll.NextPageQuery, string.Format("Paged=TRUE&p_ID={0}", i * pageSize)) == 0;
                            resultado = resultado && string.Compare(coll.PreviousPageQuery, string.Format("Paged=TRUE&PagedPrev=TRUE&p_ID={0}", ((i - 1) * pageSize) + 1)) == 0;
                        }
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_10_Iteracoes_Decrescente_Da_Ultima_Pagina()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pagingInfo = "Paged=TRUE&PagedPrev=TRUE&p_ID=91";
                    var pageSize = (uint)10;
                    var camlQuery = string.Empty;
                    SharePointPagedData<Item> coll;
                    var resultado = true;

                    var repo = new ProjetosRepository(web);

                    for (int i = 10; i >= 2; i--)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.PreviousPageQuery;
                        if (i == 2)
                        {
                            resultado = resultado && string.Compare(coll.NextPageQuery, string.Format("Paged=TRUE&p_ID={0}", (i - 1) * pageSize)) == 0;
                            resultado = resultado && string.IsNullOrEmpty(coll.PreviousPageQuery);
                        }
                        else if (i == 10)
                        {
                            resultado = resultado && string.Compare(coll.PreviousPageQuery, string.Format("Paged=TRUE&PagedPrev=TRUE&p_ID={0}", ((i - 2) * pageSize) + 1)) == 0;
                        }
                        else
                        {
                            resultado = resultado && string.Compare(coll.NextPageQuery, string.Format("Paged=TRUE&p_ID={0}", (i - 1) * pageSize)) == 0;
                            resultado = resultado && string.Compare(coll.PreviousPageQuery, string.Format("Paged=TRUE&PagedPrev=TRUE&p_ID={0}", ((i - 2) * pageSize) + 1)) == 0;
                        }
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_10_Iteracoes_Crescente_Da_Primeira_Pagina_Subtitle_Correto()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pagingInfo = string.Empty;
                    var pageSize = (uint)10;
                    var camlQuery = string.Empty;
                    SharePointPagedData<Item> coll;
                    var resultado = true;

                    var repo = new ProjetosRepository(web);

                    for (int i = 1; i <= 10; i++)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.NextPageQuery;
                        resultado = resultado && string.Compare(coll.CurrentPageSubtitle.ToLower(), string.Format("página {0} de {1}", i, 10)) == 0;
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_10_Iteracoes_Decrescente_Da_Ultima_Pagina_Subtitle_Correto()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pagingInfo = "Paged=TRUE&PagedPrev=TRUE&p_ID=101";
                    var pageSize = (uint)10;
                    var camlQuery = string.Empty;
                    SharePointPagedData<Item> coll;
                    var resultado = true;

                    var repo = new ProjetosRepository(web);

                    for (int i = 10; i >= 1; i--)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.PreviousPageQuery;
                        resultado = resultado && string.Compare(coll.CurrentPageSubtitle.ToLower(), string.Format("página {0} de {1}", i, 10)) == 0;
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_20_Iteracoes_PageSize_5_Crescente_Da_Primeira_Pagina_Subtitle_Correto()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pageCount = 20;
                    var pagingInfo = string.Empty;
                    var pageSize = (uint)5;
                    var camlQuery = string.Empty;
                    SharePointPagedData<Item> coll;
                    var resultado = true;

                    var repo = new ProjetosRepository(web);

                    for (int i = 1; i <= pageCount; i++)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.NextPageQuery;
                        resultado = resultado && string.Compare(coll.CurrentPageSubtitle.ToLower(), string.Format("página {0} de {1}", i, pageCount)) == 0;
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_20_Iteracoes_PageSize_5_Decrescente_Da_Ultima_Pagina_Subtitle_Correto()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pageCount = 20;
                    var pagingInfo = "Paged=TRUE&PagedPrev=TRUE&p_ID=101";
                    var pageSize = (uint)5;
                    var camlQuery = string.Empty;
                    SharePointPagedData<Item> coll;
                    var resultado = true;

                    var repo = new ProjetosRepository(web);

                    for (int i = pageCount; i >= 1; i--)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.PreviousPageQuery;
                        resultado = resultado && string.Compare(coll.CurrentPageSubtitle.ToLower(), string.Format("página {0} de {1}", i, pageCount)) == 0;
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
    }
}
