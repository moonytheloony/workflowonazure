// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="CompletedAsyncResult.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Utilities
{
    #region

    using System;

    #endregion

    /// <summary>
    /// Class CompletedAsyncResult.
    /// </summary>
    internal class CompletedAsyncResult : AsyncResult
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedAsyncResult" /> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public CompletedAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.Complete(true);
        }

        #endregion
    }
}