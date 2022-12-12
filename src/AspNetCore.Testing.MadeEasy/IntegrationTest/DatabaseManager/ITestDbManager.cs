namespace AspNetCore.Testing.MadeEasy.IntegrationTest.DatabaseManager;

/// <summary>
/// Manages test database container setup
/// </summary>
internal interface ITestDbManager
{
    //TODO: Uncomment once will move to .net 7
    //public abstract static TestcontainersContainer GetTestContainer();
    public static string ConnectionString { get; }
}
