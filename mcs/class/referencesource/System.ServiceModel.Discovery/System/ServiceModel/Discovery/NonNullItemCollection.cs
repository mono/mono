//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.ObjectModel;

    // The NonNullItemCollection overrides the InsertItem and SetItem
    // methods to check if any null items are inserted. All publicly 
    // exposed collections and collections used for serialization   
    // either use this or a collection which inherits this collection.
    class NonNullItemCollection<T> : Collection<T>
    {
        protected override void InsertItem(int index, T item)
        {
            if (item == null)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            base.InsertItem(index, item);

        }

        protected override void SetItem(int index, T item)
        {
            if (item == null)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            base.SetItem(index, item);
        }
    }
}
