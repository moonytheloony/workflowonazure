// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-06-2016
// ***********************************************************************
// <copyright file="GetAllBlobsFromContainer.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Workflow
{
    #region

    using System.Activities;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    using WorkflowHost.Entities;

    #endregion

    /// <summary>
    /// Class GetAllBlobsFromContainer. This class cannot be inherited.
    /// </summary>
    public sealed class GetAllBlobsFromContainer : NativeActivity
    {
        #region Public Properties

        // Define an activity input argument of type string
        /// <summary>
        /// Gets or sets the channel data.
        /// </summary>
        /// <value>The channel data.</value>
        public InOutArgument<ChannelData> ChannelData { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The can induce idle
        /// </summary>
        protected override bool CanInduceIdle => true;

        #endregion

        #region Methods

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(NativeActivityContext context)
        {
            var data = context.GetValue(this.ChannelData);
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("WorkflowStorage"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(data.Payload.PersistentPayload["container"]);
            container.CreateIfNotExists();
            data.Payload.ItemList = new List<string>();
            foreach (var blob in container.ListBlobs().OfType<CloudBlockBlob>())
            {
                data.Payload.ItemList.Add(blob.Name);
            }

            context.CreateBookmark($"{this.DisplayName}-Bookmark", this.OnResumeBookmark);
        }

        /// <summary>
        /// Called when [resume bookmark].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="bookmark">The bookmark.</param>
        /// <param name="value">The value.</param>
        private void OnResumeBookmark(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.SetValue(this.ChannelData, value as ChannelData);
        }

        #endregion
    }
}