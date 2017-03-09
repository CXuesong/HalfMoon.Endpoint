using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfMoon.Query.ObjectModel
{
    /// <summary>
    /// A volume of the fiction series.
    /// </summary>
    public class Volume : Entity
    {
        
        [RegexPattern("author|writer", "en")]
        public string Author { get; set; }

        //public string ArcName { get; set; }

        [RegexPattern("(publication|publish|release) date", "en")]
        public string ReleaseDate { get; set; }

        [RegexPattern("blurb|intro(duct(ion)?)?", "en")]
        public string Blurb { get; set; }

        /// <inheritdoc />
        public override string Describe()
        {
            var s = Intro;
            if (Author != null) s += " The book is written by " + Author + ".";
            if (ReleaseDate != null) s += " The release date is " + ReleaseDate + ".";
            if (Blurb != null) s += "\n\n" + Blurb;
            return s;
        }

    }
}
