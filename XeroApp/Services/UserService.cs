using Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Config;

namespace XeroApp.Services
{
	public interface IUserService
	{
		Task<XeroClientApp> CheckUserExistsAsync(string name);
	}

	public class UserService : IUserService
	{
		private readonly XeroAppDbContext _db;

		public UserService(XeroAppDbContext db)
		{
			_db = db;
		}

		public async Task<XeroClientApp> CheckUserExistsAsync(string name)
		{
			var clientApp = await _db.ClientApps.SingleOrDefaultAsync(c => c.UserName == name);
			return clientApp ?? null;
		}

    }
}