using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowHost.Entities
{
    public class InstanceData
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the entity tag.
        /// </summary>
        public string EntityTag { get; set; }

        /// <summary>
        ///     Gets or sets the instance key.
        /// </summary>
        public string InstanceKey { get; set; }

        /// <summary>
        ///     Gets or sets the payload.
        /// </summary>
        public IList<string> Payload { get; set; }

        /// <summary>
        ///     Gets or sets the bookmark id.
        /// </summary>
        public string RequestId { get; set; }

        #endregion
    }
}
