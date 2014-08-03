using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dashboard;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [Authorize]
    public partial class CycleTimeController : Controller
    {
        private readonly IHistorian historian;

        public CycleTimeController(IHistorian historian)
        {
            this.historian = historian;
        }

        // GET: CycleTime
        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual JsonResult GetData()
        {
            var cycleTimes = historian.GetEffortCycleTimeAverage(@"BPS.Scrum\Dev -SEP Project");
            var cycleTimeSeriesdata = CreateViewModel(cycleTimes, "Average Cycle Time", "#ff7f0e");

            return Json(new[] { cycleTimeSeriesdata }, JsonRequestBehavior.AllowGet);
        }

        private ChartSeriesViewModel CreateViewModel(List<WorkItemEffortAverage> cycleTimes, string seriesTitle, string colorString)
        {
            return new ChartSeriesViewModel
            {
                values =
                    cycleTimes.Select(s => new PointViewModel { x = s.Effort, y = s.AverageDays })
                        .ToList(),
                key = seriesTitle,
                color = colorString
            };
        }
    }
}