//---------------------------------------------------------------------
// <copyright file="LazyLoadedCollectionBehavior.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using System.Collections;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// Defines and injects behavior into proxy class Type definitions
    /// to allow navigation properties to lazily load their references or collection elements.
    /// </summary>
    internal sealed class LazyLoadBehavior
    {
        /// <summary>
        /// Return an expression tree that represents the actions required to load the related end
        /// associated with the intercepted proxy member.
        /// </summary>
        /// <param name="member">
        /// EdmMember that specifies the member to be intercepted.
        /// </param>
        /// <param name="property">
        /// PropertyInfo that specifies the CLR property to be intercepted.
        /// </param>
        /// <param name="proxyParameter">
        /// ParameterExpression that represents the proxy object.
        /// </param>
        /// <param name="itemParameter">
        /// ParameterExpression that represents the proxied property value.
        /// </param>
        /// <param name="getEntityWrapperDelegate">The Func that retrieves the wrapper from a proxy</param>
        /// <returns>
        /// Expression tree that encapsulates lazy loading behavior for the supplied member,
        /// or null if the expression tree could not be constructed.
        /// </returns>
        internal static Func<TProxy, TItem, bool> GetInterceptorDelegate<TProxy, TItem>(EdmMember member, Func<object, object> getEntityWrapperDelegate) 
            where TProxy : class
            where TItem : class 
        {
            Func<TProxy, TItem, bool> interceptorDelegate = (proxy, item) => true;

            Debug.Assert(member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty, "member should represent a navigation property");
            if (member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
            {
                NavigationProperty navProperty = (NavigationProperty)member;
                RelationshipMultiplicity multiplicity = navProperty.ToEndMember.RelationshipMultiplicity;

                // Given the proxy and item parameters, construct one of the following expressions:
                //
                // For collections:
                //  LazyLoadBehavior.LoadCollection(collection, "relationshipName", "targetRoleName", proxy._entityWrapperField)
                //
                // For entity references:
                //  LazyLoadBehavior.LoadReference(item, "relationshipName", "targetRoleName", proxy._entityWrapperField)
                //
                // Both of these expressions return an object of the same type as the first parameter to LoadXYZ method.
                // In many cases, this will be the first parameter.

                if (multiplicity == RelationshipMultiplicity.Many)
                {
                    interceptorDelegate = (proxy, item) => LoadProperty<TItem>(item,
                                                                               navProperty.RelationshipType.Identity,
                                                                               navProperty.ToEndMember.Identity,
                                                                               false,
                                                                               getEntityWrapperDelegate(proxy));
                }
                else
                {
                    interceptorDelegate = (proxy, item) => LoadProperty<TItem>(item,
                                                                               navProperty.RelationshipType.Identity,
                                                                               navProperty.ToEndMember.Identity,
                                                                               true,
                                                                               getEntityWrapperDelegate(proxy));
                }
            }

            return interceptorDelegate;
        }

        /// <summary>
        /// Determine if the specified member is compatible with lazy loading.
        /// </summary>
        /// <param name="ospaceEntityType">
        /// OSpace EntityType representing a type that may be proxied.
        /// </param>
        /// <param name="member">
        /// Member of the <paramref name="ospaceEntityType" /> to be examined.
        /// </param>
        /// <returns>
        /// True if the member is compatible with lazy loading; otherwise false.
        /// </returns>
        /// <remarks>
        /// To be compatible with lazy loading, 
        /// a member must meet the criteria for being able to be proxied (defined elsewhere),
        /// and must be a navigation property.
        /// In addition, for relationships with a multiplicity of Many,
        /// the property type must be an implementation of ICollection&lt;T&gt;.
        /// </remarks>
        internal static bool IsLazyLoadCandidate(EntityType ospaceEntityType, EdmMember member)
        {
            Debug.Assert(ospaceEntityType.DataSpace == DataSpace.OSpace, "ospaceEntityType.DataSpace must be OSpace");

            bool isCandidate = false;

            if (member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
            {
                NavigationProperty navProperty = (NavigationProperty)member;
                RelationshipMultiplicity multiplicity = navProperty.ToEndMember.RelationshipMultiplicity;

                PropertyInfo propertyInfo = EntityUtil.GetTopProperty(ospaceEntityType.ClrType, member.Name);
                Debug.Assert(propertyInfo != null, "Should have found lazy loading property");
                Type propertyValueType = propertyInfo.PropertyType;

                if (multiplicity == RelationshipMultiplicity.Many)
                {
                    Type elementType;
                    isCandidate = EntityUtil.TryGetICollectionElementType(propertyValueType, out elementType);
                }
                else if (multiplicity == RelationshipMultiplicity.One || multiplicity == RelationshipMultiplicity.ZeroOrOne)
                {
                    // This is an EntityReference property.
                    isCandidate = true;
                }
            }

            return isCandidate;
        }

        /// <summary>
        /// Method called by proxy interceptor delegate to provide lazy loading behavior for navigation properties.
        /// </summary>
        /// <typeparam name="TItem">property type</typeparam>
        /// <param name="propertyValue">The property value whose associated relationship is to be loaded.</param>
        /// <param name="relationshipName">String name of the relationship.</param>
        /// <param name="targetRoleName">String name of the related end to be loaded for the relationship specified by <paramref name="relationshipName"/>.</param>
        /// <param name="wrapperObject">Entity wrapper object used to retrieve RelationshipManager for the proxied entity.</param>
        /// <returns>
        /// True if the value instance was mutated and can be returned
        /// False if the class should refetch the value because the instance has changed
        /// </returns>
        private static bool LoadProperty<TItem>(TItem propertyValue, string relationshipName, string targetRoleName, bool mustBeNull, object wrapperObject) where TItem : class
        {
            // Only attempt to load collection if:
            //
            // 1. Collection is non-null.
            // 2. ObjectContext.ContextOptions.LazyLoadingEnabled is true
            // 3. A non-null RelationshipManager can be retrieved (this is asserted).
            // 4. The EntityCollection is not already loaded.

            Debug.Assert(wrapperObject == null || wrapperObject is IEntityWrapper, "wrapperObject must be an IEntityWrapper");
            IEntityWrapper wrapper = (IEntityWrapper)wrapperObject; // We want an exception if the cast fails.

            if (wrapper != null && wrapper.Context != null)
            {
                RelationshipManager relationshipManager = wrapper.RelationshipManager;
                Debug.Assert(relationshipManager != null, "relationshipManager should be non-null");
                if (relationshipManager != null && (!mustBeNull || propertyValue == null))
                {
                    RelatedEnd relatedEnd = relationshipManager.GetRelatedEndInternal(relationshipName, targetRoleName);
                    relatedEnd.DeferredLoad();
                }
            }

            return propertyValue != null;
        }
    }
}
