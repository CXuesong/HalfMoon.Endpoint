using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
            if (name == null) throw new ArgumentNullException(nameof(name));
            name = name.Trim();
            if (name == "") return null;
            var site = await WikiFamily.GetSiteAsync(LanguageCode);
            var page = await FetchPageAsync(site, name);
            if (page == null) return null;
            var parser = new WikitextParser();
            var root = parser.Parse(page.Content);
            var template = root.EnumDescendants().OfType<Template>()
                .FirstOrDefault(t => distinguishingTemplates.Contains(Utility.NormalizeTitle(t.Name)));
            Entity entity;
            if (template != null)
            {
                switch (Utility.NormalizeTitle(template.Name))
                {
                    case "Book":
                        entity = BuildVolume(root);
                        break;
                    case "Charcat":
                        entity = BuildCat(root);
                        break;
                    default:
                        Debug.Assert(false);
                        return null;
                }
            }
            else if (await page.IsDisambiguationAsync())
            {
                entity = BuildDisambiguation(root);
            }
            else
            {
                entity = BuildUnknown(root);
            }
            entity.Name = page.Title;
            entity.DetailUrl = Utility.GetPageUrl(site, page.Title);
            return entity;
        }

        private async Task<Page> FetchPageAsync(Site site, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            var page = Page.FromTitle(site, name);
            using (var cts = new CancellationTokenSource())
            {
                // Performs open search, and tries to fetch the page of "name" directly.
                var searchTask = site.OpenSearchAsync(name, 2, OpenSearchOptions.ResolveRedirects, cts.Token);
                var pageTask = page.RefreshAsync(PageQueryOptions.FetchContent | PageQueryOptions.ResolveRedirects,
                    cts.Token);
                var finishedTask = await Task.WhenAny(searchTask, pageTask);
                if (finishedTask == pageTask)
                {
                    if (page.Exists)
                    {
                        cts.Cancel();
                        return page;
                    }
                }
                await pageTask;
                var searchResult = await searchTask;
                if (page.Exists) return page;
                if (searchResult.Any())
                {
                    page = Page.FromTitle(site, searchResult.First().Title);
                    // ReSharper disable once MethodSupportsCancellation
                    await page.RefreshAsync(PageQueryOptions.FetchContent | PageQueryOptions.ResolveRedirects);
                    return page;
                }
                return null;
            }
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

        private DisambiguationEntity BuildDisambiguation(Wikitext root)
        {
            var items = root.EnumDescendants().OfType<ListItem>().Select(l => Tuple.Create(l, l.FirstWikiLink()))
                .Where(t => t.Item2 != null).Select(t =>
                {
                    var line = (ListItem) t.Item1.Clone();
                    var firstLink = line.FirstWikiLink();
                    firstLink.Remove();
                    var s = line.StripText().Trim(' ', ',', '?', '.');
                    return new DisambiguationTopic {Target = firstLink.Target.StripText(), Description = s};
                }).ToArray();
            var entity = new DisambiguationEntity
            {
                Intro = root.Lines.FirstOrDefault(l => !(l.Inlines.FirstNode is Template))?.StripText(),
                Topics = items,
            };
            return entity;
        }

        private UnknownEntity BuildUnknown(Wikitext root)
        {
            return new UnknownEntity {Intro = root.ExtractIntro()};
        }
    }
}
