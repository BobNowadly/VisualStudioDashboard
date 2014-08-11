using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using Dashboard;
using WebApplication.Extensions;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public partial class BurnDownController : Controller
    {
        private readonly IHistorian historian;

        public BurnDownController(IHistorian historian)
        {
            this.historian = historian;
        }

        // GET: BurnDown
        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual JsonResult GetData()
        {
            var burnupData = historian.GetBurnDown(new DateTime(2014, 7, 9, 23, 59, 59),
                new DateTime(2014, 11, 7, 23, 59, 59), @"BPS.Scrum\Dev -SEP Project");
            var requestedViewModel = CreateViewModel(burnupData.Ideal, "Ideal", "#ff7f0e", false);
            var completedViewModel = CreateViewModel(burnupData.Remaining, "Remaining Points", "#2ca02c", true);

            return Json(new[] { requestedViewModel, completedViewModel }, JsonRequestBehavior.AllowGet);
        }

        private ChartSeriesViewModel CreateViewModel(IEnumerable<WorkItemEffortSum> data, string seriesTitle, string colorString, bool area)
        {
            return new ChartSeriesViewModel
            {
                values =
                    data.Select(s => new PointViewModel { x = s.Date.ToInt(), y = s.Count })
                        .ToList(),
                key = seriesTitle,
                color = colorString,
                area = area.ToString().ToLower()
            };
        }
    }
}