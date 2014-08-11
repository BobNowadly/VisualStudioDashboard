using System.Collections.Generic;

namespace WebApplication.Models
{
    public class ChartSeriesViewModel
    {
        public string area { get; set; }
        public List<PointViewModel> values { get; set; }
        public string key { get; set; }
        public string color { get; set; }
    }
}