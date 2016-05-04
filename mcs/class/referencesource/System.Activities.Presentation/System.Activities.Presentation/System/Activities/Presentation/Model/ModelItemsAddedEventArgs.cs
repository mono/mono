//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal class ModelItemsAddedEventArgs : EventArgs
    {
        public ModelItemsAddedEventArgs(IEnumerable<ModelItem> modelItemsAdded)
        {
            Fx.Assert(modelItemsAdded != null, "modelItemsAdded should not be null");
            this.ModelItemsAdded = modelItemsAdded;
        }

        public IEnumerable<ModelItem> ModelItemsAdded { get; private set; }
    }
}
