using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using CsvHelper;
using CsvHelper.Configuration;
using Dashboard.Data;
using Dashboard.DataAccess;

namespace Dashboard.Console
{
    internal class Program
    {
        private static void Main()
        {
            using (
                var client = new HttpClient {BaseAddress = new Uri(ConfigurationManager.AppSettings["VSOnlineBaseUrl"])}
                )
            {
                var history = new Historian(
                    new WorkItemRepository(new TfsConnection(ConfigurationManager.AppSettings["username"],
                        ConfigurationManager.AppSettings["password"], client)));


                var burnup = history.GetBurnUpDataSince(new DateTime(2014, 7, 7, 23, 59, 59), @"BPS.Scrum\Dev -SEP Project");
                using (var writer = new StreamWriter(@".\completed" + DateTime.Now.ToString("yyyymmmmdd") + ".xls"))
                {
                    var csvWriter = new CsvWriter(writer, new CsvConfiguration { Delimiter = "\t" });

                    csvWriter.WriteRecords(burnup.First(s => s.Title == "Completed").Data);
                }

                using (var writer = new StreamWriter(@".\requested" + DateTime.Now.ToString("yyyymmmmdd") + ".xls"))
                {
                    var csvWriter = new CsvWriter(writer, new CsvConfiguration { Delimiter = "\t" });

                    csvWriter.WriteRecords(burnup.First(s => s.Title == "Requested").Data);
                }
            }
        }
    }
}