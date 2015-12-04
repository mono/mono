//------------------------------------------------------------------------------
// <copyright file="OleDbErrorCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb {

    using System;
    using System.ComponentModel;
    using System.Collections;
    using System.Data.Common;

    [Serializable, ListBindable(false)]
    public sealed class OleDbErrorCollection : System.Collections.ICollection {
        readonly private ArrayList items; // WebData 106655

        internal OleDbErrorCollection(UnsafeNativeMethods.IErrorInfo errorInfo) {
            ArrayList items = new ArrayList();

            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OS> IErrorRecords\n");
            UnsafeNativeMethods.IErrorRecords errorRecords = (errorInfo as UnsafeNativeMethods.IErrorRecords);
            if (null != errorRecords) {

                int recordCount = errorRecords.GetRecordCount();
                Bid.Trace("<oledb.IErrorRecords.GetRecordCount|API|OS|RET> RecordCount=%d\n", recordCount);

                for (int i = 0; i < recordCount; ++i) {
                    OleDbError error = new OleDbError(errorRecords, i);
                    items.Add(error);
                }
            }
            this.items = items;
        }

        bool System.Collections.ICollection.IsSynchronized {
            get { return false;}
        }

        object System.Collections.ICollection.SyncRoot {
            get { return this;}
        }

        public int Count {
            get {
                ArrayList items = this.items;
                return ((null != items) ? items.Count : 0);
            }
        }

        public OleDbError this[int index] {
            get {
                return (this.items[index] as OleDbError);
            }
        }

        internal void AddRange(ICollection c) {
            items.AddRange(c);
        }

        public void CopyTo(Array array, int index) {
            this.items.CopyTo(array, index);
        }

        public void CopyTo (OleDbError[] array, int index) {
            this.items.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator() {
            return this.items.GetEnumerator();
        }
    }
}
