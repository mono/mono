//------------------------------------------------------------------------------
// <copyright file="iwebproxy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Net {
    using System.Runtime.Serialization;

    /// <devdoc>
    ///    <para>
    ///       Holds the interface for implementation of the proxy interface.
    ///       Used to implement and control proxy use of WebRequests. 
    ///    </para>
    /// </devdoc>
    public interface IWebProxy  { 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        Uri GetProxy( Uri destination );
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        bool IsBypassed(Uri host);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        ICredentials Credentials { get; set; }
    }
}
