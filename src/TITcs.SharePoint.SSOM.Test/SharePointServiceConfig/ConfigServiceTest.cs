using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TITcs.SharePoint.SSOM.Services;

namespace TITcs.SharePoint.SSOM.Test.SharePointServiceConfig
{
    [TestClass]
    public class ConfigServiceTest
    {
        [TestMethod]
        public void DeveCarregarOArquivoDeConfiguracao()
        {
            var filePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, @"SharePointServiceConfig", @"App.config");
            var config = ConfigurationManager.OpenExeConfiguration(filePath);
            Assert.IsNotNull(config);
        }

        [TestMethod]
        public void DeveRetornarAppSettingsDoArquivoDeConfiguracao()
        {
            var filePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, @"SharePointServiceConfig", @"App.config");
            var config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() {
                ExeConfigFilename = filePath
            }, ConfigurationUserLevel.None);

            var services = config.AppSettings;
            Assert.IsNotNull(services);
        }

        [TestMethod]
        public void DeveRetornarAsSecoesDoArquivoDeConfiguracao()
        {
            var filePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, @"SharePointServiceConfig", @"App.config");
            var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = filePath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None, true);

            var services = (SharePointServiceSection) ConfigurationManager.GetSection("sharePointService");
            Assert.IsNotNull(services);
        }
    }
}
