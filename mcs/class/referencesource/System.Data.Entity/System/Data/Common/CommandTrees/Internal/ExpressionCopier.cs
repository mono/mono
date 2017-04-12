//---------------------------------------------------------------------
// <copyright file="DbExpressionRebinder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.CommandTrees.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.EntitySql;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Ensures that all metadata in a given expression tree is from the specified metadata workspace,
    /// potentially rebinding and rebuilding the expressions to appropriate replacement metadata where necessary.
    /// </summary>
    internal class DbExpressionRebinder : DefaultExpressionVisitor
    {
        private readonly MetadataWorkspace _metadata;
        private readonly Perspective _perspective;

        protected DbExpressionRebinder(MetadataWorkspace targetWorkspace)
        {
            Debug.Assert(targetWorkspace != null, "Metadata workspace is null");
            _metadata = targetWorkspace;
            _perspective = new ModelPerspective(targetWorkspace);
        }

        // 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static DbExpression BindToWorkspace(DbExpression expression, MetadataWorkspace targetWorkspace)
        {
            Debug.Assert(expression != null, "expression is null");

            DbExpressionRebinder copier = new DbExpressionRebinder(targetWorkspace);
            return copier.VisitExpression(expression);
        }

        protected override EntitySetBase VisitEntitySet(EntitySetBase entitySet)
        {
            EntityContainer container;
            if (_metadata.TryGetEntityContainer(entitySet.EntityContainer.Name, entitySet.EntityContainer.DataSpace, out container))
            {
                EntitySetBase extent = null;
                if (container.BaseEntitySets.TryGetValue(entitySet.Name, false, out extent) &&
                    extent != null &&
                    entitySet.BuiltInTypeKind == extent.BuiltInTypeKind) // EntitySet -> EntitySet, AssociationSet -> AssociationSet, etc
                {
                    return extent;
                }

                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Copier_EntitySetNotFound(entitySet.EntityContainer.Name, entitySet.Name));
            }

            throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Copier_EntityContainerNotFound(entitySet.EntityContainer.Name));
        }

        protected override EdmFunction VisitFunction(EdmFunction function)
        {
            List<TypeUsage> paramTypes = new List<TypeUsage>(function.Parameters.Count);
            foreach (FunctionParameter funcParam in function.Parameters)
            {
                TypeUsage mappedParamType = this.VisitTypeUsage(funcParam.TypeUsage);
                paramTypes.Add(mappedParamType);
            }

            if (DataSpace.SSpace == function.DataSpace)
            {
                EdmFunction foundFunc = null;
                if (_metadata.TryGetFunction(function.Name,
                                             function.NamespaceName,
                                             paramTypes.ToArray(),
                                             false /* ignoreCase */,
                                             function.DataSpace,
                                             out foundFunc) &&
                    foundFunc != null)
                {
                    return foundFunc;
                }
            }
            else
            {
                // Find the function or function import.
                IList<EdmFunction> candidateFunctions;
                if (_perspective.TryGetFunctionByName(function.NamespaceName, function.Name, /*ignoreCase:*/ false, out candidateFunctions))
                {
                    Debug.Assert(null != candidateFunctions && candidateFunctions.Count > 0, "Perspective.TryGetFunctionByName returned true with null/empty function result list");

                    bool isAmbiguous;
                    EdmFunction retFunc = FunctionOverloadResolver.ResolveFunctionOverloads(candidateFunctions, paramTypes, /*isGroupAggregateFunction:*/ false, out isAmbiguous);
                    if (!isAmbiguous &&
                        retFunc != null)
                    {
                        return retFunc;
                    }
                }
            }

            throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Copier_FunctionNotFound(TypeHelpers.GetFullName(function)));
        }

        protected override EdmType VisitType(EdmType type)
        {
            EdmType retType = type;

            if (BuiltInTypeKind.RefType == type.BuiltInTypeKind)
            {
                RefType refType = (RefType)type;
                EntityType mappedEntityType = (EntityType)this.VisitType(refType.ElementType);
                if (!object.ReferenceEquals(refType.ElementType, mappedEntityType))
                {
                    retType = new RefType(mappedEntityType);
                }
            }
            else if (BuiltInTypeKind.CollectionType == type.BuiltInTypeKind)
            {
                CollectionType collectionType = (CollectionType)type;
                TypeUsage mappedElementType = this.VisitTypeUsage(collectionType.TypeUsage);
                if (!object.ReferenceEquals(collectionType.TypeUsage, mappedElementType))
                {
                    retType = new CollectionType(mappedElementType);
                }
            }
            else if (BuiltInTypeKind.RowType == type.BuiltInTypeKind)
            {
                RowType rowType = (RowType)type;
                List<KeyValuePair<string, TypeUsage>> mappedPropInfo = null;
                for (int idx = 0; idx < rowType.Properties.Count; idx++)
                {
                    EdmProperty originalProp = rowType.Properties[idx];
                    TypeUsage mappedPropType = this.VisitTypeUsage(originalProp.TypeUsage);
                    if (!object.ReferenceEquals(originalProp.TypeUsage, mappedPropType))
                    {
                        if (mappedPropInfo == null)
                        {
                            mappedPropInfo = new List<KeyValuePair<string, TypeUsage>>(
                                                rowType.Properties.Select(
                                                    prop => new KeyValuePair<string, TypeUsage>(prop.Name, prop.TypeUsage)
                                                ));
                        }
                        mappedPropInfo[idx] = new KeyValuePair<string,TypeUsage>(originalProp.Name, mappedPropType);
                    }
                }
                if (mappedPropInfo != null)
                {
                    IEnumerable<EdmProperty> mappedProps = mappedPropInfo.Select(propInfo => new EdmProperty(propInfo.Key, propInfo.Value));
                    retType = new RowType(mappedProps, rowType.InitializerMetadata);
                }
            }
            else
            {
                if (!_metadata.TryGetType(type.Name, type.NamespaceName, type.DataSpace, out retType) ||
                    null == retType)
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Copier_TypeNotFound(TypeHelpers.GetFullName(type)));
                }
            }

            return retType;
        }
                        
        protected override TypeUsage VisitTypeUsage(TypeUsage type)
        {
            //
            // If the target metatadata workspace contains the same type instances, then the type does not
            // need to be 'mapped' and the same TypeUsage instance may be returned. This can happen if the
            // target workspace and the workspace of the source Command Tree are using the same ItemCollection.
            //
            EdmType retEdmType = this.VisitType(type.EdmType);
            if (object.ReferenceEquals(retEdmType, type.EdmType))
            {
                return type;
            }

            //
            // Retrieve the Facets from this type usage so that
            // 1) They can be used to map the type if it is a primitive type
            // 2) They can be applied to the new type usage that references the mapped type
            //
            Facet[] facets = new Facet[type.Facets.Count];
            int idx = 0;
            foreach (Facet f in type.Facets)
            {
                facets[idx] = f;
                idx++;
            }

            return TypeUsage.Create(retEdmType, facets);
        }

        private bool TryGetMember<TMember>(DbExpression instance, string memberName, out TMember member) where TMember : EdmMember
        {
            member = null;
            StructuralType declType = instance.ResultType.EdmType as StructuralType;
            if (declType != null)
            {
                EdmMember foundMember = null;
                if (declType.Members.TryGetValue(memberName, false, out foundMember))
                {
                    member = foundMember as TMember;
                }
            }

            return (member != null);
        }

        public override DbExpression Visit(DbPropertyExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;
            DbExpression newInstance = this.VisitExpression(expression.Instance);
            if (!object.ReferenceEquals(expression.Instance, newInstance))
            {
                if (Helper.IsRelationshipEndMember(expression.Property))
                {
                    RelationshipEndMember endMember;
                    if(!TryGetMember(newInstance, expression.Property.Name, out endMember))
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Copier_EndNotFound(expression.Property.Name, TypeHelpers.GetFullName(newInstance.ResultType.EdmType)));
                    }
                    result = DbExpressionBuilder.Property(newInstance, endMember);
                }
                else if (Helper.IsNavigationProperty(expression.Property))
                {
                    NavigationProperty navProp;
                    if (!TryGetMember(newInstance, expression.Property.Name, out navProp))
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Copier_NavPropertyNotFound(expression.Property.Name, TypeHelpers.GetFullName(newInstance.ResultType.EdmType)));
                    }
                    result = DbExpressionBuilder.Property(newInstance, navProp);
                }
                else
                {
                    EdmProperty prop;
                    if (!TryGetMember(newInstance, expression.Property.Name, out prop))
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Copier_PropertyNotFound(expression.Property.Name, TypeHelpers.GetFullName(newInstance.ResultType.EdmType)));
                    }
                    result = DbExpressionBuilder.Property(newInstance, prop);
                }
            }
            return result;
        }
    }
}
