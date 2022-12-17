using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace AspNetCore.Testing.MadeEasy.Extensions;

/// <summary>
/// Extension for Moq logger
/// </summary>
public static class LoggerTestExtension
{
    /// <summary>
    /// Extension method for verifying ILogger calls
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger"></param>
    /// <param name="expectedMessage">message to verify</param>
    /// <param name="expectedLogLevel">Log level</param>
    /// <param name="times">number of times</param>
    /// <param name="exactMessage">check for contains</param>
    /// <returns></returns>
    public static Mock<ILogger<T>> VerifyLogging<T>(this Mock<ILogger<T>> logger,
        string expectedMessage, LogLevel expectedLogLevel, Times? times = null, bool exactMessage = true)
    {
        times ??= Times.Once();

        Func<object, Type, bool> state = (v, t) =>
        {
            return exactMessage
            ? v.ToString()!.CompareTo(expectedMessage) == 0
            : v.ToString()!.Contains(expectedMessage);
        };

        logger.Verify(
               x => x.Log(
         It.Is<LogLevel>(l => l == expectedLogLevel),
         It.IsAny<EventId>(),
         It.Is<It.IsAnyType>((v, t) => state(v, t)),
         It.IsAny<Exception>(),
         It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!));
        return logger;
    }

}
