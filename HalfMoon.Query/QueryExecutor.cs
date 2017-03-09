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

        private static readonly IList<string> distinguishingTemplates = new[] {"Book", "Charcat" };

        public async Task<Entity> QueryByNameAsync(string name)
        {
            var site = await WikiFamily.GetSiteAsync(LanguageCode);
            var page = Page.FromTitle(site, name);
            await page.RefreshAsync(PageQueryOptions.FetchContent | PageQueryOptions.ResolveRedirects);
            if (!page.Exists) return null;
            var parser = new WikitextParser();
            var root = parser.Parse(page.Content);
            var template = root.EnumDescendants().OfType<Template>()
                .FirstOrDefault(t => distinguishingTemplates.Contains(Utility.NormalizeTitle(t.Name)));
            if (template == null) return null;
            Entity entity;
            switch (Utility.NormalizeTitle(template.Name))
            {
                case "Book":
                    entity =  BuildVolume(root);
                    break;
                case "Charcat":
                    entity = BuildCat(root);
                    break;
                default:
                    Debug.Assert(false);
                    return null;
            }
            entity.Name = page.Title;
            return entity;
        }

        private Volume BuildVolume(Wikitext root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            var infobox =
                root.EnumDescendants().OfType<Template>().First(t => Utility.NormalizeTitle(t.Name) == "Book");
            var entity = new Volume
            {
                Intro = root.ExtractIntro(),
                Author = infobox.Arguments["author"]?.Value.FirstWikiLink()?.ToPlainText(),
                ReleaseDate = infobox.Arguments["publish date"]?.Value.ToPlainText(NodePlainTextOptions.RemoveRefTags),
            };
            {
                var lines = root.ExtractSection("Blurb").Select(l => l.StripText());
                entity.Blurb = string.Join("\n", lines);
            }
            return entity;
        }

        private Character BuildCat(Wikitext root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            var infobox =
                root.EnumDescendants().OfType<Template>().First(t => Utility.NormalizeTitle(t.Name) == "Charcat");
            var entity = new Character
            {
                Intro = root.ExtractIntro(),
                Age = infobox.Arguments["age"]?.Value.StripText(),
                PastAffiliation = infobox.Arguments["pastaffie"]?.Value.EnumDescendants().OfType<WikiLink>().Select(l => l.Target.StripText()).ToArray(),
                CurrentAffiliation = infobox.Arguments["affie"]?.Value.EnumDescendants().OfType<WikiLink>().Select(l => l.Target.StripText()).ToArray(),
            };
            return entity;
        }
    }
}
