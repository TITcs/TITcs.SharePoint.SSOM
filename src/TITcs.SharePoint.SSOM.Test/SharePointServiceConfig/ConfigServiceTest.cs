using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using TITcs.SharePoint.SSOM.Services;

namespace TITcs.SharePoint.SSOM.Test.SharePointServiceConfig
{
    [TestClass]
    public class ConfigServiceTest
    {
        private static readonly string _sharepointServiceSectionTypeName = "TITcs.SharePoint.SSOM.Services.SharePointServiceSection";

        public Configuration GetConfig()
        {
            // var filePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, @"SharePointServiceConfig", @"App.config");
            Assembly.LoadFrom(@"C:\dev\git\TITcs.SharePoint.SSOM\src\TITcs.SharePoint.SSOM\bin\Debug\TITcs.SharePoint.SSOM.dll");
            var filePath = @"C:\dev\git\TITcs.SharePoint.SSOM\src\TITcs.SharePoint.SSOM.Test\SharePointServiceConfig\App.config";
            return ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() {
                ExeConfigFilename = filePath
            }, ConfigurationUserLevel.None);
        }

        [TestMethod]
        [TestCategory("SharePointServiceConfig")]
        public void DeveCarregarOArquivoDeConfiguracao()
        {
            var config = GetConfig();
            Assert.IsNotNull(config);
        }

        [TestMethod]
        [TestCategory("SharePointServiceConfig")]
        public void DeveCarregarOArquivoDeConfiguracaoERetornarASecaoTipada()
        {
            var config = GetConfig();
            var serviceSection = (SharePointServiceSection)config.Sections.Cast<ConfigurationSection>().FirstOrDefault(c => c.SectionInformation.Type == _sharepointServiceSectionTypeName);
            Assert.IsNotNull(serviceSection);
        }
    }
}
