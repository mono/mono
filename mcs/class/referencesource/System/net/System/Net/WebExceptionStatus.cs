//------------------------------------------------------------------------------
// <copyright file="WebExceptionStatus.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Threading;

    /// <devdoc>
    ///    <para>
    ///       Specifies the status of a network request.
    ///    </para>
    /// </devdoc>
    public enum WebExceptionStatus {
        /// <devdoc>
        ///    <para>
        ///       No error was encountered.
        ///    </para>
        /// </devdoc>
        Success = 0,

        /// <devdoc>
        ///    <para>
        ///       The name resolver service could not resolve the host name.
        ///    </para>
        /// </devdoc>
        NameResolutionFailure = 1,

        /// <devdoc>
        ///    <para>
        ///       The remote service point could not be contacted at the transport level.
        ///    </para>
        /// </devdoc>
        ConnectFailure = 2,

        /// <devdoc>
        ///    <para>
        ///       A complete response was not received from the remote server.
        ///    </para>
        /// </devdoc>
        ReceiveFailure = 3,

        /// <devdoc>
        ///    <para>
        ///       A complete request could not be sent to the remote server.
        ///    </para>
        /// </devdoc>
        SendFailure = 4,

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        PipelineFailure = 5,

        /// <devdoc>
        ///    <para>
        ///       The request was cancelled.
        ///    </para>
        /// </devdoc>
        RequestCanceled = 6,

        /// <devdoc>
        ///    <para>
        ///       The response received from the server was complete but indicated a
        ///       protocol-level error. For example, an HTTP protocol error such as 401 Access
        ///       Denied would use this status.
        ///    </para>
        /// </devdoc>
        ProtocolError = 7,

        /// <devdoc>
        ///    <para>
        ///       The connection was prematurely closed.
        ///    </para>
        /// </devdoc>
        ConnectionClosed = 8,

        /// <devdoc>
        ///    <para>
        ///       A server certificate could not be validated.
        ///    </para>
        /// </devdoc>
        TrustFailure = 9,

        /// <devdoc>
        ///    <para>
        ///       An error occurred in a secure channel link.
        ///    </para>
        /// </devdoc>
        SecureChannelFailure = 10,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        ServerProtocolViolation = 11,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        KeepAliveFailure = 12,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Pending = 13,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Timeout = 14,

        /// <devdoc>
        ///    <para>
        ///       Similar to NameResolution Failure, but for proxy failures.
        ///    </para>
        /// </devdoc>
        ProxyNameResolutionFailure = 15,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        UnknownError = 16,

        /// <devdoc>
        ///    <para>
        ///       Sending the request to the server or receiving the response from it,
        ///       required handling a message that exceeded the specified limit.
        ///    </para>
        /// </devdoc>
        MessageLengthLimitExceeded = 17,

        //
        // A request could be served from Cache but was not found and effective CachePolicy=CacheOnly
        //
        CacheEntryNotFound = 18,

        //
        // A request is not suitable for caching and effective CachePolicy=CacheOnly
        //
        RequestProhibitedByCachePolicy = 19,

        //
        // The proxy script (or other proxy logic) declined to provide proxy info, effectively blocking the request.
        //
        RequestProhibitedByProxy = 20,

        // !! If new values are added, increase the size of the s_Mapping array below to the largest value + 1.
    }; // enum WebExceptionStatus

    // Mapping from enum value to error message.
    internal static class WebExceptionMapping
    {
        private static readonly string[] s_Mapping = new string[21];

        internal static string GetWebStatusString(WebExceptionStatus status)
        {
            int statusInt = (int) status;
            if (statusInt >= s_Mapping.Length || statusInt < 0)
            {
                throw new InternalException();
            }

            string message = Volatile.Read(ref s_Mapping[statusInt]);
            if (message == null)
            {
                message = "net_webstatus_" + status.ToString();
                Volatile.Write(ref s_Mapping[statusInt], message);
            }
            return message;
        }
    }
} // namespace System.Net
