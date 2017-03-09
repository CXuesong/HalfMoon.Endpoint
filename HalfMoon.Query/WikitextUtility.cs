using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MwParserFromScratch.Nodes;

namespace HalfMoon.Query
{
    internal static class WikitextUtility
    {
        public static string ExtractIntro(this Wikitext root)
        {
            var lines = root.Lines.TakeWhile(l => !(l is Heading)).NonEmptyLines();
            var s = string.Join("\n", lines.Select(l => l.ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim()));
            if (s == "") return null;
            return s;
        }

        public static IEnumerable<LineNode> ExtractSection(this Wikitext root, Func<Heading, bool> headingSelector)
        {
            Heading currentHeading = null;
            foreach (var l in root.Lines)
            {
                var h = l as Heading;
                if (h != null)
                {
                    if (currentHeading == null)
                    {
                        if (headingSelector(h))
                        {
                            currentHeading = h;
                            continue;
                        }
                    }
                    else if (currentHeading.Level >= h.Level)
                    {
                        yield break;
                    }
                }
                if (currentHeading != null) yield return l;
            }
        }

        public static IEnumerable<LineNode> ExtractSection(this Wikitext root, string heading)
        {
            return ExtractSection(root, h => h.ToPlainText().Trim()
                .Equals(heading, StringComparison.CurrentCultureIgnoreCase));
        }

        public static string StripText(this Node node)
        {
            return node.ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim();
        }
    }
}
