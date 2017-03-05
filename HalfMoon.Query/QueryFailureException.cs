using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfMoon.Query
{
    // Reserved for future use.
    public class QueryFailureException : Exception
    {
        public QueryFailureException()
            : base()
        {
        }

        public QueryFailureException(string message)
            : base(message)
        {
        }

        public QueryFailureException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
