//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.Globalization;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines the localized entry.
    /// </summary>
    public abstract class LocalizedEntry
    {
        CultureInfo language;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        protected LocalizedEntry()
            : this(null)
        {
        }

        /// <summary>
        /// Constructor that uses a <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="language">The <see cref="CultureInfo"/> for this instance.</param>
        protected LocalizedEntry(CultureInfo language)
        {
            this.language = language;
        }

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/>.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">If value is null.</exception>
        public CultureInfo Language
        {
            get { return this.language; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.language = value;
            }
        }
    }
}
