// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="CompletedAsyncResult.Generic.cs" company="">
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
    /// <typeparam name="T"></typeparam>
    internal class CompletedAsyncResult<T> : AsyncResult
    {
        #region Fields

        /// <summary>
        /// The data.
        /// </summary>
        private readonly T data;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedAsyncResult{T}" /> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public CompletedAsyncResult(T data, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.data = data;
            this.Complete(true);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The end.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The <see cref="T" />.</returns>
        public static T End(IAsyncResult result)
        {
            var completedResult = End<CompletedAsyncResult<T>>(result);
            return completedResult.data;
        }

        #endregion
    }
}