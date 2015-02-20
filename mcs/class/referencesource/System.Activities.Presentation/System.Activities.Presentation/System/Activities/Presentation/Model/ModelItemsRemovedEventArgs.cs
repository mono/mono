//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Collections.Generic;
    using System.Runtime;

    internal class ModelItemsRemovedEventArgs : EventArgs
    {
        public ModelItemsRemovedEventArgs(IEnumerable<ModelItem> modelItemsRemoved)
        {
            Fx.Assert(modelItemsRemoved != null, "modelItemsRemoved should not be null");
            this.ModelItemsRemoved = modelItemsRemoved;
        }

        public IEnumerable<ModelItem> ModelItemsRemoved { get; private set; }
    }
}
