using ePayRoll.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ePayRoll.API.APIControllers
{
	[RoutePrefix("api/payroll")]
	public class PayrollAPIController : ApiController
    {
		private PayrollDbContext db = new PayrollDbContext();

		//GET: List Employees valid for Salary Processing
		[HttpGet]
		[Route("valid/{monthYear}")]
		public async Task<IHttpActionResult> GetEligibleEmployees(DateTime monthYear)
		{
			var employees = await db.Employees
				.Where(e => e.JoinDate <= monthYear && (e.ResignDate == null || e.ResignDate >= monthYear))
				.ToListAsync();

			return Ok(employees);
		}

		public class ProcessSalaryRequest
		{
			public List<int> EmployeeIds { get; set; }
			public DateTime MonthYear { get; set; }
		}

		//POST: Calculate Salary for Multiple Employees
		[HttpPost]
		[Route("calculate")]
		public async Task<IHttpActionResult> ProcessSalaries([FromBody] ProcessSalaryRequest request)
		{
			if (request.EmployeeIds == null || request.EmployeeIds.Count == 0)
				return BadRequest("No employees selected for salary processing.");

			foreach (var employeeId in request.EmployeeIds)
			{
				var processedSalary = await db.Payrolls
				.Where(p => p.EmployeeId == employeeId &&
							p.MonthYear.Year == request.MonthYear.Year &&
							p.MonthYear.Month == request.MonthYear.Month)
				.FirstOrDefaultAsync();

				if (processedSalary != null) continue; //If already processed, skip the processing.

				var employee = await db.Employees
					.Include(e => e.Allowances)
					.Include(e => e.Deductions)
					.FirstOrDefaultAsync(e => e.Id == employeeId);

				if (employee == null) continue;

				//Filter allowance by Month-Year before summing
				decimal totalAllowance = employee.Allowances
					.Where(a => a.MonthYear.Year == request.MonthYear.Year && a.MonthYear.Month == request.MonthYear.Month)
					.Sum(a => (decimal?)a.Amount) ?? 0;

				//Filter deductions by Month-Year before summing
				decimal totalDeduction = employee.Deductions
					.Where(d => d.MonthYear.Year == request.MonthYear.Year && d.MonthYear.Month == request.MonthYear.Month)
					.Sum(d => (decimal?)d.Amount) ?? 0;

				int totalDaysInMonth = DateTime.DaysInMonth(request.MonthYear.Year, request.MonthYear.Month);

				//the first and last working day for the employee
				DateTime firstDay = new DateTime(request.MonthYear.Year, request.MonthYear.Month, 1);
				DateTime lastDay = new DateTime(request.MonthYear.Year, request.MonthYear.Month, totalDaysInMonth);

				//Adjust for employees joining or resigning mid-month
				DateTime actualStartDate = (employee.JoinDate > firstDay) ? employee.JoinDate : firstDay;
				DateTime actualEndDate = (employee.ResignDate != null && employee.ResignDate < lastDay) ? (DateTime)employee.ResignDate : lastDay;

				//Calculate only the actual worked days
				int workingDays = CalculateWorkingDays(actualStartDate, actualEndDate, request.MonthYear);
				if (workingDays == 0) workingDays = 1;  // to avoid division errors

				decimal dailySalary = employee.BasicSalary / totalDaysInMonth;  // divide by total days in the month
				decimal totalSalary = Math.Round((dailySalary * workingDays) + totalAllowance - totalDeduction, 2);


				db.Payrolls.Add(new Payroll
				{
					EmployeeId = employeeId,
					MonthYear = request.MonthYear,
					ProcessedDate = DateTime.Now,
					TotalSalary = totalSalary,
					TotalAllowances = totalAllowance,
					TotalDeductions = totalDeduction,
				});
			}

			await db.SaveChangesAsync();
			return Ok(new { Message = "Salaries Processed Successfully" });
		}

		//POST: Delete Processed Salary
		[HttpPost]
		[Route("delete")]
		public async Task<IHttpActionResult> DeleteProcessedSalaries([FromBody] ProcessSalaryRequest request)
		{
			if (request.EmployeeIds == null || request.EmployeeIds.Count == 0)
				return BadRequest("No employees selected for salary deletion.");

			var processedSalaries = await db.Payrolls
				.Where(p => request.EmployeeIds.Contains(p.EmployeeId) &&
							p.MonthYear.Year == request.MonthYear.Year &&
							p.MonthYear.Month == request.MonthYear.Month)
				.ToListAsync();

			if (!processedSalaries.Any())
				return BadRequest("Processed salaries Not Found.");

			db.Payrolls.RemoveRange(processedSalaries);
			await db.SaveChangesAsync();

			return Ok(new { Message = "Processed salaries deleted successfully!" });
		}

		//GET: Get processed Salaries list for PaySlips
		[HttpGet]
		[Route("processed/{year}/{month}")]
		public async Task<IHttpActionResult> GetProcessedSalaries(int year, int month)
		{
			var salaries = await db.Payrolls
				.Join(db.Employees,
					  payroll => payroll.EmployeeId,
					  employee => employee.Id,
					  (payroll, employee) => new
					  {
						  PayrollId = payroll.PayrollId,
						  EmployeeName = employee.FullName,
						  MonthYear = payroll.MonthYear,
						  BasicSalary = employee.BasicSalary,
						  TotalAllowances = payroll.TotalAllowances,
						  TotalDeductions = payroll.TotalDeductions,
						  TotalSalary = payroll.TotalSalary,
						  ProcessedDate = payroll.ProcessedDate
					  })
				.Where(p => p.MonthYear.Year == year && p.MonthYear.Month == month)
				.ToListAsync();

			if (!salaries.Any())
				return NotFound();

			return Ok(salaries);
		}

		//GET: Get processed Salary detail for PaySlip
		[HttpGet]
		[Route("get-payroll/{payrollId}")]
		public async Task<IHttpActionResult> GetPaySlipDetails(int payrollId)
		{
			var salary = await db.Payrolls
				.Join(db.Employees,
					  payroll => payroll.EmployeeId,
					  employee => employee.Id,
					  (payroll, employee) => new
					  {
						  PayrollId = payroll.PayrollId,
						  EmployeeName = employee.FullName,
						  MonthYear = payroll.MonthYear,
						  BasicSalary = employee.BasicSalary,
						  TotalAllowances = payroll.TotalAllowances,
						  TotalDeductions = payroll.TotalDeductions,
						  TotalSalary = payroll.TotalSalary,
						  ProcessedDate = payroll.ProcessedDate
					  })
				.Where(p => p.PayrollId == payrollId)
				.FirstOrDefaultAsync();

			if (salary == null)
				return NotFound();

			return Ok(salary);
		}

		
		//Calculate Working Days (Excluding Weekends)
		private int CalculateWorkingDays(DateTime joinDate, DateTime? resignDate, DateTime monthYear)
		{
			DateTime start = joinDate > monthYear ? joinDate : monthYear;
			DateTime end = resignDate != null && resignDate < monthYear.AddMonths(1) ? (DateTime)resignDate : monthYear.AddMonths(1).AddDays(-1);

			int workingDays = 0;
			for (DateTime dt = start; dt <= end; dt = dt.AddDays(1))
			{
				if (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday)
					workingDays++;
			}
			return workingDays;
		}
	}
}
