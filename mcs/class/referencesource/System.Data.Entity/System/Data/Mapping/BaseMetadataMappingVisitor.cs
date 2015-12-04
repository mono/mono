//---------------------------------------------------------------------
// <copyright file="BaseMetadataMappingVisitor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

// @owner       [....]
// @backupOwner [....]
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
    internal abstract class BaseMetadataMappingVisitor
    {
        protected BaseMetadataMappingVisitor()
        {
        }

        protected virtual void Visit(StorageEntityContainerMapping storageEntityContainerMapping)
        {
            Visit(storageEntityContainerMapping.EdmEntityContainer);
            Visit(storageEntityContainerMapping.StorageEntityContainer);
            foreach (var mapping in storageEntityContainerMapping.EntitySetMaps)
            {
                Visit(mapping);
            }
        }

        protected virtual void Visit(EntitySetBase entitySetBase)
        {
            // this is a switching node, so no object header and footer will be add for this node,
            // also this Visit won't add the object to the seen list

            switch (entitySetBase.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntitySet:
                    Visit((EntitySet)entitySetBase);
                    break;
                case BuiltInTypeKind.AssociationSet:
                    Visit((AssociationSet)entitySetBase);
                    break;
                default:
                    Debug.Fail(string.Format(CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", entitySetBase.BuiltInTypeKind));
                    break;
            }
        }

        protected virtual void Visit(StorageSetMapping storageSetMapping)
        {
            foreach (var typeMapping in storageSetMapping.TypeMappings)
            {
                Visit(typeMapping);
            }
            Visit(storageSetMapping.EntityContainerMapping);
        }

        protected virtual void Visit(EntityContainer entityContainer)
        {
            foreach (var set in entityContainer.BaseEntitySets)
            {
                Visit(set);
            }
        }

        protected virtual void Visit(EntitySet entitySet)
        {
            Visit(entitySet.ElementType);
            Visit(entitySet.EntityContainer);
        }

        protected virtual void Visit(AssociationSet associationSet)
        {
            Visit(associationSet.ElementType);
            Visit(associationSet.EntityContainer);
            foreach (var end in associationSet.AssociationSetEnds)
            {
                Visit(end);
            }
        }
        protected virtual void Visit(EntityType entityType)
        {
            foreach (var kmember in entityType.KeyMembers)
            {
                Visit(kmember);
            }

            foreach (var member in entityType.GetDeclaredOnlyMembers<EdmMember>())
            {
                Visit(member);
            }

            foreach (var nproperty in entityType.NavigationProperties)
            {
                Visit(nproperty);
            }

            foreach (var property in entityType.Properties)
            {
                Visit(property);
            }
        }

        protected virtual void Visit(AssociationType associationType)
        {
            foreach (var endMember in associationType.AssociationEndMembers)
            {
                Visit(endMember);
            }
            Visit(associationType.BaseType);
            foreach (var keyMember in associationType.KeyMembers)
            {
                Visit(keyMember);
            }
            foreach (var member in associationType.GetDeclaredOnlyMembers<EdmMember>())
            {
                Visit(member);
            }
            foreach (var item in associationType.ReferentialConstraints)
            {
                Visit(item);
            }
            foreach (var item in associationType.RelationshipEndMembers)
            {
                Visit(item);
            }
        }

        protected virtual void Visit(AssociationSetEnd associationSetEnd)
        {
            Visit(associationSetEnd.CorrespondingAssociationEndMember);
            Visit(associationSetEnd.EntitySet);
            Visit(associationSetEnd.ParentAssociationSet);
        }
        protected virtual void Visit(EdmProperty edmProperty)
        {
            Visit(edmProperty.TypeUsage);
        }
        protected virtual void Visit(NavigationProperty navigationProperty)
        {
            Visit(navigationProperty.FromEndMember);
            Visit(navigationProperty.RelationshipType);
            Visit(navigationProperty.ToEndMember);
            Visit(navigationProperty.TypeUsage);
        }

        protected virtual void Visit(EdmMember edmMember)
        {
            Visit(edmMember.TypeUsage);
        }
        protected virtual void Visit(AssociationEndMember associationEndMember)
        {
            Visit(associationEndMember.TypeUsage);
        }

        protected virtual void Visit(ReferentialConstraint referentialConstraint)
        {
            foreach (var property in referentialConstraint.FromProperties)
            {
                Visit(property);
            }
            Visit(referentialConstraint.FromRole);

            foreach (var property in referentialConstraint.ToProperties)
            {
                Visit(property);
            }
            Visit(referentialConstraint.ToRole);
        }
        protected virtual void Visit(RelationshipEndMember relationshipEndMember)
        {
            Visit(relationshipEndMember.TypeUsage);
        }
        protected virtual void Visit(TypeUsage typeUsage)
        {
            Visit(typeUsage.EdmType);
            foreach (var facet in typeUsage.Facets)
            {
                Visit(facet);
            }
        }
        protected virtual void Visit(RelationshipType relationshipType)
        {
            // switching node, will not be add to the seen list
            if (relationshipType == null)
            {
                return;
            }

            #region Inner data visit
            switch (relationshipType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.AssociationType:
                    Visit((AssociationType)relationshipType);
                    break;
                default:
                    Debug.Fail(String.Format(CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", relationshipType.BuiltInTypeKind));
                    break;
            }
            #endregion
        }
        protected virtual void Visit(EdmType edmType)
        {
            // switching node, will not be add to the seen list
            if (edmType == null)
            {
                return;
            }

            #region Inner data visit
            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntityType:
                    Visit((EntityType)edmType);
                    break;
                case BuiltInTypeKind.AssociationType:
                    Visit((AssociationType)edmType);
                    break;
                case BuiltInTypeKind.EdmFunction:
                    Visit((EdmFunction)edmType);
                    break;
                case BuiltInTypeKind.ComplexType:
                    Visit((ComplexType)edmType);
                    break;
                case BuiltInTypeKind.PrimitiveType:
                    Visit((PrimitiveType)edmType);
                    break;
                case BuiltInTypeKind.RefType:
                    Visit((RefType)edmType);
                    break;
                case BuiltInTypeKind.CollectionType:
                    Visit((CollectionType)edmType);
                    break;
                case BuiltInTypeKind.EnumType:
                    Visit((EnumType)edmType);
                    break;
                default:
                    Debug.Fail(String.Format(CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", edmType.BuiltInTypeKind));
                    break;
            }
            #endregion
        }
        protected virtual void Visit(Facet facet)
        {
            Visit(facet.FacetType);
        }
        protected virtual void Visit(EdmFunction edmFunction)
        {
            Visit(edmFunction.BaseType);
            foreach (var entitySet in edmFunction.EntitySets)
            {
                if (entitySet != null)
                {
                    Visit(entitySet);
                }
            }
            foreach (var functionParameter in edmFunction.Parameters)
            {
                Visit(functionParameter);
            }
            foreach (var returnParameter in edmFunction.ReturnParameters)
            {
                Visit(returnParameter);
            }
        }
        protected virtual void Visit(PrimitiveType primitiveType)
        {
        }
        protected virtual void Visit(ComplexType complexType)
        {
            Visit(complexType.BaseType);
            foreach (var member in complexType.Members)
            {
                Visit(member);
            }
            foreach (var property in complexType.Properties)
            {
                Visit(property);
            }
        }
        protected virtual void Visit(RefType refType)
        {
            Visit(refType.BaseType);
            Visit(refType.ElementType);
        }
        protected virtual void Visit(EnumType enumType)
        {
            foreach (var member in enumType.Members)
            {
                Visit(member);
            }
        }
        protected virtual void Visit(EnumMember enumMember)
        {
        }
        protected virtual void Visit(CollectionType collectionType)
        {
            Visit(collectionType.BaseType);
            Visit(collectionType.TypeUsage);
        }
        protected virtual void Visit(EntityTypeBase entityTypeBase)
        {
            // switching node
            if (entityTypeBase == null)
            {
                return;
            }
            switch (entityTypeBase.BuiltInTypeKind)
            {
                case BuiltInTypeKind.AssociationType:
                    Visit((AssociationType)entityTypeBase);
                    break;
                case BuiltInTypeKind.EntityType:
                    Visit((EntityType)entityTypeBase);
                    break;
                default:
                    Debug.Fail(String.Format(CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", entityTypeBase.BuiltInTypeKind));
                    break;
            }
        }
        protected virtual void Visit(FunctionParameter functionParameter)
        {
            Visit(functionParameter.DeclaringFunction);
            Visit(functionParameter.TypeUsage);
        }
        protected virtual void Visit(DbProviderManifest providerManifest)
        {
        }
        protected virtual void Visit(StorageTypeMapping storageTypeMapping)
        {
            foreach (var type in storageTypeMapping.IsOfTypes)
            {
                Visit(type);
            }

            foreach (var fragment in storageTypeMapping.MappingFragments)
            {
                Visit(fragment);
            }

            Visit(storageTypeMapping.SetMapping);

            foreach (var type in storageTypeMapping.Types)
            {
                Visit(type);
            }
        }
        protected virtual void Visit(StorageMappingFragment storageMappingFragment)
        {
            foreach (var property in storageMappingFragment.AllProperties)
            {
                Visit(property);
            }

            Visit((EntitySetBase)storageMappingFragment.TableSet);
        }
        protected virtual void Visit(StoragePropertyMapping storagePropertyMapping)
        {
            // this is a switching node, so no object header and footer will be add for this node,
            // also this Visit won't add the object to the seen list

            if (storagePropertyMapping.GetType() == typeof(StorageComplexPropertyMapping))
            {
                Visit((StorageComplexPropertyMapping)storagePropertyMapping);
            }
            else if (storagePropertyMapping.GetType() == typeof(StorageConditionPropertyMapping))
            {
                Visit((StorageConditionPropertyMapping)storagePropertyMapping);
            }
            else if (storagePropertyMapping.GetType() == typeof(StorageScalarPropertyMapping))
            {
                Visit((StorageScalarPropertyMapping)storagePropertyMapping);
            }
            else
            {
                Debug.Fail(String.Format(CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", storagePropertyMapping.GetType()));
            }
        }
        protected virtual void Visit(StorageComplexPropertyMapping storageComplexPropertyMapping)
        {
            Visit(storageComplexPropertyMapping.EdmProperty);
            foreach (var mapping in storageComplexPropertyMapping.TypeMappings)
            {
                Visit(mapping);
            }
        }
        protected virtual void Visit(StorageConditionPropertyMapping storageConditionPropertyMapping)
        {
            Visit(storageConditionPropertyMapping.ColumnProperty);
            Visit(storageConditionPropertyMapping.EdmProperty);
        }
        protected virtual void Visit(StorageScalarPropertyMapping storageScalarPropertyMapping)
        {
            Visit(storageScalarPropertyMapping.ColumnProperty);
            Visit(storageScalarPropertyMapping.EdmProperty);
        }

        protected virtual void Visit(StorageComplexTypeMapping storageComplexTypeMapping)
        {
            foreach (var property in storageComplexTypeMapping.AllProperties)
            {
                Visit(property);
            }

            foreach (var type in storageComplexTypeMapping.IsOfTypes)
            {
                Visit(type);
            }

            foreach (var type in storageComplexTypeMapping.Types)
            {
                Visit(type);
            }
        }
    }
}
