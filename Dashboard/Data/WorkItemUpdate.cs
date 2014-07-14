using System.Collections.Generic;
using System.Linq;

namespace Dashboard.Data
{
    public class WorkItemUpdate
    {
        public int Id { get; set; }
        public int Rev { get; set; }
        public List<WorkItemFieldValue> Fields { get; set; }

        public string Title
        {
            get { return GetValueOfField("Title"); }
        }

        public string Effort
        {
            get { return GetValueOfField("Effort"); }
        }

        public string State
        {
            get { return GetValueOfField("State"); }
        }

        public string ChangedDate
        {
            get { return GetValueOfField("Changed Date"); }
        }

        public string ClosedDate
        {
            get { return GetValueOfField("Closed Date"); }
        }

        private string GetValueOfField(string propertyName)
        {
            if (Fields != null)
            {
                WorkItemFieldValue field = Fields.FirstOrDefault(f => f.Field.Name == propertyName);
                return field != null ? field.GetValue() : string.Empty;
            }

            return string.Empty;
        }
    }
}