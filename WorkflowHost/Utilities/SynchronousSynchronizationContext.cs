using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowHost.Utilities
{
    using System.Diagnostics.Contracts;
    using System.Threading;

    internal class SynchronousSynchronizationContext : SynchronizationContext
    {
        #region Public Methods and Operators

        /// <summary>
        /// The post.
        /// </summary>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        public override void Post(SendOrPostCallback callback, object state)
        {
            Contract.Requires<Exception>(null != callback, "callback");
            callback(state);
        }

        #endregion
    }
}
