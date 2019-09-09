using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrainingAssessmentProject.Models
{
    [MetadataType(typeof(tblEmployeeMetadata))]
    public partial class tblEmployee
    {
        public string ConfirmPassword { get; set; }
    }

    public class tblEmployeeMetadata
    {
        [Display(Name = "Name ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Employee name required")]
        public string EmpName { get; set; }

        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID required")]
        [DataType(DataType.EmailAddress)]
        public string EmpEmailId { get; set; }

        [Display(Name = "Domain ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Employee name required")]
        public string Domain { get; set; }


        [Display(Name = "Mobile_No ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Employee name required")]
        public string Mobile_No { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password and password do not match")]
        public string ConfirmPassword { get; set; }

    }
}