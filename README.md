# AspNetCore.Testing.MadeEasy

**AspNetCore.Testing.MadeEasy** provides as implementation of `DbAsyncQueryProvider` that can be used when testing a component that uses async queries with EntityFramework. Also it's provides utlity to run database though docker for testing and dispose them automatically after running all the test cases.

---

Unit test cases set up is quite inspired from [EntityFramework.Testing](https://github.com/scott-xu/EntityFramework.Testing). But for Ef core.

It supports mock of following opeartions:

- `Find`
- `FindAsync`
- `Add`
- `AddAsync`
- `Attach`
- `Remove`
- `AddRange`
- `AddRangeAsync`
- `AttachRange`
- `RemoveRange`
- `AddRange`


### The following samples unit and integration tests are based on this Controller：

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

### You can write a **unit test** against a mock context as follows.
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
    var dbset = MockDb.CreateDbSet<Person>(data);

    mockContext.Setup(x => x.Person).Returns(dbset.Object);
    var controller = new PersonController(mockContext.Object);
    var result = controller.Get();

    Assert.Equal(data, result);
}
```
---
### You can write a **integration test** against a real db as follows.

Under the hood we are using [Testcontainer](https://github.com/testcontainers/testcontainers-dotnet) to run the docker container. This lib provides some support functions to quickly run the container and setup you test cases.

- Configure `appsettings.Testing.json`
```JSON
{
  "AspNetCore.Testing.MadeEasy": {
    "UseExternaldb": false,
    "ConnectionString": "Server=localhost;database=example;User Id=postgres;password=welcome;port=5432;",
    "DockerDb": {
      "Image": "kartoza/postgis",
      "ContainerName": "example_postgris",
      "EnviromentVariables": {
        "POSTGRES_USER": "postgres",
        "POSTGRES_PASSWORD": "welcome",
        "POSTGRES_DB": "example"
      }
    }
  }
}
```

- Get your database container and start it
```C#
var manager = DatabaseManager();
await manager.SpinContainer();
```

- Run you test case by inhereting TestBase and override ConfigureServices to inject any service for the test case.
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
                opt.UseNpgsql(DatabaseManager.ConnectionString, o => o.UseNetTopologySuite());
            });
    }
}
```
- Finally stop database container
```C#
await manager.StopContainer();
```

For detail example you can refere this [link](`https://github.com/akshay-zz/AspNetCore.Testing.MadeEasy/blob/main/Examples/Example.WebApi.Test.WithXunit/IntegrationTest.cs`)