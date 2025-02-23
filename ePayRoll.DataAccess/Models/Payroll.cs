using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ePayRoll.DataAccess.Models
{
	public class Payroll
	{
		public int PayrollId { get; set; }
		public int EmployeeId { get; set; }

		[Required]
		public DateTime MonthYear { get; set; }

		public DateTime ProcessedDate { get; set; }
		public decimal TotalSalary { get; set; }

		public decimal TotalAllowances { get; set; }

		public decimal TotalDeductions { get; set; }

	}
}