# AspNetCore.Testing.MadeEasy

**AspNetCore.Testing.MadeEasy** provides as implementation of `DbAsyncQueryProvider` that can be used when testing a component that uses async queries with EntityFramework. Also it's provides utlity to run database though docker for testing and dispose them automatically after running all the test cases.


The following samples unit and integration tests are based on this Controller：

```C#
public class PersonController : ControllerBase
{
    private readonly DatabaseContext context;

    public PersonController(DatabaseContext database)
    {
        this.context = database;
    }

    [HttpGet]
    public IEnumerable<Person> Get()
    {
        return context.Person
            .Where(x => x.Status == Status.Active).ToArray();
    }
}
```

You can write a **unit test** against a mock context as follows.
```C#
[Fact]
public void Get_should_return_valid_persons()
{
    var data = new List<Person>{
        new Person { Name = "AAA", Status = Status.Active },
        new Person { Name = "BBB", Status = Status.Active },
        new Person { Name = "CCC", Status = Status.Active },
        new Person { Name = "DDD", Status = Status.Inactive },
    };

    var mockContext = new Mock<DatabaseContext>();

    // using lib
    var dbset = MockDb.CreateDbSet<Person>(data.AsQueryable());

    mockContext.Setup(x => x.Person).Returns(dbset.Object);
    var controller = new PersonController(mockContext.Object);
    var result = controller.Get();

    Assert.Equal(data, result);
}
```

You can write a **integration test** against a mock context as follows.

- Configure `appsettings.Testing.json`
```JSON
"InternalTestSetting": {
		"UseExternaldb": false,
		"ConnectionString": "",
		"DockerDb": {
			"Image": "kartoza/postgis",
			"ContainerName": "example_postgris",
			"DbName": "example",
			"UserName": "postgres",
			"Password": "welcome"
		}
	}
```

- Get your container and start it
```C#
var container = PostgresDbManager.GetTestContainer();
await container.StartAsync();
```

- Run you taste case by inhereting TestBase and override to configure service. Like database.
```C#
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
                opt.UseNpgsql(PostgresDbManager.ConnectionString, o => o.UseNetTopologySuite());
            });
    }
}
```
- Finally stop and dispose the container
```C#
await container.StopAsync();
await container.DisposeAsync();
```