//------------------------------------------------------------------------------
// <copyright file="OperationAbortedException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    using System;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class OperationAbortedException : SystemException {
        private OperationAbortedException(string message, Exception innerException) : base(message, innerException) {
            HResult = HResults.OperationAborted;
        }

        private OperationAbortedException(SerializationInfo si, StreamingContext sc) : base(si, sc) {
        }

        static internal OperationAbortedException Aborted(Exception inner) {
            OperationAbortedException e;
            if (inner == null) {
                e = new OperationAbortedException(Res.GetString(Res.ADP_OperationAborted), null);
            }
            else {
                e = new OperationAbortedException(Res.GetString(Res.ADP_OperationAbortedExceptionMessage), inner);
            }
            ADP.TraceExceptionAsReturnValue(e);
            return e;
        }
    }
}
