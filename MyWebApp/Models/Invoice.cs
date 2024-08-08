using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models
{
	public class Invoice 
	{
		public int InvoiceId { get; set; }
		public string PMC { get; set; }
		public string SiteName { get; set; }
		public string VendorName { get; set; }
		public decimal PriorBalance { get; set; }
		
	}
}
