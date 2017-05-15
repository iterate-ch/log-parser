using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Analysis
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            var logTypeArgument = app.Argument("logType", "Which Log Type? Possible values: <windows|macos>", false);
            var logFileOption = app.Option("-l|--log", "Log File to be analyzed", CommandOptionType.SingleValue);
            var outFileOption = app.Option("-o|--out", "Output File", CommandOptionType.SingleValue);
            var jsonFormatOption = app.Option("--pretty", "Pretty print json", CommandOptionType.NoValue);
            app.HelpOption("-?|--help");

            app.OnExecute(() =>
            {
                var logTypeValue = logTypeArgument.Value;
                var logAnalyzer = default(ILogAnalyzer);

                switch (logTypeValue)
                {
                    case "windows":
                        logAnalyzer = new WindowsLogAnalyzer(app.Out);
                        break;

                    case "macos":
                        logAnalyzer = new MacOSLogAnalyzer(app.Out);
                        break;

                    default:
                        throw new ArgumentException();
                }

                var logFile = logFileOption.Value();
                var fileInfo = new FileInfo(Environment.ExpandEnvironmentVariables(logFile));
                using (var reader = fileInfo.OpenText())
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        app.Out.WriteLine(line);
                        logAnalyzer.ResolveLine(line);
                    }
                }

                foreach (var item in logAnalyzer.OpenItems)
                {
                    app.Out.WriteLine($"Did not close {item.Group.Name} {item.Timestamp} {item.Title}");
                }

                var formatting = jsonFormatOption.HasValue() ? Formatting.Indented : Formatting.None;
                var groupsData = $"var groupData = {JsonConvert.SerializeObject(logAnalyzer.Groups.Values, formatting)};";
                var itemsData = $"var itemsData = {JsonConvert.SerializeObject(logAnalyzer.Items, formatting)};";

                var file = "data.js";
                if (outFileOption.HasValue())
                {
                    file = outFileOption.Value();
                }
                File.WriteAllText(file, $"{groupsData}\n{itemsData}");

                return 0;
            });

            return app.Execute(args);
        }
    }
}
