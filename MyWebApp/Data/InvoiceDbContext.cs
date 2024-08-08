using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;

namespace MyWebApp.Data
{
	public class InvoiceDbContext : DbContext
	{
		public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options)
			: base(options)
		{
		}

		public DbSet<Invoice> Invoices { get; set; }
		public DbSet<Users> Users { get; set; }

		public DbSet<InvoiceAssignments> InvoiceAssignments{ get; set; }

		//public DbSet<InvoiceAssign> invoiceAssign { get; set; }

	}
}