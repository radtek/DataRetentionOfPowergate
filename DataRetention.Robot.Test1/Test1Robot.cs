using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DataRetention.Core.DataEntities;
using DataRetention.Core.Infrastructure;
using DataRetention.Robot.Core;

namespace DataRetention.Robot.Test1
{
    public class Test1Robot
    {
        private readonly string _robotId;
        private readonly ITaskServer _taskServer;
        private readonly IStagingServer _stagingServer;
        private readonly IEntity1Provider _entity1Provider;
        private readonly IEntity2Provider _entity2Provider;

        public Test1Robot(string robotId, ITaskServer taskServer, IStagingServer stagingServer, IEntity1Provider entity1Provider, IEntity2Provider entity2Provider)
        {
            _robotId = robotId;
            _taskServer = taskServer;
            _stagingServer = stagingServer;
            _entity1Provider = entity1Provider;
            _entity2Provider = entity2Provider;

            if (string.IsNullOrWhiteSpace(_robotId))
                throw new ArgumentNullException("robotId", "Robot created with no Id!");

            if (_taskServer == null)
                throw new ArgumentNullException("taskServer", "Robot created with no task server defined!");

            if (_stagingServer == null)
                throw new ArgumentNullException("stagingServer", "Robot created with no staging server defined!");
            
            // for any data types that this robot needs to handle, ensure there is a valid provider ...
            if (_entity1Provider == null)
                throw new ArgumentNullException("entity1Provider", "Robot created with no provider for Entity1 data");

            if (_entity2Provider == null)
                throw new ArgumentNullException("entity2Provider", "Robot created with no provider for Entity2 data");
        }

        public bool Verbose { get; set; }
        public bool HealthCheckOnly { get; set; }
        public bool StagingDisabled { get; set; }

        private RobotDiagnostics _robotDiagnostics;

        public bool Start()
        {
            VerboseOutput(" * Verbose mode selected.");

            if (StagingDisabled)
            {
                VerboseOutput(" * Pretend mode selected (no data will be staged).");
            }

            if (HealthCheckOnly)
            {
                VerboseOutput(" * Health check only.");
            }


            // Test connectivity of all providers and staging server
            _robotDiagnostics = PerformDiagnostics();
            if (Verbose || HealthCheckOnly)
            {
                Console.WriteLine(FormatDiagnosticsMessage(_robotDiagnostics));
            }

            if (HealthCheckOnly)
            {
                VerboseOutput("Health check completed.  Exiting.");
                return true;
            }

            // Connect to task server and :
            //   - report systems connectivity
            //   - ask server if a new run is required (along with parameters for run)
            // (on error, send error notification and abort)
            ActionResponse actionResponse = _taskServer.ReportRobotHealth(_robotDiagnostics);
            if (!actionResponse.Success)
            {
                // We failed connecting to the task server!!!!!  Need to fall back to manually alerting someone...
                SendErrorEmail("Error", "Failed reporting Robot health to task server! " + actionResponse.Message);
                return false;
            }

            ActionResponse<NewTaskRunResult> requestNewTaskRunResult = _taskServer.RequestNewTaskRun();
            if (!requestNewTaskRunResult.Success)
            {
                // We failed connecting to the task server!!!!!  Need to fall back to manually alerting someone...
                SendErrorEmail("Error", "Failed requesting new task run with task server! " + requestNewTaskRunResult.Message);
                return false;
            }

            if (!requestNewTaskRunResult.Response.RunRequired)
            {
                // The task server has told us we don't need to run.  Nothing left to do.
                return true;
            }

            return PerformTask(requestNewTaskRunResult.Response);
        }

        private RobotDiagnostics PerformDiagnostics()
        {
            var results = new RobotDiagnostics();

            // gather data about the current hosting machine
            results.ServerName = Dns.GetHostName();
            results.ServerPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var host = Dns.GetHostEntry(results.ServerName);
            results.ServerIPAddress = host.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

            // ping the connectivity of the remote services and providers
            results.TaskServerHealth = _taskServer.TestHealth();
            results.StagingServerHealth = _stagingServer.TestHealth();
            if (_entity1Provider != null)
                results.Entity1ProviderHealth = _entity1Provider.TestHealth();
            if (_entity2Provider != null)
                results.Entity2ProviderHealth = _entity2Provider.TestHealth();

            return results;
        }

