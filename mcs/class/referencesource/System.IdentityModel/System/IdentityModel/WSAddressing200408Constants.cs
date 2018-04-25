//-----------------------------------------------------------------------
// <copyright file="WSAddressing200408Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines constants used in WS-Addressing standard schema.
    /// </summary>
    internal static class WSAddressing200408Constants
    {
#pragma warning disable 1591
        public const string Prefix = "wsa";
        public const string NamespaceUri = "http://schemas.xmlsoap.org/ws/2004/08/addressing";

        public static class Elements
        {
            public const string Action = "Action";
            public const string ReplyTo = "ReplyTo";
        }
#pragma warning restore 1591
    }
}
