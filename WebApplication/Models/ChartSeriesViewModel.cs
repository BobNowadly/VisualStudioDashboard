using System.Collections.Generic;

namespace WebApplication.Models
{
    public class ChartSeriesViewModel
    {
        bool area { get; set; }
        public List<PointViewModel> values { get; set; }
        public string key { get; set; }
        public string color { get; set; }
    }
}