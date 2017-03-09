using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HalfMoon.Query
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class RegexPatternAttribute : Attribute
    {
        public string Pattern { get; }

        public string Culture { get; }

        public bool CaseSensitive { get; set; }

        public RegexPatternAttribute(string pattern, string culture)
        {
            Pattern = pattern;
            Culture = culture;
        }

        public static PropertyInfo MatchProperty(Type entityType, string query, string culture)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (query == null) throw new ArgumentNullException(nameof(query));
            foreach (var prop in entityType.GetRuntimeProperties())
            {
                foreach (var attr in prop.GetCustomAttributes<RegexPatternAttribute>())
                {
                    if (culture == null || attr.Culture == culture)
                    {
                        var opt = RegexOptions.None;
                        if (attr.CaseSensitive) opt |= RegexOptions.IgnoreCase;
                        if (Regex.IsMatch(query, "^(" + attr.Pattern + ")$", opt)) return prop;
                    }
                }
            }
            return null;
        }
    }
}
