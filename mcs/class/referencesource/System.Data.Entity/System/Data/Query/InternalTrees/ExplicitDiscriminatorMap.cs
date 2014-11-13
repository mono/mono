//---------------------------------------------------------------------
// <copyright file="ExplicitDiscriminatorMap.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Metadata.Edm;
using System.Collections.Generic;
using System.Linq;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// Describes user-defined discriminator metadata (e.g. for a basic TPH mapping). Encapsulates
    /// relevant data from System.Data.Mapping.ViewGenerabetion.DiscriminatorMap (that is to say,
    /// data relevant to the PlanCompiler). This separate class accomplishes two things:
    /// 
    /// 1. Maintain separation of ViewGen and PlanCompiler
    /// 2. Avoid holding references to CQT expressions in ITree ops (which the ViewGen.DiscriminatorMap
    /// holds a few CQT references)
    /// </summary>
    internal class ExplicitDiscriminatorMap
    {
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<KeyValuePair<object, EntityType>> m_typeMap;
        private readonly EdmMember m_discriminatorProperty;
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<EdmProperty> m_properties;

        internal ExplicitDiscriminatorMap(System.Data.Mapping.ViewGeneration.DiscriminatorMap template)
        {
            m_typeMap = template.TypeMap;
            m_discriminatorProperty = template.Discriminator.Property;
            m_properties = template.PropertyMap.Select(propertyValuePair => propertyValuePair.Key)
                .ToList().AsReadOnly();
        }

        /// <summary>
        /// Maps from discriminator value to type.
        /// </summary>
        internal System.Collections.ObjectModel.ReadOnlyCollection<KeyValuePair<object, EntityType>> TypeMap
        {
            get { return m_typeMap; }
        }

        /// <summary>
        /// Gets property containing discriminator value.
        /// </summary>
        internal EdmMember DiscriminatorProperty
        {
            get { return m_discriminatorProperty; }
        }


        /// <summary>
        /// All properties for the type hierarchy.
        /// </summary>
        internal System.Collections.ObjectModel.ReadOnlyCollection<EdmProperty> Properties
        {
            get { return m_properties; }
        }

        /// <summary>
        /// Returns the type id for the given entity type, or null if non exists.
        /// </summary>
        internal object GetTypeId(EntityType entityType)
        {
            object result = null;
            foreach (var discriminatorTypePair in this.TypeMap)
            {
                if (discriminatorTypePair.Value.EdmEquals(entityType))
                {
                    result = discriminatorTypePair.Key;
                    break;
                }
            }
            return result;
        }
    }
}
