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
        BurnUp GetBurnUpDataSince(DateTime since, string area);
        List<WorkItemEffortAverage> GetEffortCycleTimeAverage(string area);
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

        public BurnUp GetBurnUpDataSince(DateTime since, string area)
        {
            var dateWeCareAbout = CreateDatesWeCareAbout(since);
            var requestedSums = new List<WorkItemEffortSum>();
            var completedSums = new List<WorkItemEffortSum>();
            
            foreach (var date in dateWeCareAbout)
            {
                var allWorkitemIds = repository.GetPrdouctBacklogItemsAsOf(area, date).Result;
                var workItems = repository.GetWorkItemsAsOf(date, allWorkitemIds.Results.Select(s => s.SourceId).ToArray());

                var closedWorkitemIds = repository.GetPrdouctBacklogItemsAsOf(area, date, "Done").Result;
                var closedworkItems = repository.GetWorkItemsAsOf(date, closedWorkitemIds.Results.Select(s => s.SourceId).ToArray());

                var requested = 0;
                if (workItems.Result != null)
                {
                    requested = workItems.Result.Sum(s => string.IsNullOrEmpty(s.Effort) ? 0 : int.Parse(s.Effort));
                }
                requestedSums.Add(new WorkItemEffortSum() {Count = requested, Date = date});

                var completed = 0;
                if (closedworkItems.Result != null)
                {
                    completed = closedworkItems.Result
                        .Sum(s => string.IsNullOrEmpty(s.Effort) ? 0 : int.Parse(s.Effort));
                }
                
                completedSums.Add(new WorkItemEffortSum() {Count = completed, Date = date});
            }
          
            var burnUp = new BurnUp
            {
                Requested = requestedSums,
                Completed = completedSums
            };

            return burnUp;
        }

        private List<DateTime> CreateDatesWeCareAbout(DateTime since)
        {
            var date = since.Date.AddHours(23).AddMinutes(59);
            var endDate = DateTime.Today.AddHours(23).AddMinutes(59);
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

    public class WorkItemEffortAverage
    {
        public int Effort { get; set; }
        public double AverageDays { get; set; }
    }
}