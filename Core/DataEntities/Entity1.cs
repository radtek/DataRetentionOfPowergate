using System;

namespace DataRetention.Core.DataEntities
{
    public class Entity1
    {
        public const int SchemaVersion = 1;

        public string Field1 { get; set; }

        public string Field2 { get; set; }

        // use DateTime??? Or NodaTime???  If we stick with DateTime, we should also peform checks on the DateTime zone (enforcing DateTimeKind.UTC)
        public DateTime Timestamp1Utc { get; set; }
        //public Instant Timestamp1 { get; set; }

        public int IntValue1 { get; set; }
    }
}
