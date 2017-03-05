using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HalfMoon.Query.ObjectModel;
using WikiClientLibrary;
using WikiClientLibrary.Client;

namespace HalfMoon.Query
{
    /// <summary>
    /// Queries Warriors Wiki.
    /// </summary>
    public class QueryEngine : IDisposable
    {
        private readonly Family family;
        private readonly WikiClient client;
        private readonly QueryExecutor executor;

        public QueryEngine()
        {
            client = new WikiClient {ClientUserAgent = "HalfMoon/0.1 (https://github.com/CXuesong/HalfMoon.Endpoint)"};
            family = new Family(client, "Warriors Wiki");
            family.Register("en", "http://warriors.wikia.com/api.php");
            family.Register("zh", "http://warriors.huiji.wiki/api.php");
            executor = new QueryExecutor(family);
        }

        public Task<Entity> ExecuteQueryAsync(string query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            query = query.Trim();
            if (query == "") return null;
            query = Regex.Replace(query, @"[\s_]+", " ");
            return executor.QueryByNameAsync(query);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            client.Dispose();
        }
    }
}
