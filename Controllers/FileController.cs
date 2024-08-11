using CsvHelper;
using FileUpload.Data;
using FileUpload.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Text;

namespace FileUpload.Controllers
{
    public class FileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public FileController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }




        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }



        // GET: File/Upload
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(FileUploadViewModel model)
        {
            if (model.File != null && model.File.Length > 0)
            {
                var employees = new List<EmployeeModel>();

                using (var stream = new StreamReader(model.File.OpenReadStream()))
                {
                    using (var csv = new CsvReader(stream, new CsvHelper.Configuration.CsvConfiguration
                        (CultureInfo.InvariantCulture)))
                    {
                        employees = csv.GetRecords<EmployeeModel>().ToList();
                    }
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = "Employees";
                        var table = new DataTable();
                        table.Columns.Add("Id", typeof(int));
                        table.Columns.Add("Name", typeof(string));
                        table.Columns.Add("Position", typeof(string));
                        table.Columns.Add("Salary", typeof(decimal));

                        foreach (var employee in employees)
                        {
                            table.Rows.Add(employee.Id, employee.Name, employee.Position, employee.Salary);
                        }

                        await bulkCopy.WriteToServerAsync(table);
                    }
                }

                return RedirectToAction("Index", "File");
            }

            return View(model);
        }

        // GET: File/Download
        public async Task<IActionResult> Download()
        {
            var employees = await _context.Employees.ToListAsync();
            var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecords(employees);
                }
            }

            return File(memoryStream.ToArray(), "text/csv", "employees.csv");
        }
    }
}