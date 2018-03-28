//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// The entities descriptor class, defines a collection of entities.
    /// </summary>
    public class EntitiesDescriptor : MetadataBase
    {
        Collection<EntitiesDescriptor> entityGroupCollection = new Collection<EntitiesDescriptor>();
        Collection<EntityDescriptor> entityCollection = new Collection<EntityDescriptor>();
        string name;

        /// <summary>
        /// The empty constructor.
        /// </summary>
        public EntitiesDescriptor()
            : this(new Collection<EntityDescriptor>(), new Collection<EntitiesDescriptor>())
        {
        }

        /// <summary>
        /// Constructs an entities descriptor with a collection of <see cref="EntitiesDescriptor"/>.
        /// </summary>
        /// <param name="entityGroupList">The collection of entities descriptor.</param>
        public EntitiesDescriptor(Collection<EntitiesDescriptor> entityGroupList)
        {
            this.entityGroupCollection = entityGroupList;
        }

        /// <summary>
        /// Constructs an entities descriptor with a collection of <see cref="EntityDescriptor"/>.
        /// </summary>
        /// <param name="entityList">The collection of entity descriptor.</param>
        public EntitiesDescriptor(Collection<EntityDescriptor> entityList)
        {
            this.entityCollection = entityList;
        }

        /// <summary>
        /// Constructs an entities descriptor with a collection of <see cref="EntityDescriptor"/>
        /// and a collection of <see cref="EntitiesDescriptor"/>.
        /// </summary>
        /// <param name="entityList">The entity descriptor collection.</param>
        /// <param name="entityGroupList">The entities descriptor collection.</param>
        public EntitiesDescriptor(Collection<EntityDescriptor> entityList, Collection<EntitiesDescriptor> entityGroupList)
        {
            this.entityCollection = entityList;
            this.entityGroupCollection = entityGroupList;
        }

        /// <summary>
        /// Gets the collection of child <see cref="EntityDescriptor"/>.
        /// </summary>
        public ICollection<EntityDescriptor> ChildEntities
        {
            get { return this.entityCollection; }
        }

        /// <summary>
        /// Gets the collection of child <see cref="EntitiesDescriptor"/>.
        /// </summary>
        public ICollection<EntitiesDescriptor> ChildEntityGroups
        {
            get { return this.entityGroupCollection; }
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
