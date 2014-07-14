using System;

namespace Dashboard.Data
{
    public class WorkItem
    {
        public DateTime? DateCommittedTime { get; set; }
        public DateTime? DateClosed { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public int? Effort { get; set; }
    }
}