using System.Collections.Generic;

namespace Dashboard.Data
{
    public class WorkItemUpdates
    {
        public List<WorkItemUpdate> Value { get; set; }
    }

    //public class WorkItems
    //{
    //    public List<WorkItem> Value { get; set; }
    //}

    //public class WorkItem
    //{
    //      public int Id { get; set; }
    //    public int Rev { get; set; }
    //    public List<WorkItemFieldValue> OldFields { get; set; }
    //    public Dictionary<string, WorkItemFieldValue> Fields { get; set; }

    //    public string Title
    //    {
    //        get { return GetValueOfField("System.Title"); }
    //    }

    //    public string Effort
    //    {
    //        get { return GetValueOfField("Microsoft.VSTS.Scheduling.Effort"); }
    //    }

    //    public string State
    //    {
    //        get { return GetValueOfField("System.State"); }
    //    }

    //    public string ChangedDate
    //    {
    //        get { return GetValueOfField("System.ChangedDate"); }
    //    }

    //    public string ClosedDate
    //    {
    //        get { return GetValueOfField("Microsoft.VSTS.Common.ClosedDate"); }
    //    }

    //    private string GetValueOfField(string propertyName)
    //    {
    //        if (Fields != null)
    //        {
    //            var field = Fields[propertyName];
    //            return field.NewValue;
    //        }

    //        return string.Empty;
    //    }
    //}
}