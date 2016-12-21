using System;
using System.Collections.Generic;
using DataRetention.Core.DataEntities;
using DataRetention.Core.Infrastructure;
using DataRetention.Robot.Core;

namespace DataRetention.Robot.Test1
{
    public class DummyEntity1Provider : IEntity1Provider
    {
        public Type DataType { get { return typeof (Entity1); } }

        public HealthTestResult TestHealth()
        {
            return new HealthTestResult { Success = true };
        }

        // we might want to use just plain DateTime (with a UTC name extension)
        public QueryResult<Entity1> Query(DateTime dateFrom, DateTime dateTo)
        {
            var queryResult = new QueryResult<Entity1>
                {
                    QuerySuccess = true,
                    Data = new List<Entity1>
                        {
                            new Entity1
                                {
                                    Field1 = "a value",
                                    Field2 = "another value",
                                    IntValue1 = 42,
                                    Timestamp1Utc = new DateTime(2016, 12, 15, 0, 0, 0, DateTimeKind.Utc)
                                },
                            new Entity1
                                {
                                    Field1 = "a quick brown fox",
                                    Field2 = "jumped ....",
                                    IntValue1 = 22,
                                    Timestamp1Utc = new DateTime(2016, 12, 15, 1, 30, 0, DateTimeKind.Utc)
                                }
                        }
                };
            return queryResult;
        }

        public QueryResult<Entity1> Query(DateTime dateFrom, DateTime dateTo, int maxCount)
        {
            return Query(dateFrom, dateTo);
        }

        public string FriendlyDisplay(Entity1 data)
        {
            return string.Format("Field1: '{0}', Field2: '{1}'", data.Field1, data.Field2);
        }
    }

    public class DummyEntity2Provider : IEntity2Provider
    {
        public Type DataType { get { return typeof(Entity2); } }

        public HealthTestResult TestHealth()
        {
            return new HealthTestResult { Success = true };
        }

        // we might want to use just plain DateTime (with a UTC name extension)
        public QueryResult<Entity2> Query(DateTime dateFrom, DateTime dateTo)
        {
            var queryResult = new QueryResult<Entity2>
            {
                QuerySuccess = true,
                Data = new List<Entity2>
                        {
                            new Entity2
                                {
                                    Field1 = "a value",
                                    Field2 = "another value",
                                    BoolField1 = true,
                                    Field3 = "blah"
                                },
                            new Entity2
                                {
                                    Field1 = "a quick brown fox",
                                    Field2 = "jumped ....",
                                    BoolField1 = false,
                                    Field3 = "xyz"
                                }
                        }
            };
            return queryResult;
        }

        public QueryResult<Entity2> Query(DateTime dateFrom, DateTime dateTo, int maxCount)
        {
            return Query(dateFrom, dateTo);
        }

        public string FriendlyDisplay(Entity2 data)
        {
            return string.Format("Field1: '{0}', Field2: '{1}'", data.Field1, data.Field2);
        }
    }

}
