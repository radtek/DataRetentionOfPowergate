using System.Configuration;
using CommandLine;
using CommandLine.Text;

namespace DataRetention.Robot.Test1
{
    public static class ConfigOptions
    {
        #region App.Config Options

        public static string RobotId { get { return ConfigurationManager.AppSettings["RobotId"]; } }

        public static string EmailErrorsTo { get { return ConfigurationManager.AppSettings["EmailErrorsTo"]; } }

        public static string EmailErrorsFrom { get { return ConfigurationManager.AppSettings["EmailErrorsFrom"]; } }

        #endregion

        #region Command Line Options

        private class CommandLineOptions
        {
            [Option("health", Required = false, DefaultValue = false, HelpText = "Display robot connectivity and health.")]
            public bool HealthCheck
            {
                get { return ConfigOptions.HealthCheck; }
                set { ConfigOptions.HealthCheck = value; }
            }

            [Option("nostaging", Required = false, DefaultValue = false, HelpText = "Do not stage data. Only output what data would be staged.")]
            public bool StagingDisabled
            {
                get { return ConfigOptions.StagingDisabled; }
                set { ConfigOptions.StagingDisabled = value; }
            }

            [Option('v', "verbose", Required = false, DefaultValue = false, HelpText = "Display verbose output to console.")]
            public bool Verbose
            {
                get { return ConfigOptions.Verbose; }
                set { ConfigOptions.Verbose = value; }
            }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        public static bool ParseCommandLine(string[] args)
        {
            var options = new CommandLineOptions();
            return Parser.Default.ParseArguments(args, options);
        }

        public static bool HealthCheck { get; set; }

        public static bool StagingDisabled { get; set; }

        public static bool Verbose { get; set; }

        public static string GetUsage()
        {
            var options = new CommandLineOptions();
            return options.GetUsage();
        }

        #endregion

    }
}
