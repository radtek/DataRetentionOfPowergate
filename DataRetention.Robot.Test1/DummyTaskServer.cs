using System;
using DataRetention.Core.Infrastructure;

namespace DataRetention.Robot.Test1
{
    public class DummyTaskServer : ITaskServer
    {
        public DummyTaskServer(string robotId)
        {
            RobotId = robotId;
        }

        public string RobotId { get; private set; }

        public HealthTestResult TestHealth()
        {
            return new HealthTestResult { Success = true };
        }

        public ActionResponse ReportRobotHealth(RobotDiagnostics robotDiagnostics)
        {
            return new ActionResponse {Success = true};
        }

        public ActionResponse<NewTaskRunResult> RequestNewTaskRun()
        {
            return new ActionResponse<NewTaskRunResult>
                {
                    Success = true,
                    Response = new NewTaskRunResult { RunRequired = true, SessionId = "unique-session-id-123", DateFrom = new DateTime(2016, 12, 1), DateTo = new DateTime(2016, 12, 2) }
                };
        }

        public ActionResponse Starting(string sessionId)
        {
            return new ActionResponse { Success = true };
        }

        public ActionResponse Error(string sessionId, string errorMessage)
        {
            return new ActionResponse { Success = true };
        }

        public ActionResponse Completed(string sessionId)
        {
            return new ActionResponse { Success = true };
        }
    }
}
