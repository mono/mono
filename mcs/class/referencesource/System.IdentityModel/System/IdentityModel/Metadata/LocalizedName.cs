//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Metadata
{
    using System.Globalization;

    /// <summary>
    /// Defines a localized name.
    /// </summary>
    public class LocalizedName : LocalizedEntry
    {
        string name;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public LocalizedName()
            : this(null, null)
        {
        }

        /// <summary>
        /// Constructs a localized name with the input <paramref name="name"/> and <paramref name="language"/>.
        /// </summary>
        /// <param name="name">The name for this instance.</param>
        /// <param name="language">The <see cref="CultureInfo"/> defining the language for this instance.</param>
        public LocalizedName(string name, CultureInfo language)
            : base(language)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
}
