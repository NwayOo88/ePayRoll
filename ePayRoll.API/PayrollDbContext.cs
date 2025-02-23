using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ePayRoll.DataAccess.Models;
using System.Data.Entity;

namespace ePayRoll.API
{
	public class PayrollDbContext : DbContext
	{
		public PayrollDbContext() : base("payRollConnection") { }

		public DbSet<Employee> Employees { get; set; }
		public DbSet<Allowance> Allowances { get; set; }
		public DbSet<Deduction> Deductions { get; set; }
		public DbSet<Payroll> Payrolls { get; set; }
	}
}