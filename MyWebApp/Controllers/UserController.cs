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
	public class UserController : ControllerBase
	{
		private readonly InvoiceDbContext _dbContext;

		public UserController(InvoiceDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		// 1. Post request for creating a user
		[HttpPost]
		public async Task<IActionResult> CreateUser(CreateUserRequest request)
		{
			try
			{
				var user = new Users
				{
					UserId = request.UserId,
					UserName = request.Name,
					Role = request.Role
				};

				_dbContext.Users.Add(user);
				await _dbContext.SaveChangesAsync();

				return Ok(new BaseResponseModel<Users>
				{
					StatusCode = 200,
					Message = "User created successfully",
					Data = user
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error creating user",
					Data = ex.Message
				});
			}
		}

		// 2. Get request for retrieving all users
		[HttpGet]
		public async Task<IActionResult> GetAllUsers()
		{
			try
			{
				var users = await _dbContext.Users.ToListAsync();
				return Ok(new BaseResponseModel<List<Users>>
				{
					StatusCode = 200,
					Message = "Users retrieved successfully",
					Data = users
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error retrieving users",
					Data = ex.Message
				});
			}
		}

		// 3. Get request for retrieving a user by ID
		[HttpGet("{userId}")]
		public async Task<IActionResult> GetUserById(int userId)
		{
			try
			{
				var user = await _dbContext.Users.FindAsync(userId);
				if (user == null)
				{
					return NotFound(new BaseResponseModel<string>
					{
						StatusCode = 404,
						Message = "User not found",
						Data = null
					});
				}

				return Ok(new BaseResponseModel<Users>
				{
					StatusCode = 200,
					Message = "User retrieved successfully",
					Data = user
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error retrieving user",
					Data = ex.Message
				});
			}
		}

		[HttpDelete("{userId}")]
		public async Task<IActionResult> DeleteUser(int userId)
		{
			try
			{
				var user = await _dbContext.Users.FindAsync(userId);
				if (user == null)
				{
					return NotFound(new BaseResponseModel<string>
					{
						StatusCode = 404,
						Message = "User not found",
						Data = null
					});
				}

				_dbContext.Users.Remove(user);
				await _dbContext.SaveChangesAsync();

				return Ok(new BaseResponseModel<string>
				{
					StatusCode = 200,
					Message = "Removed User successfully",
					Data = null
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponseModel<string>
				{
					StatusCode = 500,
					Message = "Error deleting user",
					Data = ex.Message
				});
			}
		}
	}



	public class CreateUserRequest
	{
		public int UserId { get; set; }
		public string Name { get; set; }
		public string Role { get; set; }
	}
}