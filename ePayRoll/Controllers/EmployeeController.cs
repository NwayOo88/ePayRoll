using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ePayRoll.DataAccess.Models;
using System.Configuration;


namespace ePayRoll.Controllers
{
	public class EmployeeController : Controller
	{
		private readonly string apiUrl;

		public EmployeeController()
		{
			apiUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] + "employee";
		}


		// GET: Employee List
		public async Task<ActionResult> Index(string searchName, string searchDob, string filterStatus)
		{
			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync($"{apiUrl}/list");

			if (!response.IsSuccessStatusCode)
			{
				ModelState.AddModelError("", "Error retrieving employee list.");
				return View(new List<Employee>());
			}

			string jsonData = await response.Content.ReadAsStringAsync();
			List<Employee> employees = JsonConvert.DeserializeObject<List<Employee>>(jsonData);

			// Apply Filters
			if (!string.IsNullOrEmpty(searchName))
				employees = employees.Where(e => e.FullName.ToLower().Contains(searchName.ToLower())).ToList();

			if (!string.IsNullOrEmpty(searchDob) && DateTime.TryParse(searchDob, out DateTime dobFilter))
				employees = employees.Where(e => e.DOB.Date == dobFilter.Date).ToList();

			if (!string.IsNullOrEmpty(filterStatus))
			{
				if (filterStatus == "Active")
					employees = employees.Where(e => e.ResignDate == null).ToList();
				else if (filterStatus == "Resigned")
					employees = employees.Where(e => e.ResignDate != null).ToList();
			}

			return View(employees);
		}

		//GET: Employee Form (Create or Edit)
		public async Task<ActionResult> Detail(int? id)
		{
			Employee employee = new Employee
			{
				Allowances = new List<Allowance>(),  
				Deductions = new List<Deduction>()   
			};
			if (id != null)
			{
				HttpClient client = new HttpClient();
				HttpResponseMessage response = await client.GetAsync($"{apiUrl}/{id}");

				if (response.IsSuccessStatusCode)
				{
					string jsonData = await response.Content.ReadAsStringAsync();
					var responseData = JsonConvert.DeserializeObject<dynamic>(jsonData);

					// Deserialize Employee Details
					employee = JsonConvert.DeserializeObject<Employee>(responseData["Employee"].ToString());

					// Deserialize Allowances
					employee.Allowances = JsonConvert.DeserializeObject<List<Allowance>>(responseData["Allowances"].ToString());

					// Deserialize Deductions
					employee.Deductions = JsonConvert.DeserializeObject<List<Deduction>>(responseData["Deductions"].ToString());
				}
			}
			return View("Detail", employee);
		}


		//POST: Save (Create or Update)
		[HttpPost]
		public async Task<ActionResult> Save(Employee employee)
		{
			if (!ModelState.IsValid)
			{
				return View("Detail", employee);
			}

			try
			{
				HttpClient client = new HttpClient();
				string json = JsonConvert.SerializeObject(new
				{
					employee.Id,
					employee.FullName,
					employee.DOB,
					employee.Gender,
					employee.JoinDate,
					employee.ResignDate,
					employee.BasicSalary,
					Allowances = employee.Allowances?.Select(a => new
					{
						a.Type,
						a.Amount,
						a.MonthYear
					}).ToList(),
					Deductions = employee.Deductions?.Select(d => new
					{
						d.Type,
						d.Amount,
						d.MonthYear
					}).ToList()
				});

				StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

				HttpResponseMessage response = employee.Id == 0
					? await client.PostAsync($"{apiUrl}/create", content)
					: await client.PutAsync($"{apiUrl}/update/{employee.Id}", content);

				if (response.IsSuccessStatusCode)
				{
					return RedirectToAction("Index");
				}
				else
				{
					string errorMessage = await response.Content.ReadAsStringAsync();
					TempData["ErrorMessage"] = $"Error saving employee: {response.StatusCode} - {errorMessage}";
				}
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "Unexpected error: " + ex.Message;
			}

			return View("Detail", employee);
		}

		//DELETE: Remove Employee
		public async Task<ActionResult> Delete(int id)
		{
			try
			{
				HttpClient client = new HttpClient();
				HttpResponseMessage response = await client.DeleteAsync($"{apiUrl}/delete/{id}");

				if (response.IsSuccessStatusCode)
				{
					return Json(new { success = true, message = "Employee deleted successfully!" });
				}
				else
				{
					string errorMessage = await response.Content.ReadAsStringAsync();
					return Json(new { success = false, message = $"Error deleting employee: {response.StatusCode} - {errorMessage}" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Unexpected error: " + ex.Message });
			}

		}
	}
}