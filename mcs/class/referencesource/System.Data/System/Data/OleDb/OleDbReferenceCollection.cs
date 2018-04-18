//------------------------------------------------------------------------------
// <copyright file="OleDbReferenceCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb {

    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    sealed internal class OleDbReferenceCollection : DbReferenceCollection {
        internal const int Closing = 0;
        internal const int Canceling = -1;

        internal const int CommandTag = 1;
        internal const int DataReaderTag = 2;

        override public void Add(object value, int tag) {
            base.AddItem(value, tag);
        }

        override protected void NotifyItem(int message, int tag, object value) {
            bool canceling = (Canceling == message);
            if (CommandTag == tag) {
                ((OleDbCommand) value).CloseCommandFromConnection(canceling);
            }
            else if (DataReaderTag == tag) {
                ((OleDbDataReader) value).CloseReaderFromConnection(canceling);
            }
            else {
                Debug.Assert(false, "shouldn't be here");
            }
        }

        override public void Remove(object value) {
            base.RemoveItem(value);
        }

    }
}
