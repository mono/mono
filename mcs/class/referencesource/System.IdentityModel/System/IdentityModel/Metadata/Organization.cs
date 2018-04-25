//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines an organization.
    /// </summary>
    public class Organization
    {
        //
        // We do not support extensions as yet. So on receive, we should skip parsing it.
        //
        LocalizedEntryCollection<LocalizedName> displayNames = new LocalizedEntryCollection<LocalizedName>();
        LocalizedEntryCollection<LocalizedName> names = new LocalizedEntryCollection<LocalizedName>();
        LocalizedEntryCollection<LocalizedUri> urls = new LocalizedEntryCollection<LocalizedUri>();

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public Organization()
            : this(new LocalizedEntryCollection<LocalizedName>(), new LocalizedEntryCollection<LocalizedName>(), new LocalizedEntryCollection<LocalizedUri>())
        {
        }

        /// <summary>
        /// Creates an organization with collections of names, display names, and URIs
        /// </summary>
        /// <param name="names">A collection of <see cref="LocalizedName"/> for this instance.</param>
        /// <param name="displayNames">A collection of <see cref="LocalizedName"/> for this instance representing the display names.</param>
        /// <param name="urls">A collection of <see cref="LocalizedUri"/> for this instance.</param>
        /// <exception cref="System.ArgumentNullException">If any of the input parameters is null.</exception>
        public Organization(LocalizedEntryCollection<LocalizedName> names, LocalizedEntryCollection<LocalizedName> displayNames, LocalizedEntryCollection<LocalizedUri> urls)
        {
            if (names == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("names");
            }

            if (displayNames == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("displayNames");
            }

            if (urls == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("urls");
            }


            this.names = names;
            this.displayNames = displayNames;
            this.urls = urls;
        }

        /// <summary>
        /// Gets the collection of <see cref="LocalizedName"/> representing the display names.
        /// This is a required element.
        /// </summary>
        public LocalizedEntryCollection<LocalizedName> DisplayNames
        {
            get { return this.displayNames; }
        }

        /// <summary>
        /// Gets the collection of <see cref="LocalizedName"/>.
        /// This is a required element.
        /// </summary>
        public LocalizedEntryCollection<LocalizedName> Names
        {
            get { return this.names; }
        }

        /// <summary>
        /// Gets the collection of <see cref="LocalizedUri"/>.
        /// This is required element.
        /// </summary>
        public LocalizedEntryCollection<LocalizedUri> Urls
        {
            get { return this.urls; }
        }
    }
}
