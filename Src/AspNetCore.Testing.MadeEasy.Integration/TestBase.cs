using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Testing.MadeEasy.Integration;

/// <summary>
/// Inherit to your test class to get access of db context and test http client. 
/// For each test case that run through <see cref="RunTest(Func{TDbContext, Task}, Func{HttpClient, Task}, Func{TDbContext, Task}, Func{TDbContext, Task}, bool)"/>
/// have an independetent <see cref="HttpClient"/> and <see cref="WebApplicationFactory{TEntryPoint}"/>. Although 
/// uses same database under the hood.
/// </summary>
/// <typeparam name="TDbContext">Database context class</typeparam>
/// <typeparam name="TEntryPoint">Entry point of the application</typeparam>
public class TestBase<TDbContext, TEntryPoint>
    where TDbContext : DbContext
    where TEntryPoint : class

{
    /// <summary>
    /// Use to populate data or to clean data independet of test case.
    /// </summary>
    /// <param name="through"></param>
    /// <returns></returns>
    protected async Task ManageDb(Func<TDbContext, Task> through)
    {
        var application = new WebApplicationFactory<TEntryPoint>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(ConfigureServices);
                });

        using var scope = application.Services.CreateScope();

        _ = application.CreateClient();

        var ctx = scope.ServiceProvider.GetRequiredService<TDbContext>();

        if (through != null)
        {
            await through(ctx);
        }
    }

    /// <summary>
    /// Use to run test cases.
    /// </summary>
    /// <param name="populatedb"></param>
    /// <param name="test"></param>
    /// <param name="validateDb"></param>
    /// <param name="cleanDb"></param>
    /// <param name="addAuth"></param>
    /// <returns></returns>
    protected async Task RunTest(Func<TDbContext, Task> populatedb = null,
        Func<HttpClient, Task> test = null,
        Func<TDbContext, Task> validateDb = null,
        Func<TDbContext, Task> cleanDb = null, bool addAuth = true)
    {
        try
        {
            var application = new WebApplicationFactory<TEntryPoint>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(ConfigureServices);
                });
            var client = application.CreateClient();
            using var scope = application.Services.CreateScope();

            var ctx = scope.ServiceProvider.GetRequiredService<TDbContext>();

            await this.SetUp(ctx);

            if (populatedb != null)
                await populatedb(ctx);

            if (addAuth)
                this.AddAuthenticationToClient(client);

            if (test != null)
                await test(client);

            if (validateDb != null)
                await validateDb(ctx);

            if (cleanDb != null)
                await cleanDb(ctx);

            await this.TearDown(ctx);
        }
        catch (Exception)
        {

            throw;
        }

    }

    /// <summary>
    /// Add authentication to the http client. Overide to extend client authentication
    /// </summary>
    /// <param name="client"></param>
    protected virtual void AddAuthenticationToClient(HttpClient client)
    {
        //Add authentication
    }

    /// <summary>
    /// Setup services for the test cases. Overide to customise services. Override to configure the database connection string
    /// </summary>
    /// <param name="services"></param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TDbContext>));

        services.Remove(descriptor!);

        // Need to add if will handle the authentication

        //services.AddAuthorization(config =>
        //{
        //});
    }

    /// <summary>
    /// Will be called before every test case.
    /// </summary>
    /// <param name="ctx"></param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    protected virtual async Task SetUp(TDbContext ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        //Never add any code here. Only to be used by overriding.
    }

    /// <summary>
    /// Will be called after every test case.
    /// </summary>
    /// <param name="ctx"></param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    protected virtual async Task TearDown(TDbContext ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        //Never add any code here. Only to be used by overriding.
    }
}