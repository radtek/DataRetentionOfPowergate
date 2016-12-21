using System.Configuration;
using CommandLine;
using CommandLine.Text;

namespace DataRetention.Robot.Test1
{
    public class ConfigOptions
    {
        #region App.Config Options

        public string TaskServerApiUrlProduction
        {
            get { return ConfigurationManager.AppSettings["TaskServerApiUrl"]; }
        }

        public string StagingServerConnectionStringProduction
        {
            get { return ConfigurationManager.AppSettings["StagingServerConnectionString"]; }
        }

        public string TaskServerApiUrlTesting
        {
            get { return ConfigurationManager.AppSettings["TaskServerApiUrl-Testing"]; }
        }

        public string StagingServerConnectionStringTesting
        {
            get { return ConfigurationManager.AppSettings["StagingServerConnectionString-Testing"]; }
        }

        #endregion

        #region Command Line Options

        public bool ParseCommandLine(string[] args)
        {
            return Parser.Default.ParseArguments(args, this);
        }

        [Option("environment", Required = false, DefaultValue = "production", HelpText = "Connection strings to use (production|testing).")]
        public string Environment { get; set; }

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

        #endregion

    }
}
