//------------------------------------------------------------------------------
// <copyright file="OdbcReferenceCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Diagnostics;

namespace System.Data.Odbc {
    sealed internal class OdbcReferenceCollection : DbReferenceCollection {
        internal const int Closing = 0;
        internal const int Recover = 1;

        internal const int CommandTag = 1;

        override public void Add(object value, int tag) {
            base.AddItem(value, tag);
        }

        override protected void NotifyItem(int message, int tag, object value) {
            switch (message) {
            case Recover:
                if (CommandTag == tag) {
                    ((OdbcCommand) value).RecoverFromConnection();
                }
                else {
                    Debug.Assert(false, "shouldn't be here");
                }
                break;
            case Closing:
                if (CommandTag == tag) {
                    ((OdbcCommand) value).CloseFromConnection();
                }
                else {
                    Debug.Assert(false, "shouldn't be here");
                }
                break;
            default:
                Debug.Assert(false, "shouldn't be here");
                break;
            }
        }

        override public void Remove(object value) {
            base.RemoveItem(value);
        }

    }
}

