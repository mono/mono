//------------------------------------------------------------------------------
// <copyright file="OdbcEnvironmentHandle.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Runtime.Versioning;

namespace System.Data.Odbc {

    sealed internal class OdbcEnvironmentHandle : OdbcHandle {

        // SxS: this method uses SQLSetEnvAttr to setup ODBC environment handle settings. Environment handle is safe in SxS.
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        internal OdbcEnvironmentHandle() : base(ODBC32.SQL_HANDLE.ENV, null) {
            ODBC32.RetCode retcode;
            
            //Set the expected driver manager version
            //
            retcode = UnsafeNativeMethods.SQLSetEnvAttr(
                this,
                ODBC32.SQL_ATTR.ODBC_VERSION,
                ODBC32.SQL_OV_ODBC3,
                ODBC32.SQL_IS.INTEGER);
            // ignore retcode

            //Turn on connection pooling
            //Note: the env handle controls pooling.  Only those connections created under that
            //handle are pooled.  So we have to keep it alive and not create a new environment
            //for   every connection.
            //
            retcode = UnsafeNativeMethods.SQLSetEnvAttr(
                this,
                ODBC32.SQL_ATTR.CONNECTION_POOLING,
                ODBC32.SQL_CP_ONE_PER_HENV,
                ODBC32.SQL_IS.INTEGER);

            switch(retcode) {
            case ODBC32.RetCode.SUCCESS:
            case ODBC32.RetCode.SUCCESS_WITH_INFO:
                break;
            default:
                Dispose();
                throw ODBC.CantEnableConnectionpooling(retcode);
            }
        }
    }
}

