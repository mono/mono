//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;
    using System.Collections;
    using System.Collections.Generic;

    [DataContract]
    class ExclusiveHandleList : HybridCollection<ExclusiveHandle>
    {
        public ExclusiveHandleList()
            : base() { }

        internal bool Contains(ExclusiveHandle handle)
        {
            if (this.SingleItem != null)
            {
                if (this.SingleItem.Equals(handle))
                {
                    return true;
                }
            }
            else if (this.MultipleItems != null)
            {
                for (int i = 0; i < this.MultipleItems.Count; i++)
                {
                    if (handle.Equals(this.MultipleItems[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}


