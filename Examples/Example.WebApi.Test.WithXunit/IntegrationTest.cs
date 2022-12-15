using AspNetCore.Testing.MadeEasy.Extensions;
using AspNetCore.Testing.MadeEasy.IntegrationTest;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("Example.WebApi.Test.WithXunit.GlobalSetup", "Example.WebApi.Test.WithXunit")]
namespace Example.WebApi.Test.WithXunit
{
    public class IntegrationTest : TestBase<DatabaseContext, Program>
    {
        [Fact]
        public async Task Person_get_api_should_return_result()
            => await RunTest(
                populatedb: async ctx =>
                {
                    var person = PersonFactory.GetPerson();
                    await ctx.Person!.AddAsync(person);
                    await ctx.SaveChangesAsync();
                },
                test: async client =>
                {
                    var response = await client.GetAsync("/person");
                    var content = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    var data = content.RootElement.EnumerateArray().ToList();

                    Assert.Single(data);
                },
                cleanDb: async ctx =>
                {
                    ctx.Person.Clear();
                    await ctx.SaveChangesAsync();
                });

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddEntityFrameworkNpgsql().AddDbContext<DatabaseContext>(
                opt =>
                {
                    opt.UseNpgsql(DatabaseManager.ConnectionString, o => o.UseNetTopologySuite());
                });
        }
    }

    internal class GlobalSetup : XunitTestFramework, IDisposable
    {
        private readonly DatabaseManager _dbContainer = new();

        public GlobalSetup(IMessageSink messageSink) : base(messageSink)
        {
            _dbContainer.SpinContainer().Wait();
            // Wait for the server to be ready
            Task.Delay(5000).Wait();

            var application = new WebApplicationFactory<Program>()
                   .WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(ConfigureServices);
                   });

            var client = application.CreateClient();

            try
            {
                using var scope = application.Services.CreateScope();
                using var ctx = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                ctx.Database.MigrateAsync().Wait();
                ctx.Database.OpenConnectionAsync().Wait();
                ((Npgsql.NpgsqlConnection)ctx.Database.GetDbConnection()).ReloadTypes();
            }
            catch (Exception ex)
            {

            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));

            services.Remove(descriptor!);

            services.AddEntityFrameworkNpgsql().AddDbContext<DatabaseContext>(
                opt =>
                {
                    opt.UseNpgsql(DatabaseManager.ConnectionString, o => o.UseNetTopologySuite());
                });
        }

        public new void Dispose()
        {
            _dbContainer.StopContainer().Wait();
            base.Dispose();
        }
    }

}
