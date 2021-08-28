using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestAuthTokens.Data
{
	public class ApiDbContext : IdentityDbContext, IApiDbContext
	{
		public ApiDbContext(DbContextOptions options) 
			: base(options)
		{
		}
	}
}
