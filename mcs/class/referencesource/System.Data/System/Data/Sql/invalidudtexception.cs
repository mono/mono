//------------------------------------------------------------------------------
// <copyright file="InvalidUdtException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace Microsoft.SqlServer.Server {
    
    [Serializable]
    public sealed class InvalidUdtException : SystemException {
     
        internal InvalidUdtException() : base() {
            HResult = HResults.InvalidUdt;
        }

        internal InvalidUdtException(String message) : base(message) {
            HResult = HResults.InvalidUdt;
        }

        internal InvalidUdtException(String message, Exception innerException) : base(message, innerException) {
            HResult = HResults.InvalidUdt;
        }

        private InvalidUdtException(SerializationInfo si, StreamingContext sc) : base(si, sc) {
        }

        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags=System.Security.Permissions.SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo si, StreamingContext context) {
            base.GetObjectData(si, context);
        }

        internal static InvalidUdtException Create(Type udtType, string resourceReason) {
            string reason = Res.GetString(resourceReason);
            string message = Res.GetString(Res.SqlUdt_InvalidUdtMessage, udtType.FullName, reason);
            InvalidUdtException e =  new InvalidUdtException(message);
            ADP.TraceExceptionAsReturnValue(e);
            return e;
        }
    }
}
