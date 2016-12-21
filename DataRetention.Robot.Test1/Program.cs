using System;
using CommandLine;
using CommandLine.Text;
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


        class Options
        {
            [Option("health", Required = false, DefaultValue = false, HelpText = "Display robot connectivity and health.")]
            public bool HealthCheck { get; set; }

            [Option("nostaging", Required = false, DefaultValue = false, HelpText = "Do not stage data. Only output what data would be staged.")]
            public bool StagingDisabled { get; set; }

            [Option('v', "verbose", Required = false, DefaultValue = false, HelpText = "Display verbose output to console.")]
            public bool Verbose { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static int Main(string[] args)
        {
            try
            {
                // Parse command line arguments - and construct options
                var options = new Options();
                if (!Parser.Default.ParseArguments(args, options))
                {
                    Console.WriteLine("Unknown command line params.  Exiting...");
                    Environment.ExitCode = 1;
                    return 1;
                }

                // Instantiate all providers
                CreateProductionProviders();

                // Create the robot
                var robot = new Test1Robot(RobotId, _taskServer, _stagingServer, _entity1Provider, _entity2Provider);

                // Set any robot options (using the config file or command line options)
                // Values are available here
                robot.Verbose = options.Verbose;
                robot.HealthCheckOnly = options.HealthCheck;
                robot.StagingDisabled = options.StagingDisabled;


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
