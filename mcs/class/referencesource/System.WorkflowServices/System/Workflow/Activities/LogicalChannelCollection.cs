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
    internal sealed class LogicalChannelCollection : KeyedCollection<string, LogicalChannel>
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty LogicalChannelCollectionProperty =
            DependencyProperty.RegisterAttached("LogicalChannelCollection",
            typeof(LogicalChannelCollection),
            typeof(LogicalChannelCollection));

        public LogicalChannelCollection()
        {
        }

        public LogicalChannel GetItem(string key)
        {
            return this[key];
        }

        protected override void ClearItems()
        {
            base.ClearItems();
        }

        protected override string GetKeyForItem(LogicalChannel item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, LogicalChannel item)
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

        protected override void SetItem(int index, LogicalChannel item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }

            base.SetItem(index, item);
        }
    }
}
