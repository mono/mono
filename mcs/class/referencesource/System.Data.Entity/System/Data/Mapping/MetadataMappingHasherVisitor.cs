//---------------------------------------------------------------------
// <copyright file="MetadataMappingHasherVisitor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Metadata.Edm;
using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.Mapping;
using System.Diagnostics;
using System.Globalization;


namespace System.Data.Mapping
{
    internal partial class MetadataMappingHasherVisitor : BaseMetadataMappingVisitor
    {
        private CompressingHashBuilder m_hashSourceBuilder;
        private Dictionary<Object, int> m_itemsAlreadySeen = new Dictionary<Object, int>();
        private int m_instanceNumber = 0;
        private EdmItemCollection m_EdmItemCollection;
        private double m_EdmVersion;
        private double m_MappingVersion;

        private MetadataMappingHasherVisitor(double mappingVersion)
        {
            m_MappingVersion = mappingVersion;
            this.m_hashSourceBuilder = new CompressingHashBuilder(MetadataHelper.CreateMetadataHashAlgorithm(m_MappingVersion));
        }
        
        #region visitor method
        protected override void Visit(StorageEntityContainerMapping storageEntityContainerMapping)
        {
            Debug.Assert(storageEntityContainerMapping != null, "storageEntityContainerMapping cannot be null!");

            // at the entry point of visitor, we setup the versions
            Debug.Assert(m_MappingVersion == storageEntityContainerMapping.StorageMappingItemCollection.MappingVersion, "the original version and the mapping collection version are not the same");
            this.m_MappingVersion = storageEntityContainerMapping.StorageMappingItemCollection.MappingVersion;
            this.m_EdmVersion = storageEntityContainerMapping.StorageMappingItemCollection.EdmItemCollection.EdmVersion;

            this.m_EdmItemCollection = storageEntityContainerMapping.StorageMappingItemCollection.EdmItemCollection;

            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(storageEntityContainerMapping, out index))
            {
                // if this has been add to the seen list, then just 
                return;
            }
            if (this.m_itemsAlreadySeen.Count > 1)
            {

                // this means user try another visit over SECM, this is allowed but all the previous visit all lost due to clean
                // user can visit different SECM objects by using the same visitor to load the SECM object
                this.Clean();
                Visit(storageEntityContainerMapping);
                return;
            }

            this.AddObjectStartDumpToHashBuilder(storageEntityContainerMapping, index);

            #region Inner data visit

            this.AddObjectContentToHashBuilder(storageEntityContainerMapping.Identity);

            this.AddV2ObjectContentToHashBuilder(storageEntityContainerMapping.GenerateUpdateViews, this.m_MappingVersion);

