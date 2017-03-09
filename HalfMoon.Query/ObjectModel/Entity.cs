using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfMoon.Query.ObjectModel
{
    /// <summary>
    /// An immutable (by convention) entity object.
    /// </summary>
    public class Entity
    {

        public static readonly object NotFound = new object();

        public Entity()
        {

        }

        [RegexPattern("name|title|caption", "en")]
        [RegexPattern("名字|标题|题名", "en")]
        public string Name { get; set; }

        public string Intro { get; set; }

        public string DetailUrl { get; set; }

        public virtual string Describe()
        {
            return Intro ?? Name;
        }

        public virtual object AskProperty(string query, string culture)
        {
            var property = RegexPatternAttribute.MatchProperty(this.GetType(), query, culture);
            if (property != null) return property.GetValue(this);
            return NotFound;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}

