using FileUpload.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FileUpload.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<EmployeeModel> Employees { get; set; }
    }
}
