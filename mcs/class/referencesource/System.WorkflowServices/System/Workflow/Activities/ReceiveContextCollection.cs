//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime;

    [Serializable]
    internal sealed class ReceiveContextCollection : KeyedCollection<string, ReceiveContext>
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty ReceiveContextCollectionProperty =
            DependencyProperty.RegisterAttached("ReceiveContextCollection",
            typeof(ReceiveContextCollection),
            typeof(ReceiveContextCollection));

        public ReceiveContextCollection()
        {
        }

        public ReceiveContext GetItem(string key)
        {
            return this[key];
        }

        protected override void ClearItems()
        {
            base.ClearItems();
        }

        protected override string GetKeyForItem(ReceiveContext item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, ReceiveContext item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, ReceiveContext item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }

            base.SetItem(index, item);
        }
    }
}
