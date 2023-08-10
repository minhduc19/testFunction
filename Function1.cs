using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.Json;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;

namespace FirstFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--no-sandbox");
            chromeOptions.AddArguments("--headless");
            //chromeOptions.AddArguments("--remote-debugging-port=9222");
            IWebDriver driver = new ChromeDriver("chromedriver.exe", chromeOptions);
            // Navigate to the web page
            driver.Navigate().GoToUrl("https://jisho.org/");
            // Find the input element and enter text
            IWebElement inputElement = driver.FindElement(By.Id("keyword"));
            string inputText = "昨日すき焼きを食べました";
            inputElement.SendKeys(inputText);
            // Find the submit button and click it
            IWebElement submitButton = driver.FindElement(By.ClassName("submit"));
            submitButton.Click();
            // Get the page source as a string
            string pageSource = driver.PageSource;

            List<WordPair> wordPairList = new List<WordPair>();
            // Create a WebClient to download the HTML content of the webpage


            // Use HtmlAgilityPack to parse the HTML content
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageSource);

            // Get all elements with class name "japanese_word__furigana"
            //HtmlNodeCollection wordElements = htmlDocument.DocumentNode.SelectNodes("//span[contains(@class, 'japanese_word__text_wrapper')]");
            IEnumerable<HtmlNode> wordElements = htmlDocument.DocumentNode.Descendants("span")
                                        .Where(n => n.GetAttributeValue("class", "").Contains("japanese_word__text_wrapper"));
            IEnumerable<HtmlNode> furiganaElements = htmlDocument.DocumentNode.Descendants("span")
                                        .Where(n => n.GetAttributeValue("class", "").Contains("japanese_word__furigana_wrapper"));
            //HtmlNodeCollection furiganaElements = htmlDocument.DocumentNode.Descendants().Where(n => n.GetAttributeValue("class", "").Contains("japanese_word__furigana"));
    

                if (furiganaElements != null)
                {

                    for (int i = 0; i < wordElements.Count(); i++)
                    {
                        if (wordElements.ElementAt(i).InnerText.Trim() == furiganaElements.ElementAt(i).InnerText.Trim())
                        {
                            Console.WriteLine(wordElements.ElementAt(i).InnerText.Trim() + "the same as " + furiganaElements.ElementAt(i).InnerText.Trim());
                        }

                        wordPairList.Add(new WordPair(wordElements.ElementAt(i).InnerText.Trim(), furiganaElements.ElementAt(i).InnerText.Trim()));

                    }

                }

                string jsonString = System.Text.Json.JsonSerializer.Serialize(wordPairList, new JsonSerializerOptions
                {
                    // Set this option to ensure proper encoding of non-ASCII characters
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true // Optional: Format the JSON for readability
                });
            driver.Quit();
                return new OkObjectResult(jsonString);
        }
    }
}
