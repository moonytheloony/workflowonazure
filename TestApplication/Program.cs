// ***********************************************************************
// Assembly         : TestApplication
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="Program.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace TestApplication
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    using WorkflowHost.Entities;

    #endregion

    /// <summary>
    /// Class Program.
    /// </summary>
    internal class Program
    {
        #region Constants

        /// <summary>
        /// The queue name
        /// </summary>
        private const string QueueName = "workflowmessages";

        #endregion

        #region Methods

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            var client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            var hostQueueMessage = new HostQueueMessage
                                       {
                                           IsBookmark = false,
                                           WorkflowIdentifier = Guid.NewGuid(),
                                           PersistentPayload =
                                               new Dictionary<string, string>
                                                   {
                                                       ["container"] =
                                                           "rawfiles"
                                                   }
                                       };
            var message = new BrokeredMessage(hostQueueMessage);
            message.Properties.Add("workflowName", "CopyDocsToContainer");
            client.Send(message);
        }

        #endregion
    }
}