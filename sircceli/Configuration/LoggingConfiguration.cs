using Microsoft.Extensions.Logging;

namespace sircceli.Configuration;

public static class LoggingConfiguration
{
    extension(ILoggingBuilder builder)
    {
        public ILoggingBuilder AddSircceliLogging()
        {
            return builder.ConfigureLogging();
        }

        private ILoggingBuilder ConfigureLogging()
        {
            return builder;
        }
    }
}