using AspNetCore.Testing.MadeEasy.IntegrationTest;

namespace AspNetCore.Testing.MadeEasy.Test.IntegrationTest;

public class DatabaseManagerTest
{
    [Fact]
    public void ConnectionString_should_return_the_configured_connection_string()
    {
        Assert.Equal("Server=localhost;database=example;User Id=postgres;password=welcome;port=5432;",
            DatabaseManager.ConnectionString);
    }
}
