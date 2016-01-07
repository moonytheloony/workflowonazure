// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="AsyncResult.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Utilities
{
    #region

    using System;
    using System.Threading;

    #endregion

    /// <summary>
    /// Class AsyncResult.
    /// </summary>
    internal abstract class AsyncResult : IAsyncResult
    {
        #region Fields

        /// <summary>
        /// The asyncOperationCallback.
        /// </summary>
        private readonly AsyncCallback callback;

        /// <summary>
        /// The state.
        /// </summary>
        private readonly object state;

        /// <summary>
        /// The this lock.
        /// </summary>
        private readonly object thisLock;

        /// <summary>
        /// The completed synchronously.
        /// </summary>
        private bool completedSynchronously;

        /// <summary>
        /// The end called.
        /// </summary>
        private bool endCalled;

        /// <summary>
        /// The completeOperationException.
        /// </summary>
        private Exception exception;

        /// <summary>
        /// The is completed.
        /// </summary>
        private bool isCompleted;

        /// <summary>
        /// The manual reset event.
        /// </summary>
        private volatile ManualResetEvent manualResetEvent;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResult" /> class.
        /// </summary>
        /// <param name="callback">The asyncOperationCallback.</param>
        /// <param name="state">The state.</param>
        protected AsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            this.thisLock = new object();
        }

        #endregion

        #region Delegates

        /// <summary>
        /// Asynchronous complete.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>Pointer to function.</returns>
        protected delegate bool AsyncCompletion(IAsyncResult result);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the async state.
        /// </summary>
        /// <value>The state of the asynchronous.</value>
        public object AsyncState
        {
            get
            {
                return this.state;
            }
        }

        /// <summary>
        /// Gets the async wait handle.
        /// </summary>
        /// <value>The asynchronous wait handle.</value>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this.manualResetEvent != null)
                {
                    return this.manualResetEvent;
                }

                lock (this.ThisLock)
                {
                    if (this.manualResetEvent == null)
                    {
                        this.manualResetEvent = new ManualResetEvent(this.isCompleted);
                    }
                }

                return this.manualResetEvent;
            }
        }

        /// <summary>
        /// Gets a value indicating whether completed synchronously.
        /// </summary>
        /// <value><c>true</c> if [completed synchronously]; otherwise, <c>false</c>.</value>
        public bool CompletedSynchronously
        {
            get
            {
                return this.completedSynchronously;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is completed.
        /// </summary>
        /// <value><c>true</c> if this instance is completed; otherwise, <c>false</c>.</value>
        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the this lock.
        /// </summary>
        /// <value>The this lock.</value>
        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Ends the specified result.
        /// </summary>
        /// <typeparam name="TAsyncResult">The type of the asynchronous result.</typeparam>
        /// <param name="result">The result.</param>
        /// <returns>Asynchronous result</returns>
        /// <exception cref="System.ArgumentNullException">result</exception>
        /// <exception cref="System.ArgumentException">@Invalid AsyncResult;result</exception>
        /// <exception cref="System.InvalidOperationException">Async Result already ended</exception>
        /// <completeOperationException cref="System.ArgumentNullException">argument null result</completeOperationException>
        /// <completeOperationException cref="System.ArgumentException">Invalid AsyncResult result</completeOperationException>
        /// <completeOperationException cref="System.InvalidOperationException">AsyncResult already ended</completeOperationException>
        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : AsyncResult
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            var asyncResult = result as TAsyncResult;

            if (asyncResult == null)
            {
                throw new ArgumentException(@"Invalid AsyncResult", "result");
            }

            if (asyncResult.endCalled)
            {
                throw new InvalidOperationException("Async Result already ended");
            }

            asyncResult.endCalled = true;

            if (!asyncResult.isCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }

            if (asyncResult.manualResetEvent != null)
            {
                asyncResult.manualResetEvent.Close();
            }

            if (asyncResult.exception != null)
            {
                throw asyncResult.exception;
            }

            return asyncResult;
        }

        /// <summary>
        /// Completes the specified completed synchronously.
        /// </summary>
        /// <param name="isCompletedSynchronously">if set to <c>true</c> [completed synchronously].</param>
        /// <exception cref="System.InvalidProgramException">
        /// Async async Operation Callback threw an Exception
        /// </exception>
        /// <completeOperationException cref="System.InvalidProgramException">
        /// Async asyncOperationCallback threw an Exception
        /// </completeOperationException>
        protected void Complete(bool isCompletedSynchronously)
        {
            if (this.isCompleted)
            {
                throw new InvalidProgramException();
            }

            this.completedSynchronously = isCompletedSynchronously;

            if (isCompletedSynchronously)
            {
                this.isCompleted = true;
            }
            else
            {
                lock (this.ThisLock)
                {
                    this.isCompleted = true;
                    if (this.manualResetEvent != null)
                    {
                        this.manualResetEvent.Set();
                    }
                }
            }

            if (this.callback == null)
            {
                return;
            }

            try
            {
                this.callback(this);
            }
            catch (Exception e)
            {
                throw new InvalidProgramException("Async async Operation Callback threw an Exception", e);
            }
        }

        #endregion
    }
}