using DataRetention.Core.Infrastructure;
using DataRetention.Robot.Core;

namespace DataRetention.Robot.Test1
{
    class Program
    {
        private const string RobotId = "TestRobot1";

        private static ITaskServer _taskServer;
        private static IStagingServer _stagingServer;
        private static IEntity1Provider _entity1Provider;
        private static IEntity2Provider _entity2Provider;


        // note that logging needs to be added.  Probably would recommend Log4net - but use whatever you're comfortable with


        static void Main(string[] args)
        {
            // Parse command line arguments - and construct options

            // Instantiate all providers
            CreateProductionProviders();

            // Create the robot
            var robot = new Test1Robot(RobotId, _taskServer, _stagingServer, _entity1Provider, _entity2Provider);

            // Set any robot options (using the config file or command line options)

            // Set the robot running
            robot.Start();
        }

        static void CreateProductionProviders()
        {
            _taskServer = new DummyTaskServer(RobotId);
            _stagingServer = new DummyStagingServer(RobotId);
            _entity1Provider = new DummyEntity1Provider();
            _entity2Provider = new DummyEntity2Provider();
        }
    }
}
