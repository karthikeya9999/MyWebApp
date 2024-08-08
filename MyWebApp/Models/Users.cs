using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models
{
	public class Users
	{
		[Key]
		public int UserId { get; set; }
		public string UserName { get; set; }
		public string Role { get; set; }
		
	}
}
