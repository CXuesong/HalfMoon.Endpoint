using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;

namespace HalfMoon.Query
{
    internal static class Utility
    {
        public static WikiLink FirstWikiLink(this Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return node.EnumDescendants().OfType<WikiLink>().FirstOrDefault();
        }

        public static IEnumerable<LineNode> NonEmptyLines(this IEnumerable<LineNode> lines)
        {
            return lines.Where(l => l.EnumChildren().OfType<PlainText>().Any(t => !string.IsNullOrWhiteSpace(t.Content)));
        }

        // TODO Apply patch to MwParserFromScratch.
        public static string NormalizeTitle(Node title)
        {
            return MwParserUtility.NormalizeTitle(title).Trim();
        }


        public static string JoinSequence(IEnumerable<string> sequence)
        {
            var collection = sequence as ICollection<string> ?? sequence.ToArray();
            if (collection.Count == 0) return "";
            if (collection.Count == 1) return collection.First();
            var sb = new StringBuilder();
            int i = 0;
            foreach (var item in collection)
            {
                sb.Append(item);
                if (i < collection.Count - 2)
                    sb.Append(", ");
                else if (i == collection.Count - 2)
                    sb.Append(", and ");
                i++;
            }
            return sb.ToString();
        }
    }
}
