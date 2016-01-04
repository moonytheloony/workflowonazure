using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowHost.Entities
{
    public class HostQueueMessage
    {
        /// <summary>
        ///     Gets or sets the activity name.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is halt requested.
        /// </summary>
        public bool IsHaltRequested { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is bookmark.
        /// </summary>
        public bool IsBookmark { get; set; }



        /// <summary>
        ///     Gets or sets the operation name.
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        ///     Gets or sets the payload.
        /// </summary>
        public List<string> PersistentPayload { get; set; }

        /// <summary>
        ///     Gets or sets the request identifier.
        /// </summary>
        public string RequestIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        public Guid WorkflowIdentifier { get; set; }
    }
}
