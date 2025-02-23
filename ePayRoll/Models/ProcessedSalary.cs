using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ePayRoll.Models
{
	public class ProcessedSalary
	{
		public int PayrollId { get; set; } 
		public string EmployeeName { get; set; } 
		public DateTime MonthYear { get; set; } 
		public decimal BasicSalary { get; set; } 
		public decimal TotalAllowances { get; set; } 
		public decimal TotalDeductions { get; set; } 
		public decimal TotalSalary { get; set; }
	}
}