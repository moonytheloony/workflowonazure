// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-06-2016
// ***********************************************************************
// <copyright file="HostQueueMessage.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Entities
{
    #region

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Class HostQueueMessage.
    /// </summary>
    public class HostQueueMessage
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether is bookmark.
        /// </summary>
        /// <value><c>true</c> if this instance is bookmark; otherwise, <c>false</c>.</value>
        public bool IsBookmark { get; set; }

        /// <summary>
        /// Gets or sets the item list.
        /// </summary>
        /// <value>The item list.</value>
        public List<string> ItemList { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>The persistent payload.</value>
        public Dictionary<string, string> PersistentPayload { get; set; }

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>The workflow identifier.</value>
        public Guid WorkflowIdentifier { get; set; }

        #endregion
    }
}