namespace MyWebApp.Models
{
	public class InvoiceAssignments
	{

		public int Id { get; set; }
		public int InvoiceId { get; set; }
		public int? UserId { get; set; }
	}
}
