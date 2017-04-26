using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TITcs.SharePoint.SSOM.Test.Repositories;

namespace TITcs.SharePoint.SSOM.Test
{
    [TestClass]
    public class GetAllFromFolderTest
    {
        private const string siteUrl = "http://conectesp2016.hom.titcs.com.br/";
        private SPSite _spSite;

        [TestMethod]
        [TestCategory("GetAllFromFolderWithQuery")]
        public void DeveRetornar8ArquivosDaPastaDocumentosConfidenciais()
        {
            using (_spSite = new SPSite(siteUrl))
            {
                var web = _spSite.OpenWeb();
                var _repo = new NovaCentralArquivosRepository(web);
                var pasta = @"Documentos%20Confidenciais";
                var items = _repo.GetAllFromFolderWithQuery(pasta, string.Empty);

                Assert.IsTrue(items.Data.Count == 8);
            }
        }
    }
}
