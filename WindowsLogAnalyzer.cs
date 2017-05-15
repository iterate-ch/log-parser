using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Analysis
{
    public sealed class WindowsLogAnalyzer : AbstractLogAnalyzer
    {
        private Regex CBFSRegex = new Regex(@"^CbFs([A-Za-z]+)(?:: (.*)|)");
        private Regex GroupRegex = new Regex(@"^\[([^\]]+)\]");
        private Regex RequestRegex = new Regex(@"^([A-Z]+) (.*) HTTP/1\.1");
        private Regex ResponseRegex = new Regex(@"^HTTP/1\.1");
        private Regex TitleRegex = new Regex(@"^[A-Z]+\s+([A-Za-z0-9\.]+)(?:\$[A-Za-z0-9]+|) -");

        public WindowsLogAnalyzer(TextWriter writer) : base(writer)
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

        private ItemTemplate ReadCBFSFilesystem(ref string line, ItemTemplate baseTemplate)
        {
            var match = CBFSRegex.Match(line);
            if (!match.Success) return new InvalidItemTemplate();
            var typeGroup = match.Groups[1];
            var infoGroup = match.Groups[2];
            baseTemplate.Content = typeGroup.Value;
            if (infoGroup.Success)
            {
                baseTemplate.Title = infoGroup.Value;
            }
            return baseTemplate;
        }

        private ItemTemplate ReadFilesystemMountRegistry(ref string line, ItemTemplate baseTemplate)
        {
            var title = baseTemplate.Title;
            baseTemplate.Title = line;
            baseTemplate.Content = title;
            return baseTemplate;
        }

        private ItemTemplate ReadPooledSessionFilesystem(ref string line, ItemTemplate baseTemplate)
        {
            if (line.StartsWith("Borrow"))
            {
                baseTemplate.Open = true;
                baseTemplate.Content = "Using Session";
                return baseTemplate;
            }
            else if (line.StartsWith("Release"))
            {
                var openTemplate = PopItemTemplate(baseTemplate.Group, baseTemplate.Title);
                if (!openTemplate.Valid) return openTemplate;
                return new RangeItemTemplate()
                {
                    Base = openTemplate,
                    Content = openTemplate.Content,
                    Group = baseTemplate.Group,
                    Title = "Request",
                    Timestamp = baseTemplate.Timestamp
                };
            }
            else return new InvalidItemTemplate();
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

        private ItemTemplate ReadSessionFilesystem(ref string line, ItemTemplate baseTemplate)
        {
            return new InvalidItemTemplate();
        }

        private ItemTemplate ResolveContent(ref string line, ItemTemplate baseTemplate)
        {
            switch (baseTemplate.Title)
            {
                case "ch.iterate.mountainduck.fs.SessionFilesystem":
                    return ReadSessionFilesystem(ref line, baseTemplate);

                case "ch.iterate.mountainduck.fs.FilesystemMountRegistry":
                    return ReadFilesystemMountRegistry(ref line, baseTemplate);

                case "ch.cyberduck.transcript.request":
                    return ReadRequest(ref line, baseTemplate);

                case "ch.cyberduck.transcript.response":
                    return ReadResponse(ref line, baseTemplate);

                case "ch.iterate.mountainduck.fs.PooledSessionFilesystem":
                    return ReadPooledSessionFilesystem(ref line, baseTemplate);

                case "Ch.Iterate.Mountainduck.Fs.Cbfs.CBFSFilesystem":
                    return ReadCBFSFilesystem(ref line, baseTemplate);

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

        private ItemTemplate ResolveTemplate(string title)
        {
            switch (title)
            {
                case "ch.iterate.mountainduck.fs.SessionFilesystem":
                case "ch.iterate.mountainduck.fs.FilesystemMountRegistry":
                case "ch.cyberduck.transcript.request":
                case "ch.cyberduck.transcript.response":
                case "ch.iterate.mountainduck.fs.PooledSessionFilesystem":
                case "Ch.Iterate.Mountainduck.Fs.Cbfs.CBFSFilesystem":
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
