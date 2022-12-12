using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace AspNetCore.Testing.MadeEasy.IntegrationTest.DatabaseManager;

public class PostgresDbManager
{
    public static TestcontainersContainer GetTestContainer()
    {
        return new TestcontainersBuilder<TestcontainersContainer>()
        .WithImage(InternalTestSettingManager.Current.DockerDb.Image)
        .WithName(InternalTestSettingManager.Current.DockerDb.ContainerName)
        .WithEnvironment("POSTGRES_USER", InternalTestSettingManager.Current.DockerDb.UserName)
        .WithEnvironment("POSTGRES_PASSWORD", InternalTestSettingManager.Current.DockerDb.Password)
        .WithEnvironment("POSTGRES_DB", InternalTestSettingManager.Current.DockerDb.DbName)
        .WithCleanUp(true)
        .WithPortBinding(5432, 5432)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();
    }

    /// <summary>
    /// Provide connection string of the test database.
    /// </summary>
    public static string ConnectionString
    {
        get
        {

            return InternalTestSettingManager.Current.UseExternaldb
                ? InternalTestSettingManager.Current.ConnectionString!
                : $"Server=localhost;database={InternalTestSettingManager.Current.DockerDb!.DbName};User Id={InternalTestSettingManager.Current.DockerDb.UserName};password={InternalTestSettingManager.Current.DockerDb.Password};port=5432;";
        }
    }
}
