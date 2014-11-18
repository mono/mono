//-----------------------------------------------------------------------
// <copyright file="WSAddressing10Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines constants used in WS-Addressing 1.0 standard schema.
    /// </summary>
    internal static class WSAddressing10Constants
    {
#pragma warning disable 1591
        public const string Prefix = "wsa";
        public const string NamespaceUri = "http://www.w3.org/2005/08/addressing";

        public static class Elements
        {
            public const string Action = "Action";
            public const string Address = "Address";
            public const string ReplyTo = "ReplyTo";
            public const string EndpointReference = "EndpointReference";
        }
#pragma warning restore 1591
    }
}
