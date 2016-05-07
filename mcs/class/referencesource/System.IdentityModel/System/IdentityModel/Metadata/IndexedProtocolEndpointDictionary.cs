//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.Collections.Generic;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// A sorted list of <see cref="IndexedProtocolEndpoint"/>.
    /// </summary>
    public class IndexedProtocolEndpointDictionary : SortedList<int, IndexedProtocolEndpoint>
    {
        /// <summary>
        /// Gets the default <see cref="IndexedProtocolEndpoint"/>.
        /// </summary>
        public IndexedProtocolEndpoint Default
        {
            get
            {
                IndexedProtocolEndpoint impliedDefault = null;
                foreach (KeyValuePair<int, IndexedProtocolEndpoint> kvp in this)
                {
                    if (kvp.Value.IsDefault == true)
                    {
                        return kvp.Value;
                    }
                    if (kvp.Value.IsDefault == null && impliedDefault == null)
                    {
                        impliedDefault = kvp.Value;
                    }
                }

                if (impliedDefault != null)
                {
                    return impliedDefault;
                }

                if (this.Count > 0)
                {
                    return this[this.Keys[0]];
                }

                return null;
            }
        }
    }
}
