// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="InstanceData.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Entities
{
    #region

    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Class InstanceData.
    /// </summary>
    public class InstanceData
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        /// <value>The entity tag.</value>
        public string EntityTag { get; set; }

        /// <summary>
        /// Gets or sets the instance key.
        /// </summary>
        /// <value>The instance key.</value>
        public string InstanceKey { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>The payload.</value>
        public IList<string> Payload { get; set; }

        /// <summary>
        /// Gets or sets the bookmark id.
        /// </summary>
        /// <value>The request identifier.</value>
        public string RequestId { get; set; }

        #endregion
    }
}