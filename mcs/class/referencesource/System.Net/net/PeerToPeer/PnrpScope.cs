//------------------------------------------------------------------------------
// <copyright file="Logging.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <remarks>
    /// PnrpScope represents the scope of the cloud
    /// </remarks>
    public enum PnrpScope
    {
        /// <summary>
        /// Represents All clouds
        /// </summary>
        All = 0,

        /// <summary>
        /// Represents Global cloud
        /// </summary>
        Global = 1,

        /// <summary>
        /// Represents site local cloud
        /// </summary>
        SiteLocal = 2,

        /// <summary>
        /// Represents Link Local cloud
        /// </summary>
        LinkLocal = 3
    }
}
