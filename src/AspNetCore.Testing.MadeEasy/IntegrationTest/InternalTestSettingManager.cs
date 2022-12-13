using Microsoft.Extensions.Configuration;
using System.IO;

namespace AspNetCore.Testing.MadeEasy.IntegrationTest;

/// <summary>
/// Manages settings for the testing. It's a singleton class. To use it access it through <see cref="Current"/>
/// </summary>
public class InternalTestSettingManager
{
    private static InternalTestSetting _setting;

    private static InternalTestSetting GetTestingSetting()
    {
        var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: false)
        .AddEnvironmentVariables();

        var configuration = builder.Build();

        return configuration.GetSection(nameof(InternalTestSetting)).Get<InternalTestSetting>();


    }

    /// <summary>
    /// Provide access to <see cref="InternalTestSettingManager" instance/>
    /// </summary>
    public static InternalTestSetting Current
    {
        get
        {
            if (_setting == null)
            {
                _setting = GetTestingSetting();
            }

            return _setting;
        }
    }
}

/// <summary>
/// Holds test setting data
/// </summary>
public class InternalTestSetting
{
    public bool UseExternaldb { get; set; }
    public string ConnectionString { get; set; }
    public DockerSetting DockerDb { get; set; }

}

/// <summary>
/// Hold test docker db settings
/// </summary>
public class DockerSetting
{
    public string Image { get; set; }
    public string ContainerName { get; set; }
    public string DbName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
