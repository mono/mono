//------------------------------------------------------------------------------
// <copyright file="DummyDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;


    /// <devdoc>
    /// </devdoc>
    internal sealed class DummyDataSource : ICollection {

        private int dataItemCount;

        internal DummyDataSource(int dataItemCount) {
            this.dataItemCount = dataItemCount;
        }

        public int Count {
            get {
                return dataItemCount;
            }
        }

        public bool IsSynchronized {
            get {
                return false;
            }
        }

        public Object SyncRoot {
            get {
                return this;
            }
        }

        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }

        public IEnumerator GetEnumerator() {
            return new DummyDataSourceEnumerator(dataItemCount);
        }


        private class DummyDataSourceEnumerator : IEnumerator {

            private int count;
            private int index;

            public DummyDataSourceEnumerator(int count) {
                this.count = count;
                this.index = -1;
            }

            public object Current {
                get {
                    return null;
                }
            }

            public bool MoveNext() {
                index++;
                return index < count;
            }

            public void Reset() {
                this.index = -1;
            }
        }
    }
}

