//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;

    /// <summary>
    /// An abstract class that provides a generic property bag to derived classes.
    /// </summary>
    public abstract class OpenObject
    {
        Dictionary<string, object> _properties = new Dictionary<string, object>(); // for any custom data

        /// <summary>
        /// Gets the properties bag to extend object.
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get { return _properties; }
        }
    }
}
