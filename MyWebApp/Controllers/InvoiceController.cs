using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Data;
using MyWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebApp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InvoiceController : ControllerBase
	{
		private readonly InvoiceDbContext _dbContext;

		public InvoiceController(InvoiceDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		// 1. Post request for creating an invoice
		[HttpPost]
		public async Task<IActionResult> CreateInvoice(InvoiceRequest req)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var invoice = new Invoice
				{
					InvoiceId = req.InvoiceId,
					PMC = req.PMC,
					SiteName = req.SiteName,
					VendorName = req.VendorName,
					PriorBalance = req.PriorBalance
				};

				_dbContext.Invoices.Add(invoice);
				await _dbContext.SaveChangesAsync();

				return Ok(new BaseResponseModel<Invoice>
				{
					StatusCode = 200,
					Message = "Invoice created successfully",
					Data = invoice
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error creating invoice",
					Data = ex.Message
				});
			}
		}

		// 2. Get method for retrieving all invoices
		[HttpGet]
		public async Task<IActionResult> GetAllInvoices()
		{
			try
			{
				var invoices = await _dbContext.Invoices.ToListAsync();
				if (invoices == null || !invoices.Any())
				{
					return Ok(new BaseResponseModel<List<Invoice>>
					{
						StatusCode = 200,
						Message = "No invoices found",
						Data = new List<Invoice>()
					});
				}

				return Ok(new BaseResponseModel<List<Invoice>>
				{
					StatusCode = 200,
					Message = "Invoices retrieved successfully",
					Data = invoices
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error retrieving invoices",
					Data = ex.Message
				});
			}
		}



		// 3. Get method for retrieving an invoice by ID
		[HttpGet("{invoiceId}")]
		public async Task<IActionResult> GetInvoiceById(int invoiceId)
		{
			try
			{
				var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
				if (invoice == null)
				{
					return NotFound(new BaseResponseModel<string>
					{
						StatusCode = 404,
						Message = "Invoice not found",
						Data = null
					});
				}

				return Ok(new BaseResponseModel<Invoice>
				{
					StatusCode = 200,
					Message = "Invoice retrieved successfully",
					Data = invoice
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error retrieving invoice",
					Data = ex.Message
				});
			}
		}

		[HttpPost("assign")]
		public IActionResult AssignInvoice(InvoiceAssignmentRequest request)
		{
			// Validate the request
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Create a new InvoiceAssignments entity
			var invoiceAssignment = new InvoiceAssignments
			{
				InvoiceId = request.InvoiceId,
				UserId = request.UserId
			};

			// Save the invoice assignment to the database
			_dbContext.InvoiceAssignments.Add(invoiceAssignment);
			_dbContext.SaveChanges();

			// Return a success response
			return Ok(invoiceAssignment);
		}

		
		[HttpDelete("{invoiceId}")]
		public async Task<IActionResult> DeleteInvoice(int invoiceId)
		{
			try
			{
				var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
				if (invoice == null)
				{
					return NotFound(new BaseResponseModel<string>
					{
						StatusCode = 404,
						Message = "Invoice not found",
						Data = null
					});
				}

				_dbContext.Invoices.Remove(invoice);
				await _dbContext.SaveChangesAsync();

				return Ok(new BaseResponseModel<string>
				{
					StatusCode = 200,
					Message = "Invoice deleted successfully",
					Data = null
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error deleting invoice",
					Data = ex.Message
				});
			}
		}

		[HttpGet("assignments")]
		public async Task<IActionResult> GetInvoiceAssignments()
		{
			try
			{
				var invoiceAssignments = await _dbContext.InvoiceAssignments.ToListAsync();

				var invoiceAssignmentsDict = invoiceAssignments.GroupBy(a => a.InvoiceId)
					.ToDictionary(g => g.Key, g => g.ToList());

				var invoices = await _dbContext.Invoices.ToListAsync();

				var result = new List<InvoiceAssignments>();
				foreach (var invoice in invoices)
				{
					if (invoiceAssignmentsDict.TryGetValue(invoice.InvoiceId, out var assignments))
					{
						result.AddRange(assignments);
					}
					else
					{
						result.Add(new InvoiceAssignments
						{
							InvoiceId = invoice.InvoiceId,
							UserId = null
						});
					}
				}

				return Ok(new BaseResponseModel<List<InvoiceAssignments>>
				{
					StatusCode = 200,
					Message = "Invoice assignments retrieved successfully",
					Data = result
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error retrieving invoice assignments",
					Data = ex.Message
				});
			}
		}

		[HttpPost("unassign")]
		public async Task<IActionResult> UnassignInvoiceFromUser(InvoiceAssignmentRequest req)
		{
			try
			{
				// Check if the invoice exists
				var invoice = await _dbContext.Invoices.FindAsync(req.InvoiceId);
				if (invoice == null)
				{
					return NotFound(new BaseResponseModel<string>
					{
						StatusCode = 404,
						Message = "Invoice not found",
						Data = null
					});
				}

				// Check if the user exists
				var user = await _dbContext.Users.FindAsync(req.UserId);
				if (user == null)
				{
					return NotFound(new BaseResponseModel<string>
					{
						StatusCode = 404,
						Message = "User not found",
						Data = null
					});
				}

				// Check if the invoice is assigned to the user
				bool assignmentExists = await _dbContext.InvoiceAssignments
					.AnyAsync(ia => ia.InvoiceId == req.InvoiceId && ia.UserId == req.UserId);

				if (!assignmentExists)
				{
					return Ok(new BaseResponseModel<string>
					{
						StatusCode = 200,
						Message = "Invoice is not assigned to the user",
						Data = null
					});
				}

				// Unassign the invoice from the user
				var assignment = await _dbContext.InvoiceAssignments
					.FirstOrDefaultAsync(ia => ia.InvoiceId == req.InvoiceId && ia.UserId == req.UserId);
				_dbContext.InvoiceAssignments.Remove(assignment);
				await _dbContext.SaveChangesAsync();

				return Ok(new BaseResponseModel<string>
				{
					StatusCode = 200,
					Message = "Invoice unassigned from user successfully",
					Data = null
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error unassigning invoice from user",
					Data = ex.Message
				});
			}
		}

		public class InvoiceAssignmentDTO
		{
			public int InvoiceId { get; set; }
			public object AssignedTo { get; set; }
		}


		public class InvoiceAssignmentRequest
		{
			public int InvoiceId { get; set; }
			public int UserId { get; set; }
		}

		public class InvoiceRequest
		{
			public int InvoiceId { get; set; }
			public string PMC { get; set; }
			public string SiteName { get; set; }
			public string VendorName { get; set; }
			public decimal PriorBalance { get; set; }
		}
	}
}