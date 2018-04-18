//------------------------------------------------------------------------------
// <copyright file="SqlErrorCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [Serializable, ListBindable(false)]
    public sealed class SqlErrorCollection : ICollection {

        private ArrayList errors = new ArrayList();

        internal SqlErrorCollection() {
        }

        public void CopyTo (Array array, int index) {
            this.errors.CopyTo(array, index);
        }

        public void CopyTo (SqlError[] array, int index) {
            this.errors.CopyTo(array, index);
        }

        public int Count {
            get { return this.errors.Count;}
        }

        object System.Collections.ICollection.SyncRoot { // MDAC 68481
            get { return this;}
        }

        bool System.Collections.ICollection.IsSynchronized { // MDAC 68481
            get { return false;}
        }

        public SqlError this[int index] {
            get {
                return (SqlError) this.errors[index];
            }
        }

        public IEnumerator GetEnumerator() {
            return errors.GetEnumerator();
        }

        internal void Add(SqlError error) {
            this.errors.Add(error);
        }
    }
}
