using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfMoon.Query.ObjectModel
{
    public class DisambiguationEntity : Entity
    {
        public ICollection<DisambiguationTopic> Topics { get; set; }

        /// <inheritdoc />
        public override string Describe()
        {
            var sb = new StringBuilder("There seems to exist ambiguation in your query.");
            if (Intro != null)
            {
                sb.Append(' ');
                sb.Append(Intro);
            }
            sb.AppendLine();
            foreach (var t in Topics) sb.AppendLine(t.ToString());
            return sb.ToString();
        }
    }

    public class DisambiguationTopic
    {
        public string Target { get; set; }

        public string Description { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Description))
                return Target;
            return $"[{Target}], {Description}";
        }
    }
}
