using System;


namespace WorkflowHost.Utilities
{

    internal class CompletedAsyncResult : AsyncResult
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedAsyncResult"/> class.
        /// </summary>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        public CompletedAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.Complete(true);
        }

        #endregion
    }
}
