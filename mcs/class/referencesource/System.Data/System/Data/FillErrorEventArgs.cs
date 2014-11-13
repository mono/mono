//------------------------------------------------------------------------------
// <copyright file="FillErrorEventArgs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data { // MDAC 59437

    using System;
    using System.Data;

    public class FillErrorEventArgs : System.EventArgs {
        private bool continueFlag;
        private DataTable dataTable;
        private Exception errors;
        private object[] values;

        public FillErrorEventArgs(DataTable dataTable, object[] values) {
            this.dataTable = dataTable;
            this.values = values;
            if (null == this.values) {
                this.values = new object[0];
            }
        }

        public bool Continue {
            get {
                return this.continueFlag;
            }
            set {
                this.continueFlag = value;
            }
        }

        public DataTable DataTable {
            get {
                return this.dataTable;
            }
        }

        public Exception Errors {
            get {
                return this.errors;
            }
            set {
                this.errors = value;
            }
        }

        public object[] Values {
            get {
                object[] copy = new object[values.Length];
                for(int i = 0; i < values.Length; ++i) {
                    copy[i] = values[i]; // WebData 107464
                }
                return copy;
            }
        }
    }
}
