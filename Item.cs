using Newtonsoft.Json;
using System;

namespace Analysis
{
    public class Group
    {
        [JsonPropertyAttribute("content")]
        public string Content => Name;

        [JsonPropertyAttribute("id")]
        public string Id => Name;

        [JsonIgnoreAttribute]
        public string Name { get; }

        public Group(string name)
        {
            Name = name;
        }

        public override int GetHashCode() => Name.GetHashCode();
    }

    public abstract class Item
    {
        [JsonPropertyAttribute("content")]
        public string Content { get; }

        [JsonPropertyAttribute("group")]
        public string Group { get; }

        [JsonPropertyAttribute("start")]
        public DateTimeOffset Start { get; }

        [JsonPropertyAttribute("title")]
        public string Title { get; }

        public Item(string content, string group, string title, DateTimeOffset start)
        {
            Content = content;
            Group = group;
            Title = title;
            Start = start;
        }
    }

    public sealed class RangeItem : Item
    {
        [JsonPropertyAttribute("end")]
        public DateTimeOffset End { get; }

        public RangeItem(string content, string group, string title, DateTimeOffset start, DateTimeOffset end) : base(content, group, title, start)
        {
            End = end;
        }
    }

    public sealed class SingleItem : Item
    {
        public SingleItem(string content, string group, string title, DateTimeOffset start) : base(content, group, title, start)
        {
        }
    }
}
