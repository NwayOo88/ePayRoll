using ePayRoll.Models;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ePayRoll.Controllers
{
    public class PayslipController : Controller
    {
		private readonly string apiUrl;

		public PayslipController()
		{
			apiUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] + "payroll/";
		}

		// GET: PaySlip List by MonthYear
		public async Task<ActionResult> Index(string selectedMonthYear)
        {
			if (string.IsNullOrEmpty(selectedMonthYear))
				selectedMonthYear = DateTime.Now.ToString("yyyy-MM");

			DateTime selectedDate = DateTime.ParseExact(selectedMonthYear, "yyyy-MM", null);
			int selectedYear = selectedDate.Year;
			int selectedMonth = selectedDate.Month;

			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(apiUrl + $"processed/{selectedYear}/{selectedMonth}");

			List<ProcessedSalary> salaries = new List<ProcessedSalary>();
			if (response.IsSuccessStatusCode)
			{
				string jsonData = await response.Content.ReadAsStringAsync();
				salaries = JsonConvert.DeserializeObject<List<ProcessedSalary>>(jsonData);
			}

			return View(salaries);
		}

		// GET: Download PaySlip
		[HttpGet]
		public async Task<ActionResult> GeneratePaySlip(int payrollId)
		{
			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(apiUrl + "get-payroll/" + payrollId);

			if (!response.IsSuccessStatusCode)
				return HttpNotFound();

			string jsonData = await response.Content.ReadAsStringAsync();
			ProcessedSalary salary = JsonConvert.DeserializeObject<ProcessedSalary>(jsonData);

			LocalReport lr = new LocalReport();
			string path = Path.Combine(Server.MapPath("~/Reports"), "PaySlipReport.rdlc");
			lr.ReportPath = path;

			ReportDataSource rd = new ReportDataSource("PaySlipDataset", new List<ProcessedSalary> { salary });
			lr.DataSources.Add(rd);

			string mimeType, encoding, fileNameExtension;
			Warning[] warnings;
			string[] streams;
			byte[] renderedBytes = lr.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

			return File(renderedBytes, "application/pdf", $"PaySlip_{salary.EmployeeName}_{salary.MonthYear:yyyyMM}.pdf");
		}

		// GET: View PaySlip
		[HttpGet]
		public async Task<ActionResult> ViewPaySlip(int payrollId)
		{
			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(apiUrl + "get-payroll/" + payrollId);

			if (!response.IsSuccessStatusCode)
				return HttpNotFound("Pay slip data not found!");

			string jsonData = await response.Content.ReadAsStringAsync();
			ProcessedSalary salary = JsonConvert.DeserializeObject<ProcessedSalary>(jsonData);

			LocalReport lr = new LocalReport();
			string reportPath = Path.Combine(Server.MapPath("~/Reports"), "PaySlipReport.rdlc");

			if (!System.IO.File.Exists(reportPath))
				throw new FileNotFoundException("PaySlipReport.rdlc not found!");

			lr.ReportPath = reportPath;

			string reportDataSetName = "PaySlipDataset";

			List<ProcessedSalary> paySlipData = new List<ProcessedSalary> { salary };

			ReportDataSource rd = new ReportDataSource(reportDataSetName, paySlipData);
			lr.DataSources.Clear();
			lr.DataSources.Add(rd);

			string mimeType, encoding, fileNameExtension;
			Warning[] warnings;
			string[] streams;

			byte[] renderedBytes = lr.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

			return File(renderedBytes, "application/pdf");
		}


	}
}