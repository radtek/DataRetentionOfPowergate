using DataRetention.Core.DataEntities;

namespace DataRetention.Core.Infrastructure
{
    public interface IStagingServer
    {
        /// <summary>
        /// Identifier which will be sent to the task server along with all requests
        /// </summary>
        string RobotId { get; }

        /// <summary>
        /// Determine if we can connect to the staging server
        /// </summary>
        /// <returns></returns>
        HealthTestResult TestHealth();

        /// <summary>
        /// Stage a data object against a session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        ActionResponse Load(string sessionId, Entity1 data);

        /// <summary>
        /// Abort a session.  All staged data will be rolled back
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        ActionResponse Abort(string sessionId);

        /// <summary>
        /// Commit all loaded data to be fully staged for the session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        ActionResponse Commit(string sessionId);

    }
}
