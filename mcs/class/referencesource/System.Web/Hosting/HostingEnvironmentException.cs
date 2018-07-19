//------------------------------------------------------------------------------
// <copyright file="HostingEnvironmentException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    
    [Serializable]
    internal class HostingEnvironmentException : Exception {

        private String _details;

        protected HostingEnvironmentException(SerializationInfo info, StreamingContext context) : base(info, context) {
           _details = info.GetString("_details");
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("_details", _details);
        }

        internal HostingEnvironmentException(String message, String details) : base(message) {
            _details = details;
        }

        internal String Details {
            get { return (_details != null) ? _details : String.Empty; }
        }
    }
}
