namespace Arity.Data.Helpers
{
    public static class EnumHelper
    {
        public enum UserRole
        {
            Master = 1,
            Admin = 2,
            User = 10002
        }
        public enum UserType
        {
            Master = 1,
            Admin = 2,
            User = 3
        }

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

        public enum DocumentStatus
        {
            RECEIVED = 1,
            SENT = 2,
            SUBMITTED = 3
        }
        public enum DocumentType
        {
            PHOTOS = 1,
            PAN,
            TAN,
            ITR_RECEIPTS,
            ITR_COMPUTATIONS,
            AUDIT_REPORTS,
            BALANCE_SHEETS,
            NOTES,
            CERTI,
            RTN_RECEIPTS,
            RTN_FILLING_DETAILS,
            GST_CALCULATIONS,
            GST_DOCS,
            GST_NOTES,
            GST_CHALLANS,
            ROF_ETC
        }

        public enum BusinessType
        {
            Partnership =1,
            Proprietorship =2
        }
    }
}
