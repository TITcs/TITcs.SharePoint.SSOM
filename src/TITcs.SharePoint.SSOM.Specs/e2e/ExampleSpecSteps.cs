using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using TechTalk.SpecFlow;

namespace TITcs.SharePoint.SSOM.Specs.e2e
{
    [Binding]
    public class ExampleSpecSteps
    {
        #region fields and properties

        private string _listViewClass = ".ms-listviewtable";
        private static readonly string _url = "http://marcos.pacheco:$titcs%402016@dmz-shs-05/";
        private static IWebDriver _driver;

        #endregion

        #region hooks

        [BeforeFeature(Order = 0)]
        public static void Setup() {
            _driver = new ChromeDriver() {
                Url = _url
            };
        }

        [AfterFeature(Order = 0)]
        public static void TearDown() {
            _driver.Quit();
            _driver.Dispose();
        }

        #endregion

        #region steps

        [Given(@"I access the sharepoint site")]
        public void GivenIAccessTheSharepointSite()
        {
            _driver.Navigate();
        }
        [When(@"I navigate to the Projects list")]
        public void WhenINavigateToTheProjectsList()
        {
            var _projectsListUrl = string.Format("{0}_layouts/15/start.aspx#/Lists/Projetos/AllItems.aspx", _url);
            _driver.Navigate().GoToUrl(_projectsListUrl);
            var _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _wait.Until<IWebElement>(w => w.FindElement(By.CssSelector(_listViewClass)));
        }
        [Then(@"I should see the list items paged")]
        public void ThenIShouldSeeTheListItemsPaged()
        {
            var _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _wait.Until<IWebElement>(w => w.FindElement(By.CssSelector(_listViewClass)));
            var _pageSize = 30;
            var _trSelectors = string.Format("{0} tbody tr", _listViewClass);            
            Assert.IsTrue(_driver.FindElements(By.CssSelector(_trSelectors)).Count == _pageSize);
        }
        [Then(@"I press the next page button")]
        public void ThenIPressTheNextPageButton (){
            var _button = _driver.FindElement(By.Id("pagingWPQ2next"));
            _button.Click();
        }
        [Then(@"I see more (.*) results")]
        public void ThenISeeMoreResults(int p0)
        {
            var _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _wait.Until<IWebElement>(w => w.FindElement(By.CssSelector(_listViewClass)));
            var _pageSize = p0;            
            var _trSelectors = string.Format("{0} tbody tr", _listViewClass);
            Assert.IsTrue(_driver.FindElements(By.CssSelector(_trSelectors)).Count == _pageSize);
        }

        #endregion
    }
}
