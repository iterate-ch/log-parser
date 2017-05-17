using System.Collections.Generic;

namespace Analysis
{
    public interface ILogAnalyzer
    {
        Dictionary<string, Group> Groups { get; }

        List<Item> Items { get; }

        List<ItemTemplate> OpenItems { get; }

        void ResolveLine(string line);
    }
}