        private bool PerformTask(NewTaskRunResult taskParameters)
        {
            // tell the task server that we're starting the run
            ActionResponse actionResponse = _taskServer.Starting(taskParameters.SessionId);
            if (!actionResponse.Success)
            {
                // The call to the task server failed!!!!!  Need to fall back to manually alerting someone...
                SendErrorEmail("Error", "Failed starting task! " + actionResponse.Message);
                return false;
            }

            // Iterate through all data types we have (in appropriate order) and
            if (!StageDataFromProvider(taskParameters, _entity1Provider))
            {
                return false;
            }

            if (!StageDataFromProvider(taskParameters, _entity2Provider))
            {
                return false;
            }

            // When done, commit the staging server data set
            actionResponse = _stagingServer.Commit(taskParameters.SessionId);
            if (!actionResponse.Success)
            {
                AbortSession(taskParameters.SessionId, "Error committing staged data: " + actionResponse.Message);
                return false;
            }

            actionResponse = _taskServer.Completed(taskParameters.SessionId);
            if (!actionResponse.Success)
            {
                // Completing the session on the task server failed!!!!!  Need to fall back to manually alerting someone...
                SendErrorEmail("Error", "Failed completing task! " + actionResponse.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tell the task server that the session failed.  If that call fails, then a fallback message to the developers will be sent
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="errorMessage"></param>
        private void AbortSession(string sessionId, string errorMessage)
        {
            ActionResponse actionResponse = _taskServer.Error(sessionId, errorMessage);
            if (!actionResponse.Success)
            {
                SendErrorEmail("Error", "Failed notifying task server of error on session #" + sessionId + ". " + actionResponse.Message);
            }

            actionResponse = _stagingServer.Abort(sessionId);
            if (!actionResponse.Success)
            {
                SendErrorEmail("Error", "Failed aborting session #" + sessionId + " on staging server! " + actionResponse.Message);
            }
        }

        private bool StageDataFromProvider(NewTaskRunResult taskParameters, IEntity1Provider dataProvider)
        {
            if (StagingDisabled)
            {
                Console.WriteLine();
                Console.WriteLine("Staging is disabled for datatype: {0}. Query results displayed below...", dataProvider.DataType.Name);
            }

            QueryResult<Entity1> queryResult = dataProvider.Query(taskParameters.DateFrom, taskParameters.DateTo);
            if (!queryResult.QuerySuccess)
            {
                // something went wrong performing the query - inform the task server and abort
                string errorMessage = string.Format("Error staging '{0}' object: {1}", dataProvider.DataType.Name, queryResult.Message);
                AbortSession(taskParameters.SessionId, errorMessage);
                return false;
            }

            foreach (var data in queryResult.Data)
            {
                if (StagingDisabled)
                {
                    Console.WriteLine("   - {0}", dataProvider.FriendlyDisplay(data));
                }
                else
                {
                    ActionResponse response = _stagingServer.Load(taskParameters.SessionId, data);
                    if (!response.Success)
                    {
                        // something went wrong staging the data object - inform the task server and abort
                        string errorMessage = string.Format("Error staging '{0}' object: {1}", dataProvider.DataType.Name, response.Message);
                        AbortSession(taskParameters.SessionId, errorMessage);
                        return false;
                    }
                }
            }

            return true;
        }

        private bool StageDataFromProvider(NewTaskRunResult taskParameters, IEntity2Provider dataProvider)
        {
            if (StagingDisabled)
            {
                Console.WriteLine();
                Console.WriteLine("Staging is disabled for datatype: {0}. Query results displayed below...", dataProvider.DataType.Name);
            }

            QueryResult<Entity2> queryResult = dataProvider.Query(taskParameters.DateFrom, taskParameters.DateTo);
            if (!queryResult.QuerySuccess)
            {
                // something went wrong performing the query - inform the task server and abort
                string errorMessage = string.Format("Error staging '{0}' object: {1}", dataProvider.DataType.Name, queryResult.Message);
                AbortSession(taskParameters.SessionId, errorMessage);
                return false;
            }

            foreach (var data in queryResult.Data)
            {
                if (StagingDisabled)
                {
                    Console.WriteLine("   - {0}", dataProvider.FriendlyDisplay(data));
                }
                else
                {
                    ActionResponse response = _stagingServer.Load(taskParameters.SessionId, data);
                    if (!response.Success)
                    {
                        // something went wrong staging the data object - inform the task server and abort
                        string errorMessage = string.Format("Error staging '{0}' object: {1}", dataProvider.DataType.Name, response.Message);
                        AbortSession(taskParameters.SessionId, errorMessage);
                        return false;
                    }
                }
            }

            return true;
        }

        private void VerboseOutput(string line)
        {
            if (Verbose)
            {
                Console.WriteLine(line);
            }
        }

        private string FormatDiagnosticsMessage(RobotDiagnostics diagnostics)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("======================================================================").AppendLine();
            sb.AppendFormat("Robot Diagnostics :").AppendLine();
            sb.AppendFormat("======================================================================").AppendLine();
            sb.AppendFormat("   Robot Id       : {0}", _robotId).AppendLine();
            sb.AppendFormat("   Server Name    : {0}", diagnostics.ServerName).AppendLine();
            sb.AppendFormat("   Server IP      : {0}", diagnostics.ServerIPAddress).AppendLine();
            sb.AppendFormat("   Server Path    : {0}", diagnostics.ServerPath).AppendLine();
            sb.AppendFormat("   Remote Health  : {0}", diagnostics.AllProvidersHealthy ? "All Ok" : "Fail").AppendLine();
            sb.AppendFormat("                  : {0} -> {1}", "Task Server", diagnostics.TaskServerHealth.Success ? "Ok" : "Fail").AppendLine();
            sb.AppendFormat("                  : {0} -> {1}", "Staging Server", diagnostics.StagingServerHealth.Success ? "Ok" : "Fail").AppendLine();
            sb.AppendFormat("                  : {0} -> {1}", _entity1Provider.DataType.Name, diagnostics.Entity1ProviderHealth.Success ? "Ok" : "Fail").AppendLine();
            sb.AppendFormat("                  : {0} -> {1}", _entity2Provider.DataType.Name, diagnostics.Entity1ProviderHealth.Success ? "Ok" : "Fail").AppendLine();
            sb.AppendFormat("======================================================================").AppendLine();
            return sb.ToString();
        }

        private void SendErrorEmail(string subject, string message)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            if (_robotDiagnostics != null)
            {
                sb.AppendLine();
                sb.AppendLine(FormatDiagnosticsMessage(_robotDiagnostics));
            }
            MailFunctions.SendErrorEmail("DataRetentionRobot " + ConfigOptions.RobotId + " : " + subject, sb.ToString());
        }
    }
}
