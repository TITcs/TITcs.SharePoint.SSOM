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

        private readonly string _listViewClass = ".ms-listviewtable";
        private readonly string _newEditTable = "#Hero-WPQ2 .ms-list-addnew";
        private readonly string _formTable = ".ms-formtable";
        private static readonly string _url = "http://marcos.pacheco:$titcs%402016@dmz-shs-05/";
        private static WebDriverWait _wait;
        private static IWebDriver _driver;

        #endregion

        #region hooks

        [BeforeFeature(Order = 0)]
        public static void Setup() {
            _driver = new ChromeDriver() {
                Url = _url
            };
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5));
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
            _wait.Until<IWebElement>(w => w.FindElement(By.CssSelector(_listViewClass)));
        }
        [Then(@"I should see the list items paged")]
        public void ThenIShouldSeeTheListItemsPaged()
        {
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
            _wait.Until<IWebElement>(w => w.FindElement(By.CssSelector(_listViewClass)));
            var _pageSize = p0;            
            var _trSelectors = string.Format("{0} tbody tr", _listViewClass);
            Assert.IsTrue(_driver.FindElements(By.CssSelector(_trSelectors)).Count == _pageSize);
        }
        [Then(@"I press the new button e insert item data e save")]
        public void IPressTheNewButtonEInsertItemDataESave()
        {
            _wait.Until<IWebElement>(w => w.FindElement(By.CssSelector(_newEditTable)));
            var _newButton = _driver.FindElement(By.LinkText("new item"));
            _newButton.Click();
        }
        [Then(@"I should see the new item")]
        public void IShouldSeeTheNewItem()
        {
            var _pageSize = 30;          
            var _inputText = _driver.FindElement(By.CssSelector(string.Format("{0} .ms-formbody input", _formTable)));
            _inputText.Click();
            _inputText.SendKeys("Projeto 101");
            var _saveButton = _driver.FindElement(By.CssSelector(".ms-formtoolbar:nth-child(2) .ms-toolbar:nth-child(2) input[type=\"button\"]"));
            _saveButton.Click();            
            var _trSelectors = string.Format("{0} tbody tr", _listViewClass);
            _wait.Until<IWebElement>(w => w.FindElement(By.CssSelector(_trSelectors)));
            Assert.IsTrue(_driver.FindElements(By.CssSelector(_trSelectors)).Count == _pageSize);
        }

        #endregion
    }
}
