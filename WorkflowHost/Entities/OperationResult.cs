// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="OperationResult.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Entities
{
    /// <summary>
    /// Class OperationResult.
    /// </summary>
    public class OperationResult
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResult" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="isSuccess">if the operation is successful.</param>
        public OperationResult(int httpStatusCode, bool isSuccess)
        {
            this.HttpStatusCode = httpStatusCode;
            this.IsSuccess = isSuccess;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the HTTP status code returned for this operation.
        /// </summary>
        /// <value>The HTTP status code.</value>
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this operation was successful.
        /// </summary>
        /// <value><c>true</c> if this instance is success; otherwise, <c>false</c>.</value>
        public bool IsSuccess { get; set; }

        #endregion
    }
}