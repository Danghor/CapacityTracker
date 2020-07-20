using CapacityTracker.Properties;
using HtmlAgilityPack;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace CapacityTracker
{
    internal class Program
    {
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

            Schedule(TimeSpan.FromSeconds(int.Parse(Resources.FetchIntervalInSeconds)), () =>
            {
                string line;

                try
                {
                    line = $"{DateTime.Now}, {GetCurrentLoad()}";
                }
                catch (Exception ex)
                {
                    line = $"{DateTime.Now} ERROR: Could not fetch load: {ex.Message}";
                }

                File.AppendAllLines(Resources.OutputFile, new[] { line });
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

            var htmlDocument = htmlWeb.Load(Resources.Url);

            var loadNode = htmlDocument.DocumentNode.SelectNodes("/html/body/div[1]/div[1]/header/div[2]/a/div[2]/div[3]/div");
            var load = loadNode.Single().InnerText;

            Regex regex = new Regex("[0-9]{1,3}(?=%)");

            var foo = regex.Match(load);

            return int.Parse(foo.Value);
        }
    }
}
