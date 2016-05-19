//-----------------------------------------------------------------------
// <copyright file="ICustomIdentityConfiguration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Configuration
{
    using System.Xml;

    /// <summary>
    /// Types that implement ICustomIdentityConfiguration can load custom configuration
    /// </summary>
    public interface ICustomIdentityConfiguration
    {
        /// <summary>
        /// Override LoadCustomConfiguration to provide custom handling of configuration elements
        /// </summary>
        /// <param name="nodeList">Xml Nodes which contain custom configuration</param>
        void LoadCustomConfiguration(XmlNodeList nodeList);
    }
}
