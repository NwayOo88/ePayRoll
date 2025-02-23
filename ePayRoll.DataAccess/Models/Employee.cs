using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePayRoll.DataAccess.Models
{
	public class Employee
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Full Name is required.")]
		public string FullName { get; set; }

		[Required(ErrorMessage = "Date of Birth is required.")]
		[DataType(DataType.Date)]
		[CustomValidation(typeof(Employee), "ValidateAge")]
		public DateTime DOB { get; set; }

		[Required]
		public string Gender { get; set; }

		[Required(ErrorMessage = "Join Date is required.")]
		[DataType(DataType.Date)]
		public DateTime JoinDate { get; set; }

		[DataType(DataType.Date)]
		[CustomValidation(typeof(Employee), "ValidateResignDate")]
		public DateTime? ResignDate { get; set; }

		[Required(ErrorMessage = "Basic Salary is required.")]
		[Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive decimal value.")]
		public decimal BasicSalary { get; set; }

		//Check Validation for Employee Age (Must be 18+)
		public static ValidationResult ValidateAge(DateTime dob, ValidationContext context)
		{
			if (dob > DateTime.Now.AddYears(-18))
			{
				return new ValidationResult("Employee must be at least 18 years old.");
			}
			return ValidationResult.Success;
		}

		//Check Validation for Resign Date (Must be after Join Date)
		public static ValidationResult ValidateResignDate(DateTime? resignDate, ValidationContext context)
		{
			var instance = (Employee)context.ObjectInstance;
			if (resignDate.HasValue && resignDate < instance.JoinDate)
			{
				return new ValidationResult("Resign date cannot be earlier than join date.");
			}
			return ValidationResult.Success;
		}


		public virtual List<Allowance> Allowances { get; set; } = new List<Allowance>();
		public virtual List<Deduction> Deductions { get; set; } = new List<Deduction>();
	}
	public class Allowance
	{
		public int AllowanceId { get; set; }
		public int EmployeeId { get; set; }
		public string Type { get; set; }
		public decimal Amount { get; set; }
		public DateTime MonthYear { get; set; }
	}

	public class Deduction
	{
		public int DeductionId { get; set; }
		public int EmployeeId { get; set; }
		public string Type { get; set; }
		public decimal Amount { get; set; }
		public DateTime MonthYear { get; set; }
	}
}