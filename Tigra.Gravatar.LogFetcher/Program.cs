using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using NLog;
using NLog.Config;
using NLog.Fluent;
using NLog.Targets;

namespace Tigra.Gravatar.LogFetcher
{
    class Program
    {
        private static Options options;
        private static List<string> errorMessages = new List<string>();
        private const int SignalExtractionFailed = -2;
        private const int SignalSuccess = 0;
        private const int SignalInvalidOptions = -1;

        static async Task Main(string[] args)
        {
            /*
             * 1. Open the log file.
             * 2. Read each line in the log and extract the committer name and email address
             * 3. Build a set of unique committers.
             * 4. Download the Gravatar icon for each committer. [Parallel/Async]
             * 5. Save the image with the correct file name
             */
            var caseInsensitiveParser = new Parser(with =>
                {
                    with.CaseSensitive = false;
                    with.IgnoreUnknownArguments = false;
                    with.HelpWriter = Console.Out;
                    with.AutoVersion = true;
                    with.AutoHelp = true;
                });
            try
            {
                var result = caseInsensitiveParser.ParseArguments<Options>(args);
                result
                    .WithParsed(parsedOptions => options = parsedOptions)
                    .WithNotParsed(errors => errors.ToList().ForEach(e => errorMessages.Add(e.ToString())));
            }
            catch (Exception ex)
            {
                errorMessages.Add(ex.Message);
            }

            if (errorMessages.Any())
                Environment.Exit(SignalInvalidOptions);

            ConfigureLogging(options);
            var exitCode = await PerformTasks(options, errorMessages);
            Environment.Exit(exitCode);
        }

        private static void ConfigureLogging(Options options)
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget("Console")
                {
                    Layout = @"${date:format=HH\:mm\:ss} ${level} ${message} ${exception}"
                };
            config.AddTarget(consoleTarget);
            config.AddRule( options.Verbose ? LogLevel.Debug : LogLevel.Info, LogLevel.Fatal, consoleTarget.Name);
            LogManager.Configuration = config;
            }

        private static async Task<int> PerformTasks(Options options, IList<string> errorsOut)
        {
            try
            {
                var log = new GitLog(options.Repository);
                var committers = log.GetListOfUniqueCommitters();
                var fetcher = new GravatarFetcher(committers);
                //await fetcher.FetchGravatars(options.OutputDirectory, errorsOut);
                fetcher.FetchGravatarsSynchronously(options.OutputDirectory);
                Log.Info().Message("Task complete.").Write();
                await Task.Delay(1); // fudge
                return SignalSuccess;
            }
            catch (Exception ex)
            {
                errorsOut.Add(ex.Message);
                return SignalExtractionFailed;
            }
        }
    }
}
