using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Threading.Tasks;

namespace AspNetCore.Testing.MadeEasy.IntegrationTest;

/// <summary>
/// Database manager
/// </summary>
public class DatabaseManager
{
    private readonly TestcontainersContainer container;

    /// <summary>
    /// Initiate <see cref="DatabaseManager"/>
    /// </summary>
    public DatabaseManager()
    {
        if (!InternalTestSettingManager.Current.UseExternaldb)
        {
            container = GetTestContainer();
        }
    }

    private static TestcontainersContainer GetTestContainer()
    {
        return new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage(InternalTestSettingManager.Current.DockerDb.Image)
                .WithName(InternalTestSettingManager.Current.DockerDb.ContainerName)
                .WithEnvironment(InternalTestSettingManager.Current.DockerDb.EnviromentVariables)
                .WithCleanUp(true)
                .WithPortBinding(5432, 5432)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();
    }

    /// <summary>
    /// Start the database container
    /// </summary>
    /// <returns></returns>
    public async Task SpinContainer()
    {
        await container?.StartAsync();
    }


    /// <summary>
    /// Stop the database container
    /// </summary>
    /// <returns></returns>
    public async Task StopContainer()
    {
        await container?.StopAsync();
    }

    /// <summary>
    /// Provide connection string of the test database.
    /// </summary>
    public static string ConnectionString 
        => InternalTestSettingManager.Current.ConnectionString;
}
