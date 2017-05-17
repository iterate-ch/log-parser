using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.Text;

namespace Analysis
{
    public class VisjsLogRenderer : ILogRenderer
    {
        public void Render(TextWriter writer, IEnumerable<Group> groups, IEnumerable<Item> items, CommandOption template)
        {
            if (!template.HasValue())
                throw new InvalidOperationException("Template may not be empty");

            var start = items.Min(x => x.Start);
            var end = start + TimeSpan.FromSeconds(1.5);

            var min = start.Floor(new TimeSpan(0, 0, 1));
            var max = items.Max(x => x.Start).Ceil(new TimeSpan(0, 0, 1));

            var groupsData = JsonConvert.SerializeObject(groups, Formatting.None);
            var itemsData = JsonConvert.SerializeObject(items, Formatting.None);

            var templateContent = new StringBuilder(File.ReadAllText(template.Value()));
            templateContent.Replace("$min", min.ToString("o"));
            templateContent.Replace("$max", max.ToString("o"));
            templateContent.Replace("$start", start.ToString("o"));
            templateContent.Replace("$end", end.ToString("o"));

            templateContent.Replace("$groupsData", groupsData);
            templateContent.Replace("$itemsData", itemsData);

            writer.WriteLine(templateContent);
        }
    }
}
