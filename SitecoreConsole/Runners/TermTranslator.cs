using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SitecoreConsole.Runners
{
    public class TermTranslator
    {
        private ChromeDriver _driver;
        private ChromeDriverService _driverService;
        private const string TranslatorUrl = "https://translate.google.com/?sl=en&tl={0}&op=translate";
        private readonly string _sourceTextSelector;
        private readonly string _targetTextSelector;
        private readonly string _driverPath;
        private string Barrier = "=================";
        public TermTranslator()
        {
            _sourceTextSelector =
                Sitecore.Configuration.Settings.GetSetting("Translator.SourceTextSelector", "textarea.er8xn");
            _targetTextSelector =
                Sitecore.Configuration.Settings.GetSetting("Translator.TargetTextSelector", ".//*/span[@class='VIiyi']");
            _driverPath = Sitecore.Configuration.Settings.GetSetting("Translator.ChromeDriverPath", 
                @"D:\Projects\SitecoreConsole\SitecoreConsole\Drivers");
        }

        public void Start()
        {
            _driverService = ChromeDriverService.CreateDefaultService(_driverPath);
            _driverService.HideCommandPromptWindow = true;
            _driver = new ChromeDriver(_driverService);
            CustomConsoleLogger.Log("Term Translator Selenium Initialized", ConsoleColor.Green);
        }

        public void Stop()
        {
            _driver?.Close(); ;
            _driverService?.Dispose();
        }

        public Dictionary<string, string> Translate(List<string> terms, string languageCode)
        {
            var strBuilder = new StringBuilder();
            foreach (var term in terms)
            {
                strBuilder.AppendLine(term);
                strBuilder.AppendLine(Barrier);
            }
            var final = strBuilder.ToString();
            var url = string.Format(TranslatorUrl, languageCode);
            _driver.Navigate().GoToUrl(url);
            var sourceTextArea = _driver.FindElement(By.CssSelector(_sourceTextSelector));
            sourceTextArea.SendKeys(final);
            var wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 0, 20));
            wait.Until(driver =>
            {
                var targetSpan = driver.FindElements(By.XPath(_targetTextSelector));
                return targetSpan.Any() && targetSpan[0].Text != string.Empty;
            });
            var translatedText = _driver.FindElement(By.XPath(_targetTextSelector)).Text;
            var result = ProcessResult(terms, translatedText);
            CustomConsoleLogger.Log($"Term Translator Translated {terms.Count} terms", ConsoleColor.Magenta);
            return result;
        }

        private Dictionary<string, string> ProcessResult(List<string> terms, string translatedText)
        {
            var result = new Dictionary<string, string>();
            var translatedPieces = new List<string>();
            var strBuilder = new StringBuilder();
            foreach (var character in translatedText)
            {
                strBuilder.Append(character);
                var current = strBuilder.ToString();
                if (current.EndsWith(Barrier))
                {
                    strBuilder = new StringBuilder();
                    translatedPieces.Add(current.Replace(Barrier, string.Empty).Replace(Environment.NewLine, string.Empty));
                }
            }

            for (var i = 0; i < terms.Count; i++)
            {
                result[terms[i]] = translatedPieces[i];
            }

            return result;
        }
    }
}
