using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfMoon.Query.ObjectModel
{
    class UnknownEntity : Entity
    {
        /// <inheritdoc />
        public override string Describe()
        {
            var s = $"Warriors Wiki has an article named \"{Name}\".";
            if (Intro != null) s += "\n" + Intro;
            return s;
        }
    }
}
