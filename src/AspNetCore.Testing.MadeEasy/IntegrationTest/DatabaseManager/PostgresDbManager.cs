using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Threading.Tasks;

namespace AspNetCore.Testing.MadeEasy.IntegrationTest.DatabaseManager;

public class PostgresDbManager
{
    private readonly TestcontainersContainer container;

    public PostgresDbManager()
    {
        if (!InternalTestSettingManager.Current.UseExternaldb)
        {
            container = GetTestContainer();
        }
    }

    private static TestcontainersContainer GetTestContainer()
    {
        var compose = new TestcontainersBuilder<TestcontainersContainer>()
        .WithImage(InternalTestSettingManager.Current.DockerDb.Image)
        .WithName(InternalTestSettingManager.Current.DockerDb.ContainerName)
        .WithEnvironment(InternalTestSettingManager.Current.DockerDb.EnviromentVariables)
        .WithCleanUp(true)
        .WithPortBinding(5432, 5432)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

        return compose;
    }

    public async Task SpinContainer()
    {
        await container?.StartAsync();
    }

    public async Task StopContainer()
    {
        await container?.StopAsync();
        //await container?.DisposeAsync();
    }

    /// <summary>
    /// Provide connection string of the test database.
    /// </summary>
    public static string ConnectionString
    {
        get
        {
            return InternalTestSettingManager.Current.ConnectionString;
        }
    }
}
