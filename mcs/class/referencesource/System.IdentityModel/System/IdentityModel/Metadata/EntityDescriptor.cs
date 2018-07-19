//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Describes the entity descriptor class.
    /// </summary>
    public class EntityDescriptor : MetadataBase
    {
        Collection<ContactPerson> contacts = new Collection<ContactPerson>();
        EntityId entityId;
        string federationId;
        Organization organization;
        Collection<RoleDescriptor> roleDescriptors = new Collection<RoleDescriptor>();

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public EntityDescriptor()
            : this(null)
        {
        }

        /// <summary>
        /// Constructs an entity descriptor with the entity id.
        /// </summary>
        /// <param name="entityId">The <see cref="EntityId"/> of this instance.</param>
        public EntityDescriptor(EntityId entityId)
        {
            this.entityId = entityId;
        }

        /// <summary>
        /// Gets the collection of <see cref="ContactPerson"/>.
        /// </summary>
        public ICollection<ContactPerson> Contacts
        {
            get { return this.contacts; }
        }

        /// <summary>
        /// Gets or sets the <see cref="EntityId"/>.
        /// </summary>
        public EntityId EntityId
        {
            get { return this.entityId; }
            set { this.entityId = value; }
        }

        /// <summary>
        /// Gets or sets the federation id.
        /// </summary>
        public string FederationId
        {
            get { return this.federationId; }
            set { this.federationId = value; }
        }

        /// <summary>
        /// Gets or sets the organization.
        /// </summary>
        public Organization Organization
        {
            get { return this.organization; }
            set { this.organization = value; }
        }

        /// <summary>
        /// Gets the collection of role descriptors.
        /// </summary>
        public ICollection<RoleDescriptor> RoleDescriptors
        {
            get { return this.roleDescriptors; }
        }
    }
}
