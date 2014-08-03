using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dashboard;
using WebApplication.Extensions;
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
}