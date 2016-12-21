using System.Collections.Generic;

namespace DataRetention.Robot.Core
{
    public class QueryResult<T>
    {
        public bool QuerySuccess { get; set; }

        public string Message { get; set; }

        public IEnumerable<T> Data { get; set; }
    }
}
