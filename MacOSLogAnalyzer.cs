using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Analysis
{
    public sealed class MacOSLogAnalyzer : AbstractLogAnalyzer
    {
        private Regex GroupRegex = new Regex(@"^Mountain Duck\[[^\]]+\] \[([^\]]+)\]");
        private Regex RequestRegex = new Regex(@"^([A-Z]+) (.*) HTTP/1\.1");
        private Regex ResponseRegex = new Regex(@"^HTTP/1\.1");
        private Regex TitleRegex = new Regex(@"^[A-Z]+\s+([A-Za-z0-9\.]+) -");

        public MacOSLogAnalyzer(TextWriter writer) : base(writer)
        {
        }

        protected override ItemTemplate ResolveSingleLine(string line)
        {
            var dateTime = ResolveDateTime(ref line);
            var group = ResolveGroup(ref line);
            var title = ResolveTitle(ref line);
            var baseTemplate = ResolveTemplate(title);
            if (baseTemplate.Valid)
            {
                baseTemplate.Timestamp = dateTime;
                baseTemplate.Group = group;
                baseTemplate.Title = title;

                return ResolveContent(ref line, baseTemplate);
            }

            return baseTemplate;
        }

        private ItemTemplate ReadRequest(ref string line, ItemTemplate baseTemplate)
        {
            var match = RequestRegex.Match(line);
            if (!match.Success) return new InvalidItemTemplate();
            var method = match.Groups[1].Value;
            var path = match.Groups[2].Value;
            baseTemplate.Content = $"{method} {path}";
            baseTemplate.Open = true;

            return baseTemplate;
        }

        private ItemTemplate ReadResponse(ref string line, ItemTemplate baseTemplate)
        {
            var match = ResponseRegex.Match(line);
            if (!match.Success) return new InvalidItemTemplate();
            var openTemplate = PopItemTemplate(baseTemplate.Group, "ch.cyberduck.transcript.request");
            if (!openTemplate.Valid) return openTemplate;
            return new RangeItemTemplate()
            {
                Base = openTemplate,
                Content = "Request",
                Group = baseTemplate.Group,
                Title = openTemplate.Content,
                Timestamp = baseTemplate.Timestamp
            };
        }

        private ItemTemplate ResolveContent(ref string line, ItemTemplate baseTemplate)
        {
            switch (baseTemplate.Title)
            {
                case "ch.cyberduck.transcript.request":
                    return ReadRequest(ref line, baseTemplate);

                case "ch.cyberduck.transcript.response":
                    return ReadResponse(ref line, baseTemplate);

                case "ch.iterate.mountainduck.fs.nfs.NfsFilesystem":
                    return ResolveNfsFilesystem(ref line, baseTemplate);

                default:
                    return new InvalidItemTemplate();
            }
        }

        private Group ResolveGroup(ref string line)
        {
            var match = GroupRegex.Match(line);
            if (!match.Success) throw new Exception(line);
            var groupName = match.Groups[1].Value;
            if (groupName.StartsWith("http"))
                groupName = "http";
            try
            {
                var result = default(Group);
                if (!Groups.TryGetValue(groupName, out result))
                {
                    Groups[groupName] = result = new Group(groupName);
                }
                return result;
            }
            finally
            {
                line = line.Substring(match.Length + 1);
            }
        }

        private ItemTemplate ResolveNfsFilesystem(ref string line, ItemTemplate baseTemplate)
        {
            if (line.StartsWith("getattr") || line.StartsWith("Return"))
            {
                return new InvalidItemTemplate();
            }

            baseTemplate.Content = line;

            return baseTemplate;
        }

        private ItemTemplate ResolveTemplate(string title)
        {
            switch (title)
            {
                case "ch.cyberduck.transcript.request":
                case "ch.cyberduck.transcript.response":
                case "ch.iterate.mountainduck.fs.nfs.NfsFilesystem":
                    return new SingleItemTemplate();

                default:
                    return new InvalidItemTemplate();
            }
        }

        private string ResolveTitle(ref string line)
        {
            var match = TitleRegex.Match(line);
            if (!match.Success) throw new Exception(line);
            try
            {
                return match.Groups[1].Value;
            }
            finally
            {
                line = line.Substring(match.Length + 1);
            }
        }
    }
}
