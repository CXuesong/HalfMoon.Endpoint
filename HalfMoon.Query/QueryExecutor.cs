using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalfMoon.Query.ObjectModel;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;
using WikiClientLibrary;
using WikiLink = MwParserFromScratch.Nodes.WikiLink;

namespace HalfMoon.Query
{
    // TODO Extract Interface
    public class QueryExecutor
    {
        public QueryExecutor(IFamily wikiFamily)
        {
            if (wikiFamily == null) throw new ArgumentNullException(nameof(wikiFamily));
            WikiFamily = wikiFamily;
        }

        public IFamily WikiFamily { get; }

        public string LanguageCode => "en";

        private static readonly IList<string> DistinguishingTemplates = new[] {"Book"};

        public async Task<Entity> QueryByNameAsync(string name)
        {
            var site = await WikiFamily.GetSiteAsync(LanguageCode);
            var page = Page.FromTitle(site, name);
            await page.RefreshAsync(PageQueryOptions.FetchContent | PageQueryOptions.ResolveRedirects);
            if (!page.Exists) return null;
            var parser = new WikitextParser();
            var root = parser.Parse(page.Content);
            var template = root.EnumDescendants().OfType<Template>()
                .FirstOrDefault(t => DistinguishingTemplates.Contains(MwParserUtility.NormalizeTitle(t.Name)));
            if (template == null) return null;
            switch (MwParserUtility.NormalizeTitle(template.Name))
            {
                case "Book":
                    return BuildVolume(root);
                default:
                    Debug.Assert(false);
                    return null;
            }
        }

        private string ExtractIntro(Wikitext root)
        {
            var lines = root.Lines.TakeWhile(l => !(l is Heading)).NonEmptyLines();
            var s = string.Join("\n", lines.Select(l => l.ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim()));
            if (s == "") return null;
            return s;
        }

        private IEnumerable<LineNode> ExtractSection(Wikitext root, Func<Heading, bool> headingSelector)
        {
            Heading currentHeading = null;
            return root.Lines.SkipWhile(l =>
            {
                var h = l as Heading;
                if (h == null) return false;
                if (headingSelector(h))
                {
                    currentHeading = h;
                    return true;
                }
                return false;
            }).Skip(1).TakeWhile(l =>
            {
                var h = l as Heading;
                if (h?.Level <= currentHeading.Level) return false;
                return true;
            });
        }

        private Volume BuildVolume(Wikitext root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            var infobox =
                root.EnumDescendants().OfType<Template>().First(t => MwParserUtility.NormalizeTitle(t.Name) == "Book");
            var entity = new Volume
            {
                Intro = ExtractIntro(root),
                Author = infobox.Arguments["author"]?.Value.FirstWikiLink()?.ToPlainText(),
                ReleaseDate = infobox.Arguments["publish date"]?.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags),
            };
            {
                var lines = ExtractSection(root, h =>
                        h.ToPlainText().IndexOf("Blurb", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .Select(l => l.ToPlainText(NodePlainTextOptions.RemoveRefTags).Trim());
                entity.Blurb = string.Join("\n", lines);
            }
            return entity;
        }
    }
}
