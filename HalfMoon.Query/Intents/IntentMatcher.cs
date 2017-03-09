using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HalfMoon.Query.Intents
{
    public class IntentMatcher
    {
        public Intent Match(string query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            throw new NotImplementedException();
        }
    }
}
