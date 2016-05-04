//------------------------------------------------------------------------------
// <copyright file="GatewayDefinition.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Xml;

    internal class GatewayDefinition : BrowserDefinition {
        internal GatewayDefinition(XmlNode node) : base(node) {
        }
    }
}
