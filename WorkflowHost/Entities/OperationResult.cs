using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowHost.Entities
{
    public class OperationResult
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResult"/> class.
        /// </summary>
        /// <param name="httpStatusCode">
        /// The HTTP status code.
        /// </param>
        /// <param name="isSuccess">
        /// if the operation is successful.
        /// </param>
        public OperationResult(int httpStatusCode, bool isSuccess)
        {
            this.HttpStatusCode = httpStatusCode;
            this.IsSuccess = isSuccess;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the HTTP status code returned for this operation.
        /// </summary>
        /// <value>
        ///     The HTTP status code.
        /// </value>
        public int HttpStatusCode { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        #endregion
    }
}
