//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Arity.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class InvoiceDetail
    {
        public long Id { get; set; }
        public string Invoice_Number { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public long ClientId { get; set; }
        public long CompanyId { get; set; }
    }
}
