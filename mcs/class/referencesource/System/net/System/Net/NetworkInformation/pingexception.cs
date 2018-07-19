//------------------------------------------------------------------------------
// <copyright file="WebException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Net.NetworkInformation {
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PingException : InvalidOperationException {

        internal PingException() { }

        protected PingException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext) {}

        public PingException(string message) : base(message) {
        }

        public PingException(string message, Exception innerException) :
            base(message, innerException) {

        }
    }; 
} // namespace System.Net

