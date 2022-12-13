using Example.WebApi.Model;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Example.WebApi
{
	public class DatabaseContext : DbContext
	{
		// Only for testing purpose
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public DatabaseContext() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
		{
			//_configuration = configuration;
		}

		protected override void OnModelCreating(ModelBuilder builder)
					=> builder.HasPostgresEnum<Status>();

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//    optionsBuilder.LogTo(Console.WriteLine);
		//}

		static DatabaseContext()
		{
			NpgsqlConnection.GlobalTypeMapper.MapEnum<Status>();
		}

		public DbSet<Person>? Person { get; set; }
	}
}
