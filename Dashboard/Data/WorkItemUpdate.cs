using System.Collections.Generic;
using System.Linq;

namespace Dashboard.Data
{
    public abstract class WorkItemUpdateBase
    {
        public int Id { get; set; }
        public int Rev { get; set; }

        public string Title
        {
            get { return GetValueOfField("System.Title"); }
        }

        public string Effort
        {
            get { return GetValueOfField("Microsoft.VSTS.Scheduling.Effort"); }
        }

        public string State
        {
            get { return GetValueOfField("System.State"); }
        }

        public string ChangedDate
        {
            get { return GetValueOfField("System.ChangedDate"); }
        }

        public string ClosedDate
        {
            get { return GetValueOfField("Microsoft.VSTS.Common.ClosedDate"); }
        }

        protected abstract string GetValueOfField(string propertyName);
    }

    public class WorkItemUpdate : WorkItemUpdateBase
    {
        public List<WorkItemFieldValue> OldFields { get; set; }
        public Dictionary<string, WorkItemFieldValue> Fields { get; set; }

        protected override string GetValueOfField(string propertyName)
        {
            if (Fields != null && Fields.ContainsKey(propertyName))
            {
                var field = Fields[propertyName];
                return field.NewValue;
            }

            return string.Empty;
        }
    }

    public class WorkItemJson : WorkItemUpdateBase
    {
        public Dictionary<string, string> Fields { get; set; }

        protected override string GetValueOfField(string propertyName)
        {
            if (Fields != null && Fields.ContainsKey(propertyName))
            {
                return Fields[propertyName];
            }

            return string.Empty;
        }
    }
}