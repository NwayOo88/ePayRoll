using ePayRoll.DataAccess.Models;
using ePayRoll.Models;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ePayRoll.Controllers
{
    public class PayrollController : Controller
    {
		private readonly string apiUrl;

		public PayrollController()
		{
			apiUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] + "payroll/";
		}

		//GET: Valid Employee List to calculate Payroll
		public async Task<ActionResult> Index()
		{
			List<Employee> employees = new List<Employee>();
			HttpClient client = new HttpClient();
			string monthYear = DateTime.Now.ToString("yyyy-MM") + "-01"; 
			HttpResponseMessage response = await client.GetAsync(apiUrl + "valid/" + monthYear);
			if (response.IsSuccessStatusCode)
			{
				string jsonData = await response.Content.ReadAsStringAsync();
				employees = JsonConvert.DeserializeObject<List<Employee>>(jsonData);
			}
			return View(employees);
		}

		//POST: Calculate salary
		[HttpPost]
		public async Task<ActionResult> ProcessSalary(List<int> employeeIds, string monthYear)
		{
			if (employeeIds == null || employeeIds.Count == 0)
			{
				TempData["ErrorMessage"] = "Please select at least one employee.";
				return RedirectToAction("Process");
			}
			HttpClient client = new HttpClient();
			var requestData = new
			{
				employeeIds = employeeIds,
				monthYear = DateTime.ParseExact(monthYear, "yyyy-MM", null)
			};

			string json = JsonConvert.SerializeObject(requestData);
			StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
			HttpResponseMessage response = await client.PostAsync(apiUrl + "calculate", content);

			if (response.IsSuccessStatusCode)
			{
				return Json(new { success = true, message = "Salary processed successfully!" });
			}
			else
			{
				string errorMessage = await response.Content.ReadAsStringAsync();
				return Json(new { success = false, message = "Error processing salaries." + errorMessage });
			}
		}

		//POST: Delete processed salary
		[HttpPost]
		public async Task<ActionResult> DeleteSalary(List<int> employeeIds, string monthYear)
		{
			if (employeeIds == null || employeeIds.Count == 0)
			{
				TempData["ErrorMessage"] = "Please select at least one employee.";
				return RedirectToAction("Process");
			}

			HttpClient client = new HttpClient();

			var requestData = new 
			{
				EmployeeIds = employeeIds,
				MonthYear = DateTime.ParseExact(monthYear, "yyyy-MM", null) 
			};

			string json = JsonConvert.SerializeObject(requestData);
			StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

			HttpResponseMessage response = await client.PostAsync(apiUrl + "delete", content);

			if (response.IsSuccessStatusCode)
			{
				return Json(new { success = true, message = "Processed salaries deleted successfully!" });
			}
			else
			{
				string errorMessage = await response.Content.ReadAsStringAsync();
				return Json(new { success = false, message = "Error deleting processed salaries: " + errorMessage });
			}
		}

	}
}