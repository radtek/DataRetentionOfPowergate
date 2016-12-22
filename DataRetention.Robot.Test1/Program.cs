using System;
using DataRetention.Core.Infrastructure;
using DataRetention.Robot.Core;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]
namespace DataRetention.Robot.Test1
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static ITaskServer _taskServer;
        private static IStagingServer _stagingServer;
        private static IEntity1Provider _entity1Provider;
        private static IEntity2Provider _entity2Provider;

        private static int Main(string[] args)
        {
            try
            {
                Log.Info("Robot starting");

                if (!ConfigOptions.ParseCommandLine(args))
                {
                    Console.WriteLine("Unknown command line params.  Exiting...");

                    // todo: log this!

                    MailFunctions.SendErrorEmail("DataRetentionRobot " + ConfigOptions.RobotId + " Error", "Bad command line arguments passed");
                        // todo - put the list of args into the email body

                    Environment.ExitCode = 1;
                    return 1;
                }

                // Instantiate all providers
                CreateProductionProviders();

                // Create the robot
                var robot = new Test1Robot(ConfigOptions.RobotId, _taskServer, _stagingServer, _entity1Provider, _entity2Provider);
                robot.Verbose = ConfigOptions.Verbose;
                robot.HealthCheckOnly = ConfigOptions.HealthCheck;
                robot.StagingDisabled = ConfigOptions.StagingDisabled;

                // Set the robot running
                if (!robot.Start())
                {
                    Environment.ExitCode = 1;
                    return 1;
                }
            }
            catch (Exception e)
            {
                // log this!!!!!

                Console.WriteLine("Unhandled exception!!!! {0}", e.Message);

                // email/notify the devs!!!!
                string emailBody = "An unhandled exception was thrown in Data Retention Robot with ID : " + ConfigOptions.RobotId + Environment.NewLine + Environment.NewLine;
                emailBody += "Message: " + e.Message + Environment.NewLine + Environment.NewLine;
                emailBody += "Stack Trace: " + e.StackTrace + Environment.NewLine + Environment.NewLine;
                MailFunctions.SendErrorEmail("DataRetentionRobot " + ConfigOptions.RobotId + " Unhandled Exception!!!", emailBody);

                Environment.ExitCode = 1;
                return 1;
            }

            Environment.ExitCode = 0;
            return 0;
        }

        private static void CreateProductionProviders()
        {
            Log.Debug("creating task server");
            _taskServer = new DummyTaskServer(ConfigOptions.RobotId);
            Log.Debug("creating staging server");
            _stagingServer = new DummyStagingServer(ConfigOptions.RobotId);
            Log.Debug("creating provider for Entity1");
            _entity1Provider = new DummyEntity1Provider();
            Log.Debug("creating provider for Entity2");
            _entity2Provider = new DummyEntity2Provider();
        }
    }
}
