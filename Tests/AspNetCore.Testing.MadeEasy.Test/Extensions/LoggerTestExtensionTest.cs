using AspNetCore.Testing.MadeEasy.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspNetCore.Testing.MadeEasy.Test.Extensions
{
    public class LoggerTestExtensionTest
    {
        [Fact]
        public void Test()
        {
            var logger = new Mock<ILogger<TestLogger>>();

            _ = new TestLogger(logger.Object);

            logger.VerifyLogging("Test information", LogLevel.Information);
            logger.VerifyLogging("Test warning", LogLevel.Warning);
            logger.VerifyLogging("Some error", LogLevel.Error, Times.Exactly(2));
            logger.VerifyLogging("Debug", LogLevel.Debug, Times.Exactly(2), false);
        }
    }

    public class TestLogger
    {
        public TestLogger(ILogger<TestLogger> logger)
        {
            logger.LogInformation("Test information");
            logger.LogWarning("Test warning");

            logger.LogError("Some error");
            logger.LogError("Some error");

            logger.LogDebug("Debug detail");
        }
    }
}
