//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Globalization;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines a localized URI.
    /// </summary>
    public class LocalizedUri : LocalizedEntry
    {
        Uri _uri;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public LocalizedUri()
            : this(null, null)
        {
        }

        /// <summary>
        /// Constructs a <see cref="LocalizedUri"/> with the <paramref name="uri"/> and <paramref name="language"/>.
        /// </summary>
        /// <param name="uri">The URI for this instance.</param>
        /// <param name="language">The <see cref="CultureInfo"/> defining the language for this instance.</param>
        public LocalizedUri(Uri uri, CultureInfo language)
            : base(language)
        {
            Uri = uri;
        }

        /// <summary>
        /// Gets or sets the URI.
        /// </summary>
        public Uri Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }
    }
}
