using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dashboard;
using WebApplication.Extensions;
using WebApplication.Models;

namespace WebApplication.Controllers
{
     [Authorize]
    public partial class BurnupController : Controller
    {
        private readonly IHistorian historian;
        //TODO: This kind of sucks can we do something better? 
        private readonly Dictionary<string, string> colors = new Dictionary<string, string>
        {
            {"Requested", "#ff7f0e"},
            {"Completed", "#2ca02c"}
        };

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

               var series =
                burnupData.Select(d => CreateViewModel(d.Data, d.Title, colors[d.Title]));

            return Json(series, JsonRequestBehavior.AllowGet);
        }
        
        // TODO: Add Automapper
        private ChartSeriesViewModel CreateViewModel(IEnumerable<Metric> data, string seriesTitle, string colorString)
        {
            return new ChartSeriesViewModel
            {
                values =
                    data.Select(s => new PointViewModel { x = s.Date.ToInt(), y = s.Value })
                        .ToList(),
                key = seriesTitle,
                color = colorString
            };
        }
    }
}