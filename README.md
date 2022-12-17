# AspNetCore.Testing.MadeEasy [![NuGet Version](https://img.shields.io/nuget/v/AspNetCore.Testing.MadeEasy.svg?plastic=flat)](https://www.nuget.org/packages/AspNetCore.Testing.MadeEasy/1.0.0-beta1)

**AspNetCore.Testing.MadeEasy** provides as implementation of `DbAsyncQueryProvider` that can be used when testing a component that uses async queries with EntityFrameworkCore. Also it's provides utility to run database through docker for testing and dispose them automatically once container is being stopped.

It also comes with some helpful extensions and mock functionality to easy the testing through [Moq](https://github.com/moq/moq)

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


### The following samples of unit and integration tests are based on this Controller：

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

### One can write a **unit test** against a mock context as follows.
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

    Assert.Equal(3, result.Count);
}
```
---
### One can write a **integration test** against a real database as follows.

Under the hood, it is using [Testcontainer](https://github.com/testcontainers/testcontainers-dotnet) to run the docker container.  This library provides some support functions to quickly run the container and set up one's test cases.

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

- Get database container and start it
```C#
var manager = DatabaseManager();
await manager.SpinContainer();
```

- One can run a test case by inheriting TestBase and overriding ConfigureServices to inject any service for the test case. Although it is better to not run integration test case in parallel as they are sharing resources like database. Check test case framework you are using for running the test cases in order.  
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
            addAuth: async client =>
            {
                /*Add auth headers to the client*/
            },
            test: async client =>
            {
                var response = await client.GetAsync("/person");
                var content = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var data = content.RootElement.EnumerateArray().ToList();

                Assert.Single(data);
            },
            validateDb: async ctx =>
            {
                /*It can be used to validate data exist or not in case or update/insert and for some other cases */
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
- Finally stop the database container
```C#
await manager.StopContainer();
```

For a detailed example, you can refer to this [link](https://github.com/akshay-zz/AspNetCore.Testing.MadeEasy/blob/main/Examples/Example.WebApi.Test.WithXunit/IntegrationTest.cs)

### Other useful method
- Extension for logger
```C#
var logger = new Mock<ILogger<MyClass>>();
_ = new MyClass(logger.Object);
logger.VerifyLogging("Test information", LogLevel.Information);
logger.VerifyLogging("Some error", LogLevel.Error, Times.Exactly(2));
```
- Create mock HttpClientFactory

```C#
 var (factory, httpMessageHandler) = MockHttpClient.GetMockedNamedHttpClientFactory(
            baseUrl: "https://localhost.xyz",
            subUrl: "/data",
            response: @"{""name"":""Alex""}",
            responseStatusCode: HttpStatusCode.OK,
            httpMethod: HttpMethod.Get);

_ = new MyClass(factory.Object);

handler.VerifyAll();
```
