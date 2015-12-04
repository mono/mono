//------------------------------------------------------------------------------
// <copyright file="DbException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    [Serializable]
    public abstract class DbException : System.Runtime.InteropServices.ExternalException {

        protected DbException() : base() {
        }

        protected DbException(System.String message) : base(message) {
        }

        protected DbException(System.String message, System.Exception innerException) : base(message, innerException) {
        }

        protected DbException(System.String message, System.Int32 errorCode) : base(message, errorCode) {
        }

        protected DbException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
    }
}
