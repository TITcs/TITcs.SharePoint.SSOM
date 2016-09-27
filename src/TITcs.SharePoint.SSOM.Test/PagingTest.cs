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
        private readonly string[] randomStrings = new string[] {
            @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam sed nulla nisi. Sed ut tristique nulla, at faucibus elit. Nulla et purus erat. Nullam condimentum luctus sapien, non pretium magna sollicitudin ut. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Mauris porttitor vitae lacus at semper. Suspendisse potenti. Morbi ut lectus consequat, imperdiet nunc nec, feugiat massa. Morbi condimentum leo quis sapien mattis, eget consequat nunc dapibus.",
            @"Curabitur nec ex viverra, volutpat enim sit amet, rutrum ante. Morbi dui nisi, pellentesque eget risus eu, ornare aliquam diam. Pellentesque non dui ut dui placerat hendrerit ornare a quam. Cras aliquet, mi ultricies sodales dapibus, libero metus volutpat mi, eget tristique nulla nisi in libero. Proin odio est, vestibulum nec risus eu, egestas varius ex. Donec vitae leo vitae leo commodo pharetra nec quis quam. Sed a tempus turpis.",
            @"Donec commodo porta sapien ac elementum. Integer fringilla in libero nec laoreet. Duis lacinia magna euismod condimentum accumsan. Nunc hendrerit odio vitae ex semper semper. Cras mattis ante nisi, ac porta augue tristique sed. Mauris mattis velit et felis maximus rhoncus. Nunc hendrerit mi pharetra luctus tempus. Integer lobortis est ut lacinia consequat. Vestibulum sed mauris et felis porttitor semper. Duis viverra ipsum eu sem tristique, ac pulvinar ante pretium. Ut mollis est in enim tristique aliquet.",
            @"Suspendisse pharetra ante nec risus interdum ultricies. Vivamus augue diam, lobortis quis augue eu, gravida commodo metus. Donec eleifend sem lorem, sed volutpat lacus posuere finibus. In hac habitasse platea dictumst. Mauris nec sapien lacus. Quisque dictum et libero quis interdum. Nam id volutpat diam, sed ornare nibh. Etiam at congue sapien, nec vestibulum nulla. Mauris tincidunt nibh in arcu feugiat, et lacinia metus vulputate. Suspendisse potenti.",
            @"Cras molestie felis hendrerit ipsum mattis, id imperdiet nisi bibendum. Praesent congue, orci at commodo malesuada, massa leo aliquam odio, non bibendum sapien ipsum sed turpis. Maecenas non interdum urna. Nunc eget magna turpis. Phasellus eu porta nisl, a sollicitudin ex. Praesent id nulla scelerisque, commodo tortor vitae, facilisis lorem. Proin metus risus, consequat a consequat convallis, dictum in lacus. Nam in enim sit amet elit pellentesque volutpat."
        };

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
        //[TestMethod]
        //public void Deve_Criar_100_Registros()
        //{
        //    using (SPSite site = new SPSite(URL))
        //    {
        //        using (SPWeb web = site.OpenWeb())
        //        {
        //            var repo = new ProjetosRepository(web);
        //            var count = repo.GetAll().Count;

        //            Assert.IsTrue(count == 0);
        //            for (int i = 1; i <= 100; i++)
        //            {
        //                var rand = new Random(i);

        //                try
        //                {
        //                    var str = randomStrings[rand.Next(4)];
        //                    var _date = new DateTime(rand.Next(2009, 2016), rand.Next(1, 12), rand.Next(1, 28));
        //                    repo.Add(new Item
        //                    {
        //                        Title = string.Format("Projeto {0}", i),
        //                        Content = randomStrings[rand.Next(4)],
        //                        Created = _date
        //                    });
        //                }
        //                catch (Exception)
        //                {
        //                    throw;
        //                }
        //            }

        //            count = repo.GetAll().Count;
        //            Assert.IsTrue(count == 100);
        //        }
        //    }
        //}
        [TestMethod]
        public void Deve_Retornar_100_Registros()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var repo = new ProjetosRepository(web);
                    Assert.IsTrue(repo.GetAll().Count == 101);
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
                        if (i == 1)
                        {
                            resultado = resultado && string.Compare(coll.NextPageQuery, string.Format("Paged=TRUE&p_ID={0}", i * pageSize)) == 0;
                            resultado = resultado && string.IsNullOrEmpty(coll.PreviousPageQuery);
                        }
                        else if (i == 10)
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

                    for (int i = 1; i <= 11; i++)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.NextPageQuery;
                        resultado = resultado && string.Compare(coll.CurrentPageSubtitle.ToLower(), string.Format("página {0} de {1}", i, 11)) == 0;
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
                    var pagingInfo = "Paged=TRUE&PagedPrev=TRUE&p_ID=107";
                    var pageSize = (uint)10;
                    var camlQuery = string.Empty;
                    SharePointPagedData<Item> coll;
                    var resultado = true;
                    var repo = new ProjetosRepository(web);
                    for (int i = 10; i >= 1; i--)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.PreviousPageQuery;
                        resultado = resultado && string.Compare(coll.CurrentPageSubtitle.ToLower(), string.Format("página {0} de {1}", i, 11)) == 0;
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
                    var pageCount = 21;
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
                    var pageCount = 21;
                    var pagingInfo = "Paged=TRUE&PagedPrev=TRUE&p_ID=106";
                    var pageSize = (uint)5;
                    var camlQuery = string.Empty;
                    SharePointPagedData<Item> coll;
                    var resultado = true;
                    var repo = new ProjetosRepository(web);
                    for (int i = pageCount - 1; i >= 1; i--)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.PreviousPageQuery;
                        resultado = resultado && string.Compare(coll.CurrentPageSubtitle.ToLower(), string.Format("página {0} de {1}", i, pageCount)) == 0;
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_10_Iteracoes_Crescente_Da_Primeira_Pagina_Com_CamlQuery()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pagingInfo = string.Empty;
                    var pageSize = (uint)5;
                    // <OrderBy> FieldRef Name = 'Created' Ascending = 'FALSE' /></ OrderBy >
                    var camlQuery = string.Format(@"
                                      <Where>
                                        <Gt>
                                            <FieldRef Name = 'ID'/>
                                            <Value Type = 'Number'>{0}</Value>
                                        </Gt>
                                      </Where>", 30);
                    SharePointPagedData<Item> coll;
                    var resultado = true;
                    var repo = new ProjetosRepository(web);
                    for (int i = 1; i <= 10; i++)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.NextPageQuery;
                        resultado = resultado && string.Compare(coll.NextPageQuery, string.Format("Paged=TRUE&p_ID={0}", 30 + (i * pageSize))) == 0;
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_10_Iteracoes_Crescente_Da_Primeira_Pagina_Com_CamlQuery_Sorting()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pagingInfo = string.Empty;
                    var pageSize = (uint)5;
                    var camlQuery = string.Format(@"
                                      <OrderBy><FieldRef Name='ID' Ascending='TRUE'/></OrderBy>
                                      <Where>
                                        <Gt>
                                            <FieldRef Name='ID'/>
                                            <Value Type='Number'>{0}</Value>
                                        </Gt>
                                      </Where>", 30);
                    SharePointPagedData<Item> coll;
                    var resultado = true;
                    var repo = new ProjetosRepository(web);
                    for (int i = 1; i <= 10; i++)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.NextPageQuery;
                        resultado = resultado && pagingInfo.Contains("Paged=TRUE") && pagingInfo.Contains(string.Format("p_ID={0}", coll.Data.OfType<Item>().ToList<Item>()[coll.Data.Count - 1].Id));
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
        [TestMethod]
        public void Consulta_Paginada_10_Iteracoes_Crescente_Da_Primeira_Pagina_Subtitle_Correto_Com_CamlQuery_Sorting()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var pagingInfo = string.Empty;
                    var pageSize = (uint)5;
                    var camlQuery = string.Format(@"
                                      <OrderBy><FieldRef Name='ID' Ascending='FALSE' /></OrderBy>
                                      <Where>
                                        <Gt>
                                            <FieldRef Name = 'ID'/>
                                            <Value Type='Number'>{0}</Value>
                                        </Gt>
                                      </Where>", 30);
                    SharePointPagedData<Item> coll;
                    var resultado = true;
                    var repo = new ProjetosRepository(web);
                    for (int i = 1; i <= 10; i++)
                    {
                        coll = repo.GetAll(pagingInfo, pageSize, camlQuery);
                        pagingInfo = coll.NextPageQuery;
                        resultado = resultado && string.Compare(coll.CurrentPageSubtitle.ToLower(), string.Format("página {0} de {1}", i, 15)) == 0;
                    }
                    Assert.IsTrue(resultado);
                }
            }
        }
    }
}
