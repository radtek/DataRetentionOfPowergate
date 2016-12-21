using System;
using CommandLine;
using CommandLine.Text;
using DataRetention.Core.Infrastructure;
using DataRetention.Robot.Core;
using System.Configuration;

namespace DataRetention.Robot.Test1
{
    class Program
    {
        private const string RobotId = "TestRobot1";
        private const int RobotVersion = 1;

        private static ITaskServer _taskServer;
        private static IStagingServer _stagingServer;
        private static IEntity1Provider _entity1Provider;
        private static IEntity2Provider _entity2Provider;


        // note that logging needs to be added.  Probably would recommend Log4net - but use whatever you're comfortable with


        static int Main(string[] args)
        {
            try
            {
                // Parse command line arguments - and construct options
                var configOptions = new ConfigOptions();
                if (!configOptions.ParseCommandLine(args))
                {
                    Console.WriteLine("Unknown command line params.  Exiting...");
                    Environment.ExitCode = 1;
                    return 1;
                }

                // Instantiate all providers
                CreateProductionProviders();

                // Create the robot
                var robot = new Test1Robot(RobotId, RobotVersion, _taskServer, _stagingServer, _entity1Provider, _entity2Provider);

                // Set any robot options (using the config file or command line options)
                robot.Verbose = configOptions.Verbose;
                robot.HealthCheckOnly = configOptions.HealthCheck;
                robot.StagingDisabled = configOptions.StagingDisabled;

                // Set the robot running
                robot.Start();
            }
            catch (Exception e)
            {
                // log this!!!!!

                // email/notify the devs!!!!

                Console.WriteLine("Unhandled exception!!!! {0}", e.Message);

                Environment.ExitCode = 1;
                return 1;
            }

            Environment.ExitCode = 0;
            return 0;
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
