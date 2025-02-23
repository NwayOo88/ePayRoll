using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ePayRoll.DataAccess.Models;

namespace ePayRoll.API.APIControllers
{
	[RoutePrefix("api/employee")]
	public class EmployeeAPIController : ApiController
    {
		private PayrollDbContext db = new PayrollDbContext();

		//GET: List all employees
		[HttpGet]
		[Route("list")]
		public async Task<IHttpActionResult> GetEmployees()
		{
			var employees = await db.Employees.ToListAsync();
			return Ok(employees);
		}

		//GET: Get Employee by ID
		[HttpGet]
		[Route("{id}")]
		public async Task<IHttpActionResult> GetEmployee(int id)
		{
			var employee = await db.Employees
				.Include(e => e.Allowances)
				.Include(e => e.Deductions)
				.FirstOrDefaultAsync(e => e.Id == id);

			if (employee == null)
				return NotFound();

			// return Employee Data with Allowances & Deductions
			var response = new
			{
				Employee = employee,
				Allowances = employee.Allowances.Select(a => new
				{
					a.AllowanceId,
					a.Type,
					a.Amount,
					MonthYear = a.MonthYear.ToString("yyyy-MM")
				}).ToList(),
				Deductions = employee.Deductions.Select(d => new
				{
					d.DeductionId,
					d.Type,
					d.Amount,
					MonthYear = d.MonthYear.ToString("yyyy-MM")
				}).ToList()
			};

			return Ok(response);
		}


		//POST: Add Employee
		[HttpPost]
		[Route("create")]
		public async Task<IHttpActionResult> CreateEmployee(Employee employee)
		{
			if (employee.DOB > DateTime.Now.AddYears(-18))
				return BadRequest("Employee must be at least 18 years old.");

			if (employee.ResignDate != null && employee.ResignDate < employee.JoinDate)
				return BadRequest("Resign Date must not be earlier than Join Date.");

			//Remove Allowances & Deductions from Employee before saving 
			var allowances = employee.Allowances?.ToList();
			var deductions = employee.Deductions?.ToList();
			employee.Allowances = null;
			employee.Deductions = null;

			db.Employees.Add(employee);
			await db.SaveChangesAsync(); 

			if (allowances != null && allowances.Any())
			{
				foreach (var allowance in allowances)
				{
					allowance.EmployeeId = employee.Id;
				}
				db.Allowances.AddRange(allowances);
			}

			if (deductions != null && deductions.Any())
			{
				foreach (var deduction in deductions)
				{
					deduction.EmployeeId = employee.Id;
				}
				db.Deductions.AddRange(deductions);
			}

			await db.SaveChangesAsync();
			return Ok(employee);
		}

		//PUT: Update Employee
		[HttpPut]
		[Route("update/{id}")]
		public async Task<IHttpActionResult> UpdateEmployee(int id, Employee employee)
		{
			if (id != employee.Id) return BadRequest();

			var existingEmployee = await db.Employees
				.Include(e => e.Allowances)
				.Include(e => e.Deductions)
				.FirstOrDefaultAsync(e => e.Id == id);

			if (existingEmployee == null)
				return NotFound();

			// Update Employee Basic Details
			existingEmployee.FullName = employee.FullName;
			existingEmployee.DOB = employee.DOB;
			existingEmployee.Gender = employee.Gender;
			existingEmployee.JoinDate = employee.JoinDate;
			existingEmployee.ResignDate = employee.ResignDate;
			existingEmployee.BasicSalary = employee.BasicSalary;

			db.Allowances.RemoveRange(existingEmployee.Allowances);
			db.Deductions.RemoveRange(existingEmployee.Deductions);

			if (employee.Allowances != null)
			{
				foreach (var allowance in employee.Allowances)
				{
					allowance.EmployeeId = id; 
					db.Allowances.Add(allowance);
				}
			}

			if (employee.Deductions != null)
			{
				foreach (var deduction in employee.Deductions)
				{
					deduction.EmployeeId = id; 
					db.Deductions.Add(deduction);
				}
			}

			await db.SaveChangesAsync();
			return Ok();
		}

		//DELETE: Delete Employee
		[HttpDelete]
		[Route("delete/{id}")]
		public async Task<IHttpActionResult> DeleteEmployee(int id)
		{
			var employee = await db.Employees.FindAsync(id);
			if (employee == null) return NotFound();

			//Check if Employee has Payroll Records
			bool hasPayrollRecords = await db.Payrolls.AnyAsync(p => p.EmployeeId == id);
			if (hasPayrollRecords)
				return BadRequest("Cannot delete Employee as payroll has already been processed.");

			var allowances = db.Allowances.Where(a => a.EmployeeId == id);
			db.Allowances.RemoveRange(allowances);

			var deductions = db.Deductions.Where(d => d.EmployeeId == id);
			db.Deductions.RemoveRange(deductions);

			db.Employees.Remove(employee);
			await db.SaveChangesAsync();
			return Ok();
		}
	}
}
