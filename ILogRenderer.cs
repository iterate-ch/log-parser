using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;

namespace Analysis
{
    interface ILogRenderer
    {
        void Render(TextWriter writer, IEnumerable<Group> groups, IEnumerable<Item> items, CommandOption template);
    }
}
