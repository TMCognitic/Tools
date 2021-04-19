using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Connections.Database
{
    public class Command
    {
        internal string Query { get; private set; }
        internal bool IsStoredProcedure { get; private set; }
        internal Dictionary<string, object> Parameters { get; private set; }

        public Command(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("'query' isn't valid!!");

            Query = query;
            Parameters = new Dictionary<string, object>();
        }

        public Command(string query, bool isStoredProcedure)
            : this(query)
        {
            IsStoredProcedure = isStoredProcedure;
        }

        public void AddParameter(string parameterName, object value)
        {
            Parameters.Add(parameterName, value ?? DBNull.Value);
        }
    }
}
