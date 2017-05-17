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
            var rendererTypeArgument = app.Argument("rendererType", "Which renderer should be used? Possible values: <visjs>", false);
            var logFileOption = app.Option("-l|--log", "Log File to be analyzed", CommandOptionType.SingleValue);
            var outFileOption = app.Option("-o|--out", "Output File", CommandOptionType.SingleValue);
            var rendererTemplate = app.Option("-t|--template", "Template for <visj> renderer.", CommandOptionType.SingleValue);

            app.HelpOption("-?|--help");

            app.OnExecute(() =>
            {
                var logAnalyzer = default(ILogAnalyzer);
                var logRenderer = default(ILogRenderer);

                switch (logTypeArgument.Value)
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
                switch (rendererTypeArgument.Value)
                {
                    case "visjs":
                        logRenderer = new VisjsLogRenderer();
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
                        try
                        {
                            logAnalyzer.ResolveLine(line);
                        }
                        catch (Exception e)
                        {
                            app.Out.WriteLine(line);
                            throw e;
                        }
                    }
                }

                foreach (var item in logAnalyzer.OpenItems)
                {
                    app.Out.WriteLine($"Did not close {item.Group.Name} {item.Timestamp} {item.Title}");
                }

                var file = "data.js";
                if (outFileOption.HasValue())
                {
                    file = outFileOption.Value();
                }

                using (var stream = File.OpenWrite(file))
                using (var writer = new StreamWriter(stream))
                    logRenderer.Render(writer, logAnalyzer.Groups.Values, logAnalyzer.Items, rendererTemplate);

                return 0;
            });

            return app.Execute(args);
        }
    }
}
