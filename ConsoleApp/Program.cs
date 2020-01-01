using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace ConsoleApp {
    class Program {
        static IWebDriver Driver;
        static Config AppConfig;
        static int GamesPerPage;

        static void Main() {
            // Set up.
            AppConfig = Config.FromJsonFile(Constants.ConfigFile);
            ChangeCulture();
            InitDriver();

            // Navigation.
            Console.WriteLine("Loading page...");
            Driver.Navigate().GoToUrl(Constants.MainUrl);
            
            Console.WriteLine("Loading games...");
            GamesPerPage = GetGames().Length;
            var gamesCount = GetGamesCount();
            var gamesFound = CheckGames(gamesCount);
            Console.WriteLine("Games found: {0} of {1}", gamesFound.Length, gamesCount);

            if (gamesFound.Length > 0) {
                var filteredGames = FilterGames(gamesFound);
                SaveResults(filteredGames);
            }

            // Tear down.
            Driver.Close();
            Driver.Dispose();
            Console.WriteLine("Driver closed.");
            
            Console.WriteLine("\nPress any key to finish...");
            Console.ReadKey();
        }

        public static void ChangeCulture() {
            var customCulture = (System.Globalization.CultureInfo) Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;
        }

        public static void InitDriver() {
            var options = new ChromeOptions();
            options.AddUserProfilePreference("disk-cache-size", AppConfig.DriverCache);
            
            if (AppConfig.RunHeadless) {
                options.AddArgument("--headless");
            } else {
                options.AddArgument("--start-maximized");
            }

            if (!AppConfig.LoadImages) {
                options.AddArgument("--load-images=no");
                options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            }


            if (AppConfig.UseRemoteDriver) {
                Driver = new RemoteWebDriver(new Uri(AppConfig.RemoteDriverIp), options);
                Console.WriteLine("Driver started at: {0}", AppConfig.RemoteDriverIp);
            } else {
                if (AppConfig.LocalDriverPath != null) {
                    Driver = new ChromeDriver(AppConfig.LocalDriverPath, options);
                } else {
                    Driver = new ChromeDriver(options);
                }

                Console.WriteLine("Driver started locally.");
            }
            
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(AppConfig.DriverImplicitWait);
        }

        public static int GetGamesCount() {
            var text = Driver.FindElement(By.Id("result-count")).Text;
            var pattern = @"\d+";
            var filter = new Regex(pattern);
            var match = filter.Match(text);

            if (match.Success) {
                int.TryParse(match.Value, out var count);
                return count - 1;
            }

            return -1;
        }

        public static IWebElement[] GetGames() {
            var loadedGames = Driver.FindElements(By.ClassName("main-link")).ToArray();
            return loadedGames;
        }

        public static IWebElement[] LoadMoreGames(out bool success) {
            // Try to get the button to load more games.
            var gamesBeforeLoad = GetGames();
            IWebElement btnLoadMore;

            try {
                btnLoadMore = Driver.FindElement(By.Id("btn-load-more"));
                btnLoadMore.Click();
                
                WaitForGamesToLoad:
                Thread.Sleep(TimeSpan.FromSeconds(AppConfig.SecondsToWaitForElements));
                var gamesAfterLoad = GetGames();

                if (gamesAfterLoad.Length <= gamesBeforeLoad.Length) {
                    goto WaitForGamesToLoad;
                }

                success = true;
                return gamesAfterLoad.ToArray();
            }
            catch (Exception e)
            when(e is ElementNotInteractableException || e is NoSuchElementException) {
                Console.WriteLine("All games loaded.");
                success = false;
                return gamesBeforeLoad;
            }
            catch (Exception e) {
                Console.WriteLine("Unexpected exception:\n{0}", e.StackTrace);
                throw e;
            }
        }
        
        public static IWebElement[] CheckGames(int gamesCount, int iteration = 0, IWebElement[] previousGames = null) {
            var consoleTitle = Console.Title;
            var progress = (iteration * 100) / ((double) gamesCount / GamesPerPage);
            Console.Title = string.Format("Progress: {0:0.00}%", progress);
            var currentGames = LoadMoreGames (out var success);

            if (success) {
                iteration++;
                return CheckGames(gamesCount, iteration, currentGames);
            }

            Console.Title = consoleTitle;
            return currentGames;
        }

        public static List<Game> FilterGames(IWebElement[] games) {
            Console.WriteLine("Filtering...");
            var filteredGames = new List<Game>();
            var count = 0;
            
            foreach (var g in games) {
                count++;
                var progress = count * 100 / (double) games.Length;
                Console.Title = string.Format("{0} of {1} ({2:0.00}%)", count, games.Length, progress);

                var title = g.FindElement(By.ClassName("b3")).Text;
                var coverPath = g.FindElement(By.CssSelector("img")).GetAttribute("src");
                var price = g.FindElement(By.ClassName("strike")).Text;
                var salePrice = g.FindElement(By.ClassName("sale-price")).Text;

                var game = new Game {
                    Title = title,
                    CoverPath = coverPath,
                    Price = FilterPrice(price),
                    SalePrice = FilterPrice(salePrice)
                };
                filteredGames.Add(game);
            }
            
            return filteredGames;
        }

        public static double? FilterPrice(string text) {
            var pattern = @"\d+(\.\d+)?";
            var filter = new Regex(pattern);
            var match = filter.Match(text);

            if (match.Success) {
                double.TryParse(match.Value, out var price);
                return price;
            }

            return null;
        }

        public static void SaveResults(List<Game> filteredGames) {
            var jsonText = JsonConvert.SerializeObject(new { games = filteredGames }, Formatting.Indented);

            var date = DateTime.Now.ToShortDateString();
            var fileName = $@"{Constants.ResultsFolder}\{Constants.ResultsFileName} ({date}).json";

            Directory.CreateDirectory(Constants.ResultsFolder);
            File.WriteAllText(fileName, jsonText);
            Console.WriteLine("Deals saved to: {0}", fileName);
        }
    }
}
