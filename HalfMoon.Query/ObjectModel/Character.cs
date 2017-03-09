using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfMoon.Query.ObjectModel
{
    public class Character : Entity
    {

        public string Age { get; set; }

        public ICollection<string> PastAffiliation { get; set; } = new string[0];

        public ICollection<string> CurrentAffiliation { get; set; } = new string[0];

        public IDictionary<string, string[]> Relatives { get; set; }

        /// <inheritdoc />
        public override string Describe()
        {
            var pron = "It";
            var s = Intro;
            if (s.Contains(" tom") || s.Contains("male ")) pron = "He";
            if (s.Contains("she-")) pron = "She";
            if (Age != null) s += $" {pron} is {Age}.";
            if (PastAffiliation.Count > 0) s += $" {pron} has been in {Utility.JoinSequence(PastAffiliation)}.";
            if (CurrentAffiliation.Count > 0) s += $" Now, {pron.ToLower()} is in {Utility.JoinSequence(CurrentAffiliation)}.";
            return s;
        }
    }
}
