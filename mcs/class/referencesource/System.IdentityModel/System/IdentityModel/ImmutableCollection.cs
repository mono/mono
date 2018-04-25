//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    //
    // A collection that can be made immutable by calling the
    // MakeReadOnly method. Once the collection is made read-only
    // Add, Remove and Clear methods will throw an exception 
    // failing to add a item to the collection.
    //
    internal sealed class ImmutableCollection<T> : Collection<T>, IList<T>, IList
    {
        bool isReadOnly = false;

        public void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        protected override void ClearItems()
        {
            if (this.isReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            if (this.isReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (this.isReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (this.isReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

            base.SetItem(index, item);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        bool IList.IsReadOnly
        {
            get { return this.isReadOnly; }
        }
    }
}
