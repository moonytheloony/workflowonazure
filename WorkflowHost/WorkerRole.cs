namespace WorkflowHost
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using Microsoft.Azure;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;

    using WorkflowHost.DataStorage;
    using WorkflowHost.Entities;
    using WorkflowHost.Utilities;

    using InstanceData = WorkflowHost.Entities.InstanceData;

    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "ProcessingQueue";

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        private ManualResetEvent resetEvent;

        private AzureTableStorageRepository<InstanceData> repository;

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");
            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            Client.OnMessage(receivedMessage =>
                {
                    try
                    {
                        this.resetEvent = new ManualResetEvent(false);
                        // Process the message
                        Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());

                        // Name of workflow to trigger.
                        var workflowToTrigger = receivedMessage.Properties["workflowName"];
                        // Prepare input message for Workflow.
                        var channelData = new ChannelData() { Payload = receivedMessage.GetBody<HostQueueMessage>() };
                        // You can save the workflow xaml externally (usually database). See this.
                        var workflowXaml = File.ReadAllText($"{workflowToTrigger}.txt");
                        if (!string.IsNullOrWhiteSpace(workflowXaml))
                        {
                            //// 5. Compose WF Application.
                            var workflowApplication =
                                new WorkflowApplication(
                                    Routines.CreateWorkflowActivityFromXaml(workflowXaml, this.GetType().Assembly),
                                    new Dictionary<string, object> { { "ChannelData", channelData } });

                            //// 6. Extract Request Identifier to set it as identifier of workflow.
                            this.SetupWorkflowEnvironment(workflowApplication, channelData.Payload.WorkflowIdentifier);

                            //// 7. Test whether this is a resumed bookmark or a fresh message.
                            if (channelData.Payload.IsBookmark)
                            {
                                //// 8.1. Make sure there is data for this request identifier in storage to avoid errors due to transient errors.
                                if (null
                                    != this.repository.GetById(
                                        "WorkflowInstanceStoreData",
                                        channelData.Payload.WorkflowIdentifier.ToString()))
                                {
                                    //// Prepare a new workflow instance as we need to resume bookmark.
                                    var bookmarkedWorkflowApplication =
                                        new WorkflowApplication(
                                            Routines.CreateWorkflowActivityFromXaml(workflowXaml, this.GetType().Assembly));
                                    this.SetupWorkflowEnvironment(
                                        bookmarkedWorkflowApplication, channelData.Payload.WorkflowIdentifier);

                                    //// 9. Resume bookmark and supply input as is from channel data.
                                    bookmarkedWorkflowApplication.Load(channelData.Payload.WorkflowIdentifier);

                                    //// 9.1. If workflow got successfully completed, remove the host message.
                                    if (BookmarkResumptionResult.Success
                                        == bookmarkedWorkflowApplication.ResumeBookmark(
                                            bookmarkedWorkflowApplication.GetBookmarks().Single().BookmarkName,
                                            channelData,
                                            TimeSpan.FromDays(7)))
                                    {
                                        Trace.Write(
                                            Routines.FormatStringInvariantCulture("Bookmark successfully resumed."));
                                        this.resetEvent.WaitOne();
                                       Client.Complete(receivedMessage.LockToken);
                                        return;
                                    }

                                   }
                                else
                                {
                                    //// This was a transient error.
                                    Trace.Write(Routines.FormatStringInvariantCulture("Error"));
                                }
                            }

                            //// 10. Run workflow in case of normal execution.
                            workflowApplication.Run(TimeSpan.FromDays(7));
                            Trace.Write(Routines.FormatStringInvariantCulture("Workflow for request id has started execution"));
                            this.resetEvent.WaitOne();
                        }
                    }
                    catch
                    {
                        // Handle any message processing specific exceptions here
                    }
                });

            CompletedEvent.WaitOne();
        }

        private void SetupWorkflowEnvironment(WorkflowApplication workflowApplication, Guid workflowId)
        {
            //// Setup workflow execution environment.
            //// 1. Make the workflow synchronous
            workflowApplication.SynchronizationContext = new SynchronousSynchronizationContext();

            //// 2. Initialize instance store with instance identifier.
            this.repository = new AzureTableStorageRepository<InstanceData>("instanceStore", CloudConfigurationManager.GetSetting("WorkflowStorage"));
            var instanceStore = new UnstructuredStorageInstanceStore(repository,
                workflowId,
                this.AddBookmarkMessage);

            //// 3. Assign this instance store to WFA
            workflowApplication.InstanceStore = instanceStore;

            //// 4. Handle persistable idle to remove application from memory. 
            //// Also, at this point we need to add message to host queue to add message signaling that bookmark has been added.
            workflowApplication.PersistableIdle = persistableIdleEventArgument =>
            {
                //// Check whether the application is unloading because of bookmarks.
                if (persistableIdleEventArgument.Bookmarks.Any())
                {
                    Trace.Write(
                        Routines.FormatStringInvariantCulture(
                            "Application Instance {0} is going to save state for bookmark {1}",
                            persistableIdleEventArgument.InstanceId,
                            persistableIdleEventArgument.Bookmarks.Last().BookmarkName));
                }

                return PersistableIdleAction.Unload;
            };

            //// 5. Log when a WF completes.
            workflowApplication.Completed = applicationCompletedEventArgument =>
            {
                Trace.Write(
                    Routines.FormatStringInvariantCulture(
                        "Workflow instance {0} has completed with state {1}",
                        applicationCompletedEventArgument.InstanceId,
                        applicationCompletedEventArgument.CompletionState));
            };

            //// 6. Log when WF is unloaded from memory.
            workflowApplication.Unloaded = applicationUnloadedEventArgs =>
            {
                Trace.Write(
                    Routines.FormatStringInvariantCulture(
                        "Workflow instance {0} has been unloaded from memory",
                        applicationUnloadedEventArgs.InstanceId));
                this.resetEvent.Set();
            };

            //// 7. If workflow throws an unhandled exception, don't consume the message so that it can be retried.
            workflowApplication.OnUnhandledException = args =>
            {
                Trace.Write(
                    "Workflow encountered an unhandled exception",
                    args.UnhandledException.ToString());
                this.resetEvent.Set();
                return UnhandledExceptionAction.Abort;
            };
        }

        private void AddBookmarkMessage(Guid workflowId)
        {
            var hostQueueMessage = new HostQueueMessage
            {
                IsBookmark = true,
                WorkflowIdentifier = workflowId
            };
            Client.Send(new BrokeredMessage(hostQueueMessage));
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            // Initialize the connection to Service Bus Queue
            Client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            RoleEnvironment.Changing += (sender, args) =>
            {
                if (!args.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
                    return;
                args.Cancel = true;
            };
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            Client.Close();
            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
