using System;
using System.Collections.Generic;
using System.Linq;
using Dashboard.Data;
using Dashboard.DataAccess;

namespace Dashboard
{
    public interface IHistorian
    {
        List<ChartSeries> GetBurnUpDataSince(DateTime since, string area);
        List<ChartSeries> GetBurnDown(DateTime startDate, DateTime endDate, string area);
        List<ChartSeries> GetBugsSince(DateTime since, string area);
    }

    public class Historian : IHistorian
    {
        private readonly IWorkItemRepository repository;

        public Historian(IWorkItemRepository repository)
        {
            this.repository = repository;
        }

      
        public List<ChartSeries> GetBurnUpDataSince(DateTime since, string area)
        {
            var dateWeCareAbout = CreateDatesWeCareAbout(since).ToList();
            var requestedSums = new List<Metric>();
            var completedSums = new List<Metric>();

            foreach (var date in dateWeCareAbout)
            {
                requestedSums.Add(GetWorkItemSumByDate(area, date));
                completedSums.Add(GetWorkItemSumByDate(area, date, "Done"));
            }       
            
            return new List<ChartSeries>
            {
                new ChartSeries {Title = "Requested", Data = requestedSums},
                new ChartSeries {Title = "Completed", Data = completedSums}
            };
        }

        public List<ChartSeries> GetBugsSince(DateTime since, string area)
        {
            var dateWeCareAbout = CreateDatesWeCareAbout(since).ToList();
            var requestedSums = new List<Metric>();
            var completedSums = new List<Metric>();

            foreach (var date in dateWeCareAbout)
            {
                requestedSums.Add(GetWorkItemCountByDate(area, date, null, "Bug" ));
                completedSums.Add(GetWorkItemCountByDate(area, date, "Done", "Bug"));
            }

            return new List<ChartSeries>
            {
                new ChartSeries {Title = "Found", Data = requestedSums},
                new ChartSeries {Title = "Completed", Data = completedSums}
            };
        }

        // TODO: REfactor
        private Metric GetWorkItemCountByDate(string area, DateTime date, string state = null, string workItemType = null)
        {
            var count = 0;
            var allWorkitemIds = repository.GetPrdouctBacklogItemsAsOf(area, date, state, workItemType).Result;
            if (allWorkitemIds.WorkItems.Any())
            {
                var workItems =
                    repository.GetWorkItemsAsOf(date, allWorkitemIds.WorkItems.Select(s => s.Id).ToArray()).Result;

                
                if (workItems != null)
                {
                    count = workItems.Count();
                }
            }

            return new Metric { Value = count, Date = date };
        }

        private Metric GetWorkItemSumByDate(string area, DateTime date, string state = null, string workItemType = null)
        {
            var sum = 0;
            var allWorkitemIds = repository.GetPrdouctBacklogItemsAsOf(area, date, state, workItemType).Result;
            if (allWorkitemIds.WorkItems.Any())
            {
                var workItems =
                    repository.GetWorkItemsAsOf(date, allWorkitemIds.WorkItems.Select(s => s.Id).ToArray()).Result;

                if (workItems != null)
                {
                    sum = workItems.Sum(s =>
                    {
                        int effort;
                        return int.TryParse(s.Effort, out effort) ? effort : 0;
                    });
                }
            }
            var summedEffort = new Metric { Value = sum, Date = date };

            return summedEffort;
        }
    
        private static IEnumerable<DateTime> CreateDatesWeCareAbout(DateTime since, DateTime? theEndDate = null)
        {
            var date = since.Date.AddHours(23).AddMinutes(59);
            var endDate = theEndDate != null
                ? theEndDate.Value.Date.AddHours(23).AddMinutes(59)
                : DateTime.Today.AddHours(23).AddMinutes(59);
            var list = new List<DateTime>();

            while (date <= endDate)
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    list.Add(date);
                }

                date = date.AddDays(1);
            }

            return list;
        }

        public List<ChartSeries> GetBurnDown(DateTime startDate, DateTime endDate, string area)
        {
            var burnUp = GetBurnUpDataSince(startDate, area);
            var requested = burnUp.First(s => s.Title == "Requested").Data;
            var completed = burnUp.First(s => s.Title == "Completed").Data.ToList();

            var burndown = requested.Select((t, i) => new Metric()
            {
                Value = (int)t.Value - (int)completed[i].Value,
                Date = t.Date
            }).ToList();

            var ideal = requested.First().Value;

            var theBurndown = new List<ChartSeries>
            {
                new ChartSeries
                {
                    Title = "Remaining",
                    Data = burndown
                },
                new ChartSeries
                {
                    Title = "Ideal",
                    Data = new List<Metric>
                    {
                        new Metric {Value = ideal, Date = startDate},
                        new Metric {Value = 0, Date = endDate}
                    }
                }
            };

            return theBurndown;
        }

        private int CountWorkDays(DateTime startDate, DateTime endDate, List<DateTime> excludedDates)
        {
            int dayCount = 0;
            int inc = 1;
            bool endDateIsInPast = startDate > endDate;
            DateTime tmpDate = startDate.Date;
            DateTime finiDate = endDate.Date;

            if (endDateIsInPast)
            {
                // Swap dates around
                tmpDate = endDate;
                finiDate = startDate;

                // Set increment value to -1, so it DayCount decrements rather 
                // than increments
                inc = -1;
            }

            while (tmpDate <= finiDate)
            {
                if (!excludedDates.Contains(tmpDate) && (tmpDate.DayOfWeek != DayOfWeek.Saturday && tmpDate.DayOfWeek != DayOfWeek.Sunday) )
                {
                    dayCount += inc;
                }

                // Move onto next day
                tmpDate = tmpDate.AddDays(1);
            }

            return dayCount - 1;
        }                                      
    }

    public class BurnUp
    {
        public List<WorkItemEffortSum> Requested { get; set; }
        public List<WorkItemEffortSum> Completed { get; set; }
    }
    
    public class WorkItemEffortSum
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class Metric
    {
        public DateTime Date { get; set; }
        public object Value { get; set; }
    }
 
    public class WorkItemEffortAverage
    {
        public int Effort { get; set; }
        public double AverageDays { get; set; }
    }

    public class ChartSeries
    {
        public string Title { get; set; }
        public List<Metric> Data { get; set; }
    }    
}