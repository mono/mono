//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines a role descriptor.
    /// </summary>
    public abstract class RoleDescriptor
    {
        Collection<ContactPerson> contacts = new Collection<ContactPerson>();
        Uri errorUrl;
        Collection<KeyDescriptor> keys = new Collection<KeyDescriptor>();
        Organization organization;
        Collection<Uri> protocolsSupported = new Collection<Uri>();
        DateTime validUntil = DateTime.MaxValue;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        protected RoleDescriptor()
            : this(new Collection<Uri>())
        {
        }

        /// <summary>
        /// Constructs a role descriptor with a collection of supported protocols.
        /// </summary>
        /// <param name="protocolsSupported">The supported protocol collection.</param>
        protected RoleDescriptor(Collection<Uri> protocolsSupported)
        {
            this.protocolsSupported = protocolsSupported;
        }

        /// <summary>
        /// Gets the collection of <see cref="ContactPerson"/>.
        /// </summary>
        public ICollection<ContactPerson> Contacts
        {
            get { return this.contacts; }
        }

        /// <summary>
        /// Gets or sets the error url.
        /// </summary>
        public Uri ErrorUrl
        {
            get { return this.errorUrl; }
            set { this.errorUrl = value; }
        }

        /// <summary>
        /// Gets the collection of <see cref="KeyDescriptor"/>.
        /// </summary>
        public ICollection<KeyDescriptor> Keys
        {
            get { return this.keys; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Organization"/>.
        /// </summary>
        public Organization Organization
        {
            get { return this.organization; }
            set { this.organization = value; }
        }

        /// <summary>
        /// Gets the collection of protocols supported.
        /// </summary>
        public ICollection<Uri> ProtocolsSupported
        {
            get { return this.protocolsSupported; }
        }

        /// <summary>
        /// Gets or sets the expiration time.
        /// </summary>
        public DateTime ValidUntil
        {
            get { return this.validUntil; }
            set { this.validUntil = value; }
        }

    }
}
