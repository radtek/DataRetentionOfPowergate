namespace DataRetention.Core.Infrastructure
{
    public class HealthTestResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }
    }

    /// <summary>
    /// Carries details of the connectivity and health status of connected systems
    /// </summary>
    public class RobotDiagnostics
    {
        /// <summary>
        /// The name of the server hosting the robot
        /// </summary>
        public string ServerLocation { get; set; }

        /// <summary>
        /// Path of the robot executable
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Health reported as Ok for the staging server and all available data providers
        /// </summary>
        public bool AllProvidersHealthy
        {
            get
            {
                if (Entity1ProviderHealth != null && Entity1ProviderHealth.Success == false)
                    return false;
                if (Entity2ProviderHealth != null && Entity2ProviderHealth.Success == false)
                    return false;
                return StagingServerHealth != null && StagingServerHealth.Success;
            }
        }

        public HealthTestResult Entity1ProviderHealth { get; set; }
        public HealthTestResult Entity2ProviderHealth { get; set; }
        public HealthTestResult StagingServerHealth { get; set; }
    }
}