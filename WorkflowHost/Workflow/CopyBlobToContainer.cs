// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="CopyBlobToContainer.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Workflow
{
    #region

    using System.Activities;

    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Storage;

    using WorkflowHost.Entities;

    #endregion

    /// <summary>
    /// Class CopyBlobToContainer. This class cannot be inherited.
    /// </summary>
    public sealed class CopyBlobToContainer : CodeActivity
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the channel data.
        /// </summary>
        /// <value>The channel data.</value>
        public InOutArgument<ChannelData> ChannelData { get; set; }

        // Define an activity input argument of type string
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public InArgument<string> Data { get; set; }

        #endregion

        #region Methods

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Execute(CodeActivityContext context)
        {
            var data = context.GetValue(this.Data);
            var channelData = context.GetValue(this.ChannelData);
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("WorkflowStorage"));
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var sourceContainer =
                cloudBlobClient.GetContainerReference(channelData.Payload.PersistentPayload["container"]);
            var targetContainer = cloudBlobClient.GetContainerReference("documentcontainer");
            targetContainer.CreateIfNotExists();
            var sourceBlob = sourceContainer.GetBlockBlobReference(data);
            var targetBlob = targetContainer.GetBlockBlobReference(data);
            targetBlob.StartCopyFromBlob(sourceBlob);
        }

        #endregion
    }
}