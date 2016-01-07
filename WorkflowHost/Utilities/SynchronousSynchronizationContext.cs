// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="SynchronousSynchronizationContext.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Utilities
{
    #region

    using System.Threading;

    #endregion

    /// <summary>
    /// Class SynchronousSynchronizationContext.
    /// </summary>
    internal class SynchronousSynchronizationContext : SynchronizationContext
    {
        #region Public Methods and Operators

        /// <summary>
        /// The post.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public override void Post(SendOrPostCallback callback, object state)
        {
            callback(state);
        }

        #endregion
    }
}