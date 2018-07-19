//-----------------------------------------------------------------------
// <copyright file="WSTrust14Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// Defines constants for WS-Trust Version 1.4
    /// </summary>
    internal static class WSTrust14Constants
    {
#pragma warning disable 1591
        public const string NamespaceURI = "http://docs.oasis-open.org/ws-sx/ws-trust/200802";
        public const string Prefix = "tr";

        public static class ElementNames
        {
            public const string ActAs = "ActAs";
        }
#pragma warning restore 1591
    }
}
