//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TrainingAssessmentProject.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblEmployee
    {
        public int EmpId { get; set; }
        public string EmpName { get; set; }
        public string Domain { get; set; }
        public string EmpEmailId { get; set; }
        public Nullable<long> Mobile_No { get; set; }
        public byte[] EmpPhoto { get; set; }
        public string Password { get; set; }
        public Nullable<System.Guid> Activation { get; set; }
        public Nullable<bool> VerifyEmail { get; set; }
        public Nullable<System.Guid> ReferenceId { get; set; }
        public string ResetPasswordCode { get; set; }
    }
}
