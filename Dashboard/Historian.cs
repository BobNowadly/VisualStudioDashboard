using System;
using System.Collections.Generic;
using System.Linq;
using Dashboard.Data;
using Dashboard.DataAccess;

namespace Dashboard
{
    public interface IHistorian
    {
        List<WorkItem> GetCommittedAndClosedWorkItems(string area);
        List<ChartSeries> GetBurnUpDataSince(DateTime since, string area);
        List<WorkItemEffortAverage> GetEffortCycleTimeAverage(string area);
        List<ChartSeries> GetBurnDown(DateTime startDate, DateTime endDate, string area);
    }

    public class Historian : IHistorian
    {
        private readonly IWorkItemRepository repository;

        public Historian(IWorkItemRepository repository)
        {
            this.repository = repository;
        }

        public List<WorkItem> GetCommittedAndClosedWorkItems(string area)
        {
            // Get Workitem ids
            // to do move the area to the constructor 
            int[] workitemids =
                repository.GetInProcAndClosedWorkItems(area)
                    .Result.Results.Select(s => s.SourceId)
                    .ToArray();

            // Get Work Items 
            IEnumerable<WorkItemUpdate> workitems = repository.GetWorkItems(workitemids).Result;

            // Get Get Changes Find Revision for Committed 
            var wi = new List<WorkItem>();
            foreach (WorkItemUpdate workitem in workitems)
            {
                IOrderedEnumerable<WorkItemUpdate> changes =
                    repository.GetWorkItemUpdates(workitem.Id)
                        .Result.OrderByDescending(a => DateTime.Parse(a.ChangedDate));
                WorkItemUpdate latestPreClosedState =
                    changes.FirstOrDefault(s => s.State != "Done" && !string.IsNullOrEmpty(s.State));
                    //Give me the state that happened before closed 

                DateTime? dateCommitted = latestPreClosedState != null &&
                                          !string.IsNullOrEmpty(latestPreClosedState.ChangedDate) &&
                                          latestPreClosedState.State == "Committed"
                    ? DateTime.Parse(latestPreClosedState.ChangedDate)
                    : (DateTime?) null;
                WorkItemUpdate latestStateChange = changes.FirstOrDefault(s => !string.IsNullOrEmpty(s.State));
                    // this should be the last state change
                DateTime? closedDAte = latestStateChange != null && !string.IsNullOrEmpty(latestStateChange.ClosedDate)
                    ? DateTime.Parse(latestStateChange.ClosedDate)
                    : (DateTime?) null;

                var effort = 0;
                var useEffort = int.TryParse(workitem.Effort, out effort);
                wi.Add(new WorkItem
                {
                    DateCommittedTime = dateCommitted,
                    DateClosed = closedDAte,
                    Id = workitem.Id,
                    Effort = useEffort ? effort : (int?)null,
                    Title = workitem.Title
                });
            }

            return wi;
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

        private Metric GetWorkItemSumByDate(string area, DateTime date, string state = null)
        {
            var allWorkitemIds = repository.GetPrdouctBacklogItemsAsOf(area, date, state).Result;
            var workItems = repository.GetWorkItemsAsOf(date, allWorkitemIds.Results.Select(s => s.SourceId).ToArray()).Result;

            var sum = 0;
            if (workItems != null)
            {
                sum = workItems.Sum(s => string.IsNullOrEmpty(s.Effort) ? 0 : int.Parse(s.Effort));
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

        public List<WorkItemEffortAverage> GetEffortCycleTimeAverage(string area)
        {
            var workItems = GetCommittedAndClosedWorkItems(area);

            var data = (from w in workItems
                where w.Effort.HasValue && w.DateCommittedTime.HasValue && w.DateClosed.HasValue
                group w by new {w.Effort}
                into g
                select
                    new WorkItemEffortAverage
                    {
                        Effort = g.Key.Effort.Value,
                        AverageDays =
                            g.Average(s => CountWorkDays(s.DateCommittedTime.Value, s.DateClosed.Value, new List<DateTime>()))
                    }
                );

            return data.OrderBy(s => s.Effort).ToList();
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