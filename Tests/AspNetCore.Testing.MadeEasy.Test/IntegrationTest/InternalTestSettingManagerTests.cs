using AspNetCore.Testing.MadeEasy.IntegrationTest;

namespace AspNetCore.Testing.MadeEasy.Test.IntegrationTest;

public class InternalTestSettingManagerTests
{
    [Fact]
    public void Current_should_able_tp_provide_configurations_setting()
    {
        var a = InternalTestSettingManager.Current;

        Assert.Equal("example_postgris", a.DockerDb.ContainerName);
        Assert.Equal("kartoza/postgis", a.DockerDb.Image);
        Assert.Equal("Server=localhost;database=example;User Id=postgres;password=welcome;port=5432;",
            a.ConnectionString);
        Assert.Equal(3, a.DockerDb.EnviromentVariables.Count);
    }
}
