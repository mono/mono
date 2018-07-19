/*++
Copyright (c) Microsoft Corporation

Module Name:

    AuthenticationException.cs

Abstract:
        The classes reperesent an exceptions resulted from
        authentication process.
        It is used by SSL and Negotiate protocol implementaions.

        FatalAuthentictionException usually means that the auth
        procees cannot (shoudl not) be retried over the same transport
        stream.


Author:
    Alexei Vopilov    12-Aug-2003

Revision History:
    12-Aug-2003 New design that has obsoleted Authenticator class
    12-Dec-2003 FxCop: added default and serialization constructors
--*/

namespace System.Security.Authentication {
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// <para>
    /// This exception can be thrown from Authenticate() method of Ssl and Negotiate classes.
    /// The authentication process can be retried with different parameters subject to
    /// remote party willingess of accepting that.
    /// </para>
    /// </summary>
    [Serializable]
    public class AuthenticationException: SystemException
    {
        public AuthenticationException() {}
        protected AuthenticationException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext) {}
        public AuthenticationException(string message): base(message) {}
        public AuthenticationException(string message, Exception innerException): base(message, innerException) {}
    }

    /// <summary>
    /// <para>
    /// This exception can be thrown from Authenticate() method of Ssl and Negotiate classes.
    /// The authentication is expected to fail prematurely if called using the same
    /// underlined stream.
    /// </para>
    /// </summary>
    [Serializable]
    public class InvalidCredentialException: AuthenticationException
    {
        public InvalidCredentialException() {}
        protected InvalidCredentialException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext) {}
        public InvalidCredentialException(string message): base(message) {}
        public InvalidCredentialException(string message, Exception innerException):base(message, innerException) {}
    }
}
