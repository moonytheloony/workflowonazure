using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace WorkflowHost.Workflow
{
    using WorkflowHost.Entities;

    public sealed class CopyBlobToContainer : CodeActivity
    {
        // Define an activity input argument of type string
        public InOutArgument<ChannelData> ChannelData { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
           
        }
    }
}
