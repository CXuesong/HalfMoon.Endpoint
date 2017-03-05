using System;
using System.Collections.Generic;
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
        public Entity()
        {

        }

        public string Name { get; set; }

        public string Intro { get; set; }

        public string DetailUrl { get; set; }

        public virtual string Describe()
        {
            return Intro ?? Name;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
