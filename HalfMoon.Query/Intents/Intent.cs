using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfMoon.Query.Intents
{
    public class Intent
    {

    }

    [RegexPattern("(What('s)?|Who('s)?) (is |are )?(?<title>.+?)", "en")]
    [RegexPattern("know (.+ about)(?<title>.+?)", "en")]
    [RegexPattern("(知道|了解)(一下)?(什么|谁)是?(?<title>.+?)", "zh")]
    [RegexPattern("(知道|了解)(一下)?(?<title>.+?)是?(什么|谁|是)", "zh")]
    public class AskEntityIntent : Intent
    {
        public string Title { get; }

        public AskEntityIntent(string title)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            Title = title;
        }
    }

    public class AskPropertyIntent : AskEntityIntent
    {
        public string Attribute { get; }

        /// <inheritdoc />
        public AskPropertyIntent(string title, string attribute) : base(title)
        {
            Attribute = attribute;
        }
    }
}