            base.Visit(storageEntityContainerMapping);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(EntityContainer entityContainer)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(entityContainer, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(entityContainer, index);

            #region Inner data visit
            
            this.AddObjectContentToHashBuilder(entityContainer.Identity);
            // Name is covered by Identity

            base.Visit(entityContainer);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(StorageSetMapping storageSetMapping)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(storageSetMapping, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(storageSetMapping, index);

            #region Inner data visit
            base.Visit(storageSetMapping);
            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(StorageTypeMapping storageTypeMapping)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(storageTypeMapping, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(storageTypeMapping, index);

            #region Inner data visit

            base.Visit(storageTypeMapping);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(StorageMappingFragment storageMappingFragment)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(storageMappingFragment, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(storageMappingFragment, index);

            #region Inner data visit

            this.AddV2ObjectContentToHashBuilder(storageMappingFragment.IsSQueryDistinct, this.m_MappingVersion);

            base.Visit(storageMappingFragment);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }
        
        protected override void Visit(StoragePropertyMapping storagePropertyMapping)
        {
            base.Visit(storagePropertyMapping);
        }

        protected override void Visit(StorageComplexPropertyMapping storageComplexPropertyMapping)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(storageComplexPropertyMapping, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(storageComplexPropertyMapping, index);

            #region Inner data visit

            base.Visit(storageComplexPropertyMapping);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }
        protected override void Visit(StorageComplexTypeMapping storageComplexTypeMapping)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(storageComplexTypeMapping, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(storageComplexTypeMapping, index);

            #region Inner data visit

            base.Visit(storageComplexTypeMapping);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }
        
        protected override void Visit(StorageConditionPropertyMapping storageConditionPropertyMapping)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(storageConditionPropertyMapping, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(storageConditionPropertyMapping, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(storageConditionPropertyMapping.IsNull);
            this.AddObjectContentToHashBuilder(storageConditionPropertyMapping.Value);

            base.Visit(storageConditionPropertyMapping);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(StorageScalarPropertyMapping storageScalarPropertyMapping)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(storageScalarPropertyMapping, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(storageScalarPropertyMapping, index);

            #region Inner data visit

            base.Visit(storageScalarPropertyMapping);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }
        
        protected override void Visit(EntitySetBase entitySetBase)
        {
            base.Visit(entitySetBase);
        }
        
        protected override void Visit(EntitySet entitySet)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(entitySet, out index))
            {
                return;
            }
            #region Inner data visit

            this.AddObjectStartDumpToHashBuilder(entitySet, index);
            this.AddObjectContentToHashBuilder(entitySet.Name);
            this.AddObjectContentToHashBuilder(entitySet.Schema);
            this.AddObjectContentToHashBuilder(entitySet.Table);

            base.Visit(entitySet);

            foreach (var entityType in MetadataHelper.GetTypeAndSubtypesOf(entitySet.ElementType, this.m_EdmItemCollection, false).Where(type => type != entitySet.ElementType))
            {
                this.Visit(entityType);
            }

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(AssociationSet associationSet)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(associationSet, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(associationSet, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(associationSet.CachedProviderSql);
            // Name is coverd by Identity
            this.AddObjectContentToHashBuilder(associationSet.Identity);
            this.AddObjectContentToHashBuilder(associationSet.Schema);
            this.AddObjectContentToHashBuilder(associationSet.Table);

            base.Visit(associationSet);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(EntityType entityType)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(entityType, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(entityType, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(entityType.Abstract);
            this.AddObjectContentToHashBuilder(entityType.Identity);
            // FullName, Namespace and Name are all covered by Identity

            base.Visit(entityType);
 
            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(AssociationSetEnd associationSetEnd)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(associationSetEnd, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(associationSetEnd, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(associationSetEnd.Identity);
            // Name is covered by Identity

            base.Visit(associationSetEnd);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(AssociationType associationType)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(associationType, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(associationType, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(associationType.Abstract);
            this.AddObjectContentToHashBuilder(associationType.Identity);
            // FullName, Namespace, and Name are all covered by Identity

            base.Visit(associationType);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(EdmProperty edmProperty)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(edmProperty, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(edmProperty, index);

            #region Inner data visit
            // since the delaring type is fixed and referenced to the upper type, 
            // there is no need to hash this
            //this.AddObjectContentToHashBuilder(edmProperty.DeclaringType);
            this.AddObjectContentToHashBuilder(edmProperty.DefaultValue);
            this.AddObjectContentToHashBuilder(edmProperty.Identity);
            // Name is covered by Identity
            this.AddObjectContentToHashBuilder(edmProperty.IsStoreGeneratedComputed);
            this.AddObjectContentToHashBuilder(edmProperty.IsStoreGeneratedIdentity);
            this.AddObjectContentToHashBuilder(edmProperty.Nullable);

            base.Visit(edmProperty);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(NavigationProperty navigationProperty)
        {
            // navigation properties are not considered in view generation
            return;
        }

        protected override void Visit(EdmMember edmMember)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(edmMember, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(edmMember, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(edmMember.Identity);
            // Name is covered by Identity
            this.AddObjectContentToHashBuilder(edmMember.IsStoreGeneratedComputed);
            this.AddObjectContentToHashBuilder(edmMember.IsStoreGeneratedIdentity);

            base.Visit(edmMember);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(AssociationEndMember associationEndMember)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(associationEndMember, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(associationEndMember, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(associationEndMember.DeleteBehavior);
            this.AddObjectContentToHashBuilder(associationEndMember.Identity);
            // Name is covered by Identity
            this.AddObjectContentToHashBuilder(associationEndMember.IsStoreGeneratedComputed);
            this.AddObjectContentToHashBuilder(associationEndMember.IsStoreGeneratedIdentity);
            this.AddObjectContentToHashBuilder(associationEndMember.RelationshipMultiplicity);

            base.Visit(associationEndMember);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }
        
        protected override void Visit(ReferentialConstraint referentialConstraint)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(referentialConstraint, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(referentialConstraint, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(referentialConstraint.Identity);

            base.Visit(referentialConstraint);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(RelationshipEndMember relationshipEndMember)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(relationshipEndMember, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(relationshipEndMember, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(relationshipEndMember.DeleteBehavior);
            this.AddObjectContentToHashBuilder(relationshipEndMember.Identity);
            // Name is covered by Identity
            this.AddObjectContentToHashBuilder(relationshipEndMember.IsStoreGeneratedComputed);
            this.AddObjectContentToHashBuilder(relationshipEndMember.IsStoreGeneratedIdentity);
            this.AddObjectContentToHashBuilder(relationshipEndMember.RelationshipMultiplicity);

            base.Visit(relationshipEndMember);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(TypeUsage typeUsage)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(typeUsage, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(typeUsage, index);

            #region Inner data visit
            //No need to add identity of TypeUsage to the hash since it would take into account
            //facets that viewgen would not care and we visit the important facets anyway.

            base.Visit(typeUsage);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(RelationshipType relationshipType)
        {
            base.Visit(relationshipType);
        }

        protected override void Visit(EdmType edmType)
        {
            base.Visit(edmType);
        }
        
        protected override void Visit(EnumType enumType)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(enumType, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(enumType, index);

            this.AddObjectContentToHashBuilder(enumType.Identity);
            this.Visit(enumType.UnderlyingType);

            base.Visit(enumType);

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(EnumMember enumMember)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(enumMember, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(enumMember, index);

            this.AddObjectContentToHashBuilder(enumMember.Name);
            this.AddObjectContentToHashBuilder(enumMember.Value);

            base.Visit(enumMember);

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(CollectionType collectionType)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(collectionType, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(collectionType, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(collectionType.Identity);
            // Identity contains Name, NamespaceName and FullName

            base.Visit(collectionType);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }
        
        protected override void Visit(RefType refType)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(refType, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(refType, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(refType.Identity);
            // Identity contains Name, NamespaceName and FullName

            base.Visit(refType);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(EntityTypeBase entityTypeBase)
        {
            base.Visit(entityTypeBase);
        }

        protected override void Visit(Facet facet)
        {
            int index;
            if (facet.Name != DbProviderManifest.NullableFacetName)
            {
                // skip all the non interesting facets
                return;
            }

            if (!this.AddObjectToSeenListAndHashBuilder(facet, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(facet, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(facet.Identity);
            // Identity already contains Name
            this.AddObjectContentToHashBuilder(facet.Value);

            base.Visit(facet);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(EdmFunction edmFunction)
        {
            // View Generation doesn't deal with functions
            // so just return;
        }
        
        protected override void Visit(ComplexType complexType)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(complexType, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(complexType, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(complexType.Abstract);
            this.AddObjectContentToHashBuilder(complexType.Identity);
            // Identity covers, FullName, Name, and NamespaceName

            base.Visit(complexType);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }
        
        protected override void Visit(PrimitiveType primitiveType)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(primitiveType, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(primitiveType, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(primitiveType.Name);
            this.AddObjectContentToHashBuilder(primitiveType.NamespaceName);

            base.Visit(primitiveType);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }
        
        protected override void Visit(FunctionParameter functionParameter)
        {
            int index;
            if (!this.AddObjectToSeenListAndHashBuilder(functionParameter, out index))
            {
                return;
            }

            this.AddObjectStartDumpToHashBuilder(functionParameter, index);

            #region Inner data visit
            this.AddObjectContentToHashBuilder(functionParameter.Identity);
            // Identity already has Name
            this.AddObjectContentToHashBuilder(functionParameter.Mode);

            base.Visit(functionParameter);

            #endregion

            this.AddObjectEndDumpToHashBuilder();
        }

        protected override void Visit(DbProviderManifest providerManifest)
        {
            // the provider manifest will be checked by all the other types lining up.
            // no need to store more info.
        }
        #endregion

        #region hasher helper method

        internal string HashValue
        {
            get
            {
                return m_hashSourceBuilder.ComputeHash();
            }
        }

        private void Clean()
        {
            this.m_hashSourceBuilder = new CompressingHashBuilder(MetadataHelper.CreateMetadataHashAlgorithm(m_MappingVersion));
            this.m_instanceNumber = 0;
            this.m_itemsAlreadySeen = new Dictionary<object, int>();
        }

        /// <summary>
        /// if already seen, then out the object instance index, return false;
        /// if haven't seen, then add it to the m_itemAlreadySeen, out the current index, return true
        /// </summary>
        /// <param name="o"></param>
        /// <param name="indexSeen"></param>
        /// <returns></returns>
        private bool TryAddSeenItem(Object o, out int indexSeen)
        {
            if (!this.m_itemsAlreadySeen.TryGetValue(o, out indexSeen))
            {
                this.m_itemsAlreadySeen.Add(o, this.m_instanceNumber);

                indexSeen = this.m_instanceNumber;
                this.m_instanceNumber++;

                return true;
            }
            return false;
        }

        /// <summary>
        /// if the object has seen, then add the seen object style to the hash source, return false;
        /// if not, then add it to the seen list, and append the object start dump to the hash source, return true
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool AddObjectToSeenListAndHashBuilder(object o, out int instanceIndex)
        {
            if (o == null)
            {
                instanceIndex = -1;
                return false;
            }
            if (!TryAddSeenItem(o, out instanceIndex))
            {
                this.AddObjectStartDumpToHashBuilder(o, instanceIndex);
                this.AddSeenObjectToHashBuilder(o, instanceIndex);
                this.AddObjectEndDumpToHashBuilder();
                return false;
            }
            return true;
        }

        private void AddSeenObjectToHashBuilder(object o, int instanceIndex)
        {
            Debug.Assert(instanceIndex >= 0, "referencing index should not be less than 0");
            this.m_hashSourceBuilder.AppendLine("Instance Reference: " + instanceIndex);
        }

        private void AddObjectStartDumpToHashBuilder(object o, int objectIndex)
        {
            this.m_hashSourceBuilder.AppendObjectStartDump(o, objectIndex);
        }

        private void AddObjectEndDumpToHashBuilder()
        {
            this.m_hashSourceBuilder.AppendObjectEndDump();
        }

        private void AddObjectContentToHashBuilder(object content)
        {
            if (content != null)
            {
                IFormattable formatContent = content as IFormattable;
                if (formatContent != null)
                {
                    // if the content is formattable, the following code made it culture invariant,
                    // for instance, the int, "30,000" can be formatted to "30-000" if the user 
                    // has a different language and region setting
                    this.m_hashSourceBuilder.AppendLine(formatContent.ToString(null, CultureInfo.InvariantCulture));
                }
                else
                {
                    this.m_hashSourceBuilder.AppendLine(content.ToString());
                }
            }
            else
            {
                this.m_hashSourceBuilder.AppendLine("NULL");
            }
        }

        /// <summary>
        /// Add V2 schema properties and attributes to the hash builder
        /// </summary>
        /// <param name="content"></param>
        /// <param name="defaultValue"></param>
        private void AddV2ObjectContentToHashBuilder(object content, double version)
        {
            // if the version number is greater than or equal to V2, then we add the value
            if (version >= XmlConstants.EdmVersionForV2)
            {
                this.AddObjectContentToHashBuilder(content);
            }
        }

        internal static string GetMappingClosureHash(double mappingVersion, StorageEntityContainerMapping storageEntityContainerMapping)
        {
            Debug.Assert(storageEntityContainerMapping != null, "storageEntityContainerMapping is null!");

            MetadataMappingHasherVisitor visitor = new MetadataMappingHasherVisitor(mappingVersion);
            visitor.Visit(storageEntityContainerMapping);
            return visitor.HashValue;
        }
        #endregion

    }
}
