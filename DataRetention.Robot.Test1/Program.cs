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

        // NOTE: all interactions with remote systems or specific data providers will go through pre-defined interfaces.
        private static ITaskServer _taskServer;
        private static IStagingServer _stagingServer;

        // these are examples only.  there will be no such things as Entity1 and Entity2 - they are examples of what different types of data entities might look like.
        // in reality, there will be provider interfaces defined for each type of data
        private static IEntity1Provider _entity1Provider;
        private static IEntity2Provider _entity2Provider;

        private static int Main(string[] args)
        {
            try
            {
                // please note that comprehensive logging will be required.  Log4net has been added to the solution - but the developer will need to
                // ensure that it is being used properly everywhere
                Log.Info("Robot starting");

                // if there are any command line options that 
                if (!ConfigOptions.ParseCommandLine(args))
                {
                    Console.WriteLine("Unknown command line params.  Exiting...");

                    // todo: log this!

                    MailFunctions.SendErrorEmail("DataRetentionRobot " + ConfigOptions.RobotId + " Error", "Bad command line arguments passed");
                        // todo - put the list of args into the email body

                    // exit with error code
                    Environment.ExitCode = 1;
                    return 1;
                }

                // Instantiate all providers
                CreateProductionProviders();

                // Create the robot - and provide it with implementations of all interfaces it needs (it only deals with abstractions)
                var robot = new Test1Robot(ConfigOptions.RobotId, _taskServer, _stagingServer, _entity1Provider, _entity2Provider);
                // these are examples of what command line options *might* do (possible for development/debugging purposes).  These are examples only.
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
            // dummy providers have been created here - but these must be properly developed for each system.
            // classes which talk to the task server and the staging server will be defined once and shared across all solutions as a class library.
            // these providers are only stubbed dummies

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
