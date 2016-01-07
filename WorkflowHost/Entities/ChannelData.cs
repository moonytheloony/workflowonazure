// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="ChannelData.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Entities
{
    /// <summary>
    /// Class ChannelData.
    /// </summary>
    public class ChannelData
    {
        #region Public Properties

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>The payload.</value>
        public HostQueueMessage Payload { get; internal set; }

        #endregion
    }
}