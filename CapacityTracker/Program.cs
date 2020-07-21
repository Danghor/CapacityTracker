using HtmlAgilityPack;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace CapacityTracker
{
    internal class Program
    {
        // How many boulderers are waiting?

        private static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("DE-de");

            Console.WriteLine("This program periodically fetches the information and saves it into a csv file. Press Ctrl-C to quit.");

            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            var fetchInterval = TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings.Get("FetchIntervalInSeconds")));

            Schedule(fetchInterval, () =>
            {
                string line;

                try
                {
                    line = $"{DateTime.Now}{ConfigurationManager.AppSettings.Get("Delimiter")}{GetCurrentLoad()}";
                    File.AppendAllLines(ConfigurationManager.AppSettings.Get("OutputFile"), new[] { line });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now} ERROR: Could not fetch load: {ex.Message}");
                }
            });

            exitEvent.WaitOne();

            Console.WriteLine("Program terminated.");
        }

        private static void Schedule(TimeSpan interval, Action action)
        {
            action();
            var timer = new System.Timers.Timer(interval.TotalMilliseconds);
            timer.Elapsed += (o, e) => action();
            timer.Start();
        }

        private static int GetCurrentLoad()
        {
            var htmlWeb = new HtmlWeb();

            var htmlDocument = htmlWeb.Load(ConfigurationManager.AppSettings.Get("Url"));

            var loadNode = htmlDocument.DocumentNode.SelectNodes("/html/body/div[1]/div[1]/header/div[2]/a/div[2]/div[3]/div");
            var load = loadNode.Single().InnerText;

            Regex regex = new Regex("[0-9]{1,3}(?=%)");

            var foo = regex.Match(load);

            return int.Parse(foo.Value);
        }
    }
}
