using System;
using System.Collections.Generic;
using System.IO;

namespace Analysis
{
    public abstract class AbstractLogAnalyzer : ILogAnalyzer
    {
        public Dictionary<string, Group> Groups { get; } = new Dictionary<string, Group>();

        public List<Item> Items { get; } = new List<Item>();

        public List<ItemTemplate> OpenItems { get; } = new List<ItemTemplate>();

        protected TextWriter Writer { get; }

        public AbstractLogAnalyzer(TextWriter writer)
        {
            Writer = writer;
        }

        public void ResolveLine(string line)
        {
            var template = ResolveSingleLine(line);
            if (!template.Valid) return;
            else if (template.Open) OpenItems.Add(template);
            else
            {
                var item = template.Build();
                if (!string.IsNullOrWhiteSpace(item.Content))
                {
                    Items.Add(template.Build());
                }
                else
                {
                    Writer.WriteLine($"Content on {line} empty.");
                }
            }
        }

        protected ItemTemplate PopItemTemplate(Group group, string title)
        {
            for (int i = 0; i < OpenItems.Count; i++)
            {
                var item = OpenItems[i];
                if (item.Group != group) continue;
                if (item.Title != title) continue;
                try
                {
                    return item;
                }
                finally
                {
                    OpenItems.RemoveAt(i);
                }
            }
            return new InvalidItemTemplate();
        }

        protected DateTimeOffset ResolveDateTime(ref string line)
        {
            try
            {
                return DateTimeOffset.Parse(line.Substring(0, 23));
            }
            finally
            {
                line = line.Substring(24);
            }
        }

        protected abstract ItemTemplate ResolveSingleLine(string line);
    }
}
