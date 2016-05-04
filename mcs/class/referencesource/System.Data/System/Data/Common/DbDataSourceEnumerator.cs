//------------------------------------------------------------------------------
// <copyright file="DbDataSourceEnumerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Threading;

    public abstract class DbDataSourceEnumerator { // V1.2.3300
    
        protected DbDataSourceEnumerator() { // V1.2.3300
        }


        abstract public DataTable GetDataSources(); // V1.2.3300

        /*
        virtual public IAsyncResult BeginGetDataSources(AsyncCallback callback, object asyncStateObject) { // V1.2.3300
            DbEnumSynchronousAsyncResult asyncResult = new DbEnumSynchronousAsyncResult(callback, asyncStateObject);
            try {
                asyncResult._dataTable = GetElements();
                asyncResult.SetCompletedSynchronously();
            }
            catch(DataAdapterException e) {
                ADP.TraceExceptionForCapture(e);
                asyncResult.ExceptionObject = e;
            }
            if (null == asyncResult._dataTable) {
                throw ADP.DataAdapter("no datatable");
            }
            if (null != callback) {
                callback(asyncResult);
            }
            return asyncResult;
        }

        virtual public DataTable EndGetDataSources(IAsyncResult asyncResult) { // V1.2.3300
            ADP.CheckArgumentNull(asyncResult, "asyncResult");
            DbEnumSynchronousAsyncResult ar = (asyncResult as DbEnumSynchronousAsyncResult);
            if (ar._endXxxCalled) {
                throw ADP.InvalidOperation("EndGetElements called twice");
            }
            ar._endXxxCalled = true;

            if (null != ar.ExceptionObject) {
                throw ar.ExceptionObject;
            }
            return ar._dataTable;
        }

        sealed private class DbEnumSynchronousAsyncResult : DbAsyncResult {
            internal bool      _endXxxCalled;
            internal DataTable _dataTable;

            internal DbEnumSynchronousAsyncResult(AsyncCallback callback, Object asyncStateObject) : base(null, callback, asyncStateObject) {
            }
        }
        */
    }
}
