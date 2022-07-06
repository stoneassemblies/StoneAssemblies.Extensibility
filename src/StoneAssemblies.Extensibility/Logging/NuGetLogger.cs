// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetLogger.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Extensibility
{
    using System.Threading.Tasks;

    using NuGet.Common;

    /// <summary>
    ///     The NuGet logger.
    /// </summary>
    public class NuGetLogger : ILogger
    {
        private static NuGetLogger instance;

        private NuGetLogger()
        {
        }

        /// <summary>
        ///     The log debug.
        /// </summary>
        /// <param name="data">
        ///     The data.
        /// </param>
        public void LogDebug(string data)
        {
            Serilog.Log.Debug(data);
        }

        /// <summary>
        ///     The log verbose.
        /// </summary>
        /// <param name="data">
        ///     The data.
        /// </param>
        public void LogVerbose(string data)
        {
            Serilog.Log.Verbose(data);
        }

        /// <summary>
        ///     The log information.
        /// </summary>
        /// <param name="data">
        ///     The data.
        /// </param>
        public void LogInformation(string data)
        {
            Serilog.Log.Information(data);
        }

        /// <summary>
        ///     The log minimal.
        /// </summary>
        /// <param name="data">
        ///     The data.
        /// </param>
        public void LogMinimal(string data)
        {
            Serilog.Log.Information(data);
        }

        /// <summary>
        ///     The log warning.
        /// </summary>
        /// <param name="data">
        ///     The data.
        /// </param>
        public void LogWarning(string data)
        {
            Serilog.Log.Warning(data);
        }

        /// <summary>
        ///     The log error.
        /// </summary>
        /// <param name="data">
        ///     The data.
        /// </param>
        public void LogError(string data)
        {
            Serilog.Log.Error(data);
        }

        /// <summary>
        ///     The log information summary.
        /// </summary>
        /// <param name="data">
        ///     The data.
        /// </param>
        public void LogInformationSummary(string data)
        {
            Serilog.Log.Information(data);
        }

        /// <summary>
        ///     The log.
        /// </summary>
        /// <param name="level">
        ///     The level.
        /// </param>
        /// <param name="data">
        ///     The data.
        /// </param>
        public void Log(LogLevel level, string data)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    this.LogDebug(data);
                    break;
                case LogLevel.Error:
                    this.LogError(data);
                    break;
                case LogLevel.Information:
                    this.LogInformation(data);
                    break;
                case LogLevel.Verbose:
                    this.LogVerbose(data);
                    break;
                case LogLevel.Minimal:
                    this.LogMinimal(data);
                    break;
            }
        }

        /// <summary>
        ///     The log async.
        /// </summary>
        /// <param name="level">
        ///     The level.
        /// </param>
        /// <param name="data">
        ///     The data.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public Task LogAsync(LogLevel level, string data)
        {
            this.Log(level, data);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     The log.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        public void Log(ILogMessage message)
        {
            this.Log(message.Level, message.Message);
        }

        /// <summary>
        ///     The log async.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public async Task LogAsync(ILogMessage message)
        {
            await this.LogAsync(message.Level, message.Message);
        }

        /// <summary>
        ///     Gets the NuGet logger instance
        /// </summary>
        public static ILogger Instance
        {
            get
            {
                return instance ??= new NuGetLogger();
            }
        }
    }
}