using System;
using DataRetention.Core.DataEntities;
using DataRetention.Core.Infrastructure;
using NodaTime;

namespace DataRetention.Robot.Core
{
    public interface IDataProvider<T>
    {
        Type DataType { get; }

        HealthTestResult TestHealth();

        // if we use NodaTime.Instant
        //QueryResult<T> Query(Instant from, Instant to);
        //QueryResult<T> Query(Instant from, Instant to, int maxCount);
        QueryResult<T> Query(DateTime dateFrom, DateTime dateTo);
        QueryResult<T> Query(DateTime dateFrom, DateTime dateTo, int maxCount);
    }
}
