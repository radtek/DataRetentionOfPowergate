namespace DataRetention.Core.Infrastructure
{
    public interface ITaskServer
    {
        /// <summary>
        /// Identifier which will be sent to the task server along with all requests
        /// </summary>
        string RobotId { get; }

        /// <summary>
        /// Determine if we can connect to the task server
        /// </summary>
        /// <returns></returns>
        HealthTestResult TestHealth();

        /// <summary>
        /// Report the health of all connected systems to the task server (staging server and all data providers)
        /// </summary>
        /// <param name="robotDiagnostics"></param>
        /// <returns></returns>
        ActionResponse ReportRobotHealth(RobotDiagnostics robotDiagnostics);

        /// <summary>
        /// Ask the task server if the robot is required to run
        /// </summary>
        /// <returns></returns>
        ActionResponse<NewTaskRunResult> RequestNewTaskRun();

        /// <summary>
        /// Tell the task server the robot is starting a new session
        /// </summary>
        /// <returns></returns>
        ActionResponse Starting(string sessionId);

        /// <summary>
        /// Tell the task server that the session failed (and what went wrong)
        /// </summary>
        /// <returns></returns>
        ActionResponse Error(string sessionId, string errorMessage);

        /// <summary>
        /// Tell the task server the robot has finished a session
        /// </summary>
        /// <returns></returns>
        ActionResponse Completed(string sessionId);
    }
}
