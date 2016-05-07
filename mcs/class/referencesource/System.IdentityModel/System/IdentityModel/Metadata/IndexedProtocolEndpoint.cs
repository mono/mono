//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines an indexed <see cref="ProtocolEndpoint"/>.
    /// </summary>
    public class IndexedProtocolEndpoint : ProtocolEndpoint
    {
        int _index;
        bool? _isDefault = null; // This has tristate due to the way default is calculated in an indexed collection

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public IndexedProtocolEndpoint()
        {
        }

        /// <summary>
        /// Constructs an indexed endpoint with the index number, binding, and the location.
        /// </summary>
        /// <param name="index">The index number.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="location">The location.</param>
        public IndexedProtocolEndpoint(int index, Uri binding, Uri location)
            : base(binding, location)
        {
            _index = index;
        }

        /// <summary>
        /// Gets or sets the index. This is a required element.
        /// </summary>
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is the default endpoint. This is optional.
        /// </summary>
        public bool? IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; }
        }
    }
}
