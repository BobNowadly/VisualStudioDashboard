namespace Dashboard.Data
{
    public class WorkItemFieldValue
    {
        public WorkItemField Field { get; set; }
        public string UpdatedValue { get; set; }
        public string Value { get; set; }

        public string GetValue()
        {
            return Value ?? UpdatedValue;
        }
    }
}