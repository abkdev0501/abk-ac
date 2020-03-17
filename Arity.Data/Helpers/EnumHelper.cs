namespace Arity.Data.Helpers
{
    public static class EnumHelper
    {
        public enum CompanyColor
        {
            DarkOliveGreen = 1,
            Aqua = 2
        }

        public enum TaskStatus
        {
            InProgress = 1,
            Assigned = 2,
            Cancel = 3,
            Complete = 4,
            OnHold = 5,
            Pending = 6,
            Unknown = 7
        }

        public enum TaskPrioritis
        {
            High = 1,
            Medium = 2,
            Low = 3,
            Important = 4,
            Urgent = 5
        }

        public enum BusinessType
        {
            Partnership =1,
            Proprietorship =2
        }
    }
}
