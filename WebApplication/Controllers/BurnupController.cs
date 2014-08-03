using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dashboard;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public partial class BurnupController : Controller
    {
        private readonly IHistorian historian;

        public BurnupController(IHistorian historian)
        {
            this.historian = historian;
        }

        // GET: Burnup
        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual JsonResult GetData()
        {
            var burnupData = historian.GetBurnUpDataSince(new DateTime(2014, 7, 9, 23, 59, 59),  @"BPS.Scrum\Dev -SEP Project");
            var requestedViewModel = CreateViewModel(burnupData.Requested, "Requested Points", "#ff7f0e");
            var completedViewModel = CreateViewModel(burnupData.Completed, "Completed Points", "#2ca02c");

            return Json(new[] { requestedViewModel, completedViewModel }, JsonRequestBehavior.AllowGet);
        }

        private ChartSeriesViewModel CreateViewModel(IEnumerable<WorkItemEffortSum> data, string seriesTitle, string colorString)
        {
            return new ChartSeriesViewModel
            {
                values =
                    data.Select(s => new PointViewModel { x = s.Date.ToInt(), y = s.Count })
                        .ToList(),
                key = seriesTitle,
                color = colorString
            };
        }
    }

    public static class DataModelExtension
    {

        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a DateTime to its Unix timestamp value. This is the number of seconds
        /// passed since the Unix Epoch (1/1/1970 UTC)
        /// </summary>
        /// <param name="aDate">DateTime to convert</param>
        /// <returns>Number of seconds passed since 1/1/1970 UTC </returns>
        public static long ToInt(this DateTime aDate)
        {
            if (aDate == DateTime.MinValue)
            {
                return -1;
            }
            TimeSpan span = (aDate - UnixEpoch);
            return (long) Math.Floor(span.TotalMilliseconds);
        }

        /// <summary>
        /// Converts the specified 32 bit integer to a DateTime based on the number of seconds
        /// since the Unix epoch (1/1/1970 UTC)
        /// </summary>
        /// <param name="anInt">Integer value to convert</param>
        /// <returns>DateTime for the Unix int time value</returns>
        public static DateTime ToDateTime(this int anInt)
        {
            if (anInt == -1)
            {
                return DateTime.MinValue;
            }
            return UnixEpoch.AddSeconds(anInt);
        }
    }
}