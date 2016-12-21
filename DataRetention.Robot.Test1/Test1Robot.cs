using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Start()
        {
            // Test connectivity of all providers and staging server
            RobotDiagnostics robotDiagnostics = PerformAllHealthTests();

            // Connect to task server and :
            //   - report systems connectivity
            //   - ask server if a new run is required (along with parameters for run)
            // (on error, send error notification and abort)
            ActionResponse actionResponse = _taskServer.ReportRobotHealth(robotDiagnostics);
            if (!actionResponse.Success)
            {
                // We failed connecting to the task server!!!!!  Need to fall back to manually alerting someone...
                throw new NotImplementedException("This functionality is not yet implemented.  We need to alert developers/admin of a problem (via email?)");
            }

            ActionResponse<NewTaskRunResult> requestNewTaskRunResult = _taskServer.RequestNewTaskRun();
            if (!requestNewTaskRunResult.Success)
            {
                // We failed connecting to the task server!!!!!  Need to fall back to manually alerting someone...
                throw new NotImplementedException("This functionality is not yet implemented.  We need to alert developers/admin of a problem (via email?)");
            }

            if (!requestNewTaskRunResult.Response.RunRequired)
            {
                // The task server has told us we don't need to run.  Nothing left to do.
                return;
            }

            Run(requestNewTaskRunResult.Response);
        }

        private RobotDiagnostics PerformAllHealthTests()
        {
            var results = new RobotDiagnostics();
            results.StagingServerHealth = _stagingServer.TestHealth();
            if (_entity1Provider != null)
                results.Entity1ProviderHealth = _entity1Provider.TestHealth();
            if (_entity2Provider != null)
                results.Entity2ProviderHealth = _entity2Provider.TestHealth();
            return results;
        }

        private void Run(NewTaskRunResult taskParameters)
        {
            // tell the task server that we're starting the run
            ActionResponse actionResponse = _taskServer.Starting(taskParameters.SessionId);
            if (!actionResponse.Success)
            {
                // The call to the task server failed!!!!!  Need to fall back to manually alerting someone...
                throw new NotImplementedException("This functionality is not yet implemented.  We need to alert developers/admin of a problem (via email?)");
            }

            // Iterate through all data types we have (in appropriate order) and
            if (!StageDataFromProvider(taskParameters, _entity1Provider))
            {
                return;
            }

            if (!StageDataFromProvider(taskParameters, _entity2Provider))
            {
                return;
            }

            // When done, commit the staging server data set
            actionResponse = _stagingServer.Commit(taskParameters.SessionId);
            if (!actionResponse.Success)
            {
                // Committing the data on the staging server failed!!!!!  Need to fall back to manually alerting someone...
                AbortSession(taskParameters.SessionId, "Error committing staged data: " + actionResponse.Message);
                return;
            }

            actionResponse = _taskServer.Completed(taskParameters.SessionId);
            if (!actionResponse.Success)
            {
                // Completing the session on the task server failed!!!!!  Need to fall back to manually alerting someone...
                throw new NotImplementedException("This functionality is not yet implemented.  We need to alert developers/admin of a problem (via email?)");
                return;
            }
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
                // Even the call to tell the task server about the error failed!!!!!  Need to fall back to manually alerting someone...
                throw new NotImplementedException("This functionality is not yet implemented.  We need to alert developers/admin of a problem (via email?)");
            }

            actionResponse = _stagingServer.Abort(sessionId);
            if (!actionResponse.Success)
            {
                // The call to abort the session on the staging server failed!!!!!  Need to fall back to manually alerting someone...
                throw new NotImplementedException("This functionality is not yet implemented.  We need to alert developers/admin of a problem (via email?)");
            }
        }

        private bool StageDataFromProvider<T>(NewTaskRunResult taskParameters, IDataProvider<T> dataProvider)
        {
            QueryResult<Entity1> queryResult = _entity1Provider.Query(taskParameters.DateFrom, taskParameters.DateTo);
            if (!queryResult.QuerySuccess)
            {
                // something went wrong performing the query - inform the task server and abort
                string errorMessage = string.Format("Error staging '{0}' object: {1}", dataProvider.DataType.Name, queryResult.Message);
                AbortSession(taskParameters.SessionId, errorMessage);
                return false;
            }

            foreach (var data in queryResult.Data)
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

            return true;
        }
    }
}
