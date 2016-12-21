using DataRetention.Core.DataEntities;
using DataRetention.Core.Infrastructure;

namespace DataRetention.Robot.Test1
{
    public class DummyStagingServer : IStagingServer
    {
        public DummyStagingServer(string robotId)
        {
            RobotId = robotId;
        }

        public string RobotId { get; private set; }

        public HealthTestResult TestHealth()
        {
            return new HealthTestResult {Success = true};
        }

        public ActionResponse Load(string sessionId, Entity1 data)
        {
            return new ActionResponse {Success = true};
        }

        public ActionResponse Load(string sessionId, Entity2 data)
        {
            return new ActionResponse { Success = true };
        }

        public ActionResponse Abort(string sessionId)
        {
            return new ActionResponse { Success = true };
        }

        public ActionResponse Commit(string sessionId)
        {
            return new ActionResponse { Success = true };
        }
    }
}