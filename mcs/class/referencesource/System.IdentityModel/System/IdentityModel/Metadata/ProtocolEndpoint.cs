//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// This class defines a protocol endpoint.
    /// </summary>
    public class ProtocolEndpoint
    {
        Uri binding;
        Uri location;
        Uri responseLocation;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ProtocolEndpoint()
            : this(null, null)
        {
        }

        /// <summary>
        /// Constructs an endpoint with the specified <paramref name="binding"/> and <paramref name="location"/>.
        /// </summary>
        /// <param name="binding">The URI representing the binding for this instance.</param>
        /// <param name="location">The URI representing the location for this instance.</param>
        public ProtocolEndpoint(Uri binding, Uri location)
        {
            Binding = binding;
            Location = location;
        }

        /// <summary>
        /// Gets or sets the binding. This is a required element.
        /// </summary>
        public Uri Binding
        {
            get { return this.binding; }

            set { this.binding = value; }
        }

        /// <summary>
        /// Gets or sets the location. This is a required element.
        /// </summary>
        public Uri Location
        {
            get { return this.location; }

            set { this.location = value; }
        }

        /// <summary>
        /// Gets or sets the response location. This is an optional element.
        /// </summary>
        public Uri ResponseLocation
        {
            get { return this.responseLocation; }

            set { this.responseLocation = value; }
        }
    }
}
