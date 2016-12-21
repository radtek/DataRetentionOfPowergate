using System;

namespace DataRetention.Core.Infrastructure
{
    public class NewTaskRunResult
    {
        public bool RunRequired { get; set; }

        public string SessionId { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}