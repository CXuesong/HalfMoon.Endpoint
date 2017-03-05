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


        //public static Node NextNodeOuter(this Node node)
        //{
        //    if (node == null) throw new ArgumentNullException(nameof(node));
        //    while (node.NextNode == null)
        //    {
        //        if (node.ParentNode == null) return null;
        //        node = node.ParentNode;
        //    }
        //    return node.NextNode;
        //}
    }
}
