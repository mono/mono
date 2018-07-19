//---------------------------------------------------------------------
// <copyright file="ObjectFullSpanRewriter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupowner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    internal class ObjectFullSpanRewriter : ObjectSpanRewriter
    {
        /// <summary>
        /// Represents a node in the 'Include' navigation property tree
        /// built from the list of SpanPaths on the Span object with which
        /// the FullSpanRewriter is constructed.
        /// </summary>
        private class SpanPathInfo
        {
            internal SpanPathInfo(EntityType declaringType)
            {
                this.DeclaringType = declaringType;
            }

            /// <summary>
            /// The effective Entity type of this node in the tree
            /// </summary>
            internal EntityType DeclaringType;

            /// <summary>
            /// Describes the navigation properties that should be retrieved
            /// from this node in the tree and the Include sub-paths that extend
            /// from each of those navigation properties
            /// </summary>
            internal Dictionary<NavigationProperty, SpanPathInfo> Children;
        }

        /// <summary>
        /// Maintains a reference to the SpanPathInfo tree node representing the
        /// current position in the 'Include' path that is currently being expanded.
        /// </summary>
        private Stack<SpanPathInfo> _currentSpanPath = new Stack<SpanPathInfo>();

        internal ObjectFullSpanRewriter(DbCommandTree tree, DbExpression toRewrite, Span span, AliasGenerator aliasGenerator)
            : base(tree, toRewrite, aliasGenerator)
        {
            Debug.Assert(span != null, "Span cannot be null");
            Debug.Assert(span.SpanList.Count > 0, "At least one span path is required");

            // Retrieve the effective 'T' of the ObjectQuery<T> that produced
            // the Command Tree that is being rewritten. This could be either
            // literally 'T' or Collection<T>.
            EntityType entityType = null;
            if (!TryGetEntityType(this.Query.ResultType, out entityType))
            {
                // If the result type of the query is neither an Entity type nor a collection
                // type with an Entity element type, then full Span is currently not allowed.
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectQuery_Span_IncludeRequiresEntityOrEntityCollection);
            }

            // Construct the SpanPathInfo navigation property tree using the
            // list of Include Span paths from the Span object:
            // Create a SpanPathInfo instance that represents the root of the tree
            // and takes its Entity type from the Entity type of the result type of the query.
            SpanPathInfo spanRoot = new SpanPathInfo(entityType);
            
            // Populate the tree of navigation properties based on the navigation property names
            // in the Span paths from the Span object. Commonly rooted span paths are merged, so
            // that paths of "Customer.Order" and "Customer.Address", for example, will share a
            // common SpanPathInfo for "Customer" in the Children collection of the root SpanPathInfo,
            // and that SpanPathInfo will contain one child for "Order" and another for "Address".
            foreach (Span.SpanPath path in span.SpanList)
            {
                AddSpanPath(spanRoot, path.Navigations);
            }

            // The 'current' span path is initialized to the root of the Include span tree
            _currentSpanPath.Push(spanRoot);
        }

        /// <summary>
        /// Populates the Include span tree with appropriate branches for the Include path
        /// represented by the specified list of navigation property names.
        /// </summary>
        /// <param name="parentInfo">The root SpanPathInfo</param>
        /// <param name="navPropNames">A list of navigation property names that describes a single Include span path</param>
        private void AddSpanPath(SpanPathInfo parentInfo, List<string> navPropNames)
        {
            ConvertSpanPath(parentInfo, navPropNames, 0);
        }

        private void ConvertSpanPath(SpanPathInfo parentInfo, List<string> navPropNames, int pos)
        {
            // Attempt to retrieve the next navigation property from the current entity type
            // using the name of the current navigation property in the Include path.
            NavigationProperty nextNavProp = null;
            if (!parentInfo.DeclaringType.NavigationProperties.TryGetValue(navPropNames[pos], true, out nextNavProp))
            {
                // The navigation property name is not valid for this Entity type
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectQuery_Span_NoNavProp(parentInfo.DeclaringType.FullName, navPropNames[pos]));
            }

            // The navigation property was retrieved, an entry for it must be ensured in the Children
            // collection of the parent SpanPathInfo instance.
            // If the parent's Children collection does not exist then instantiate it now:
            if (null == parentInfo.Children)
            {
                parentInfo.Children = new Dictionary<NavigationProperty, SpanPathInfo>();
            }

            // If a sub-path that begins with the current navigation property name was already
            // encountered, then a SpanPathInfo for this navigation property may already exist
            // in the Children dictionary...
            SpanPathInfo nextChild = null;
            if (!parentInfo.Children.TryGetValue(nextNavProp, out nextChild))
            {
                // ... otherwise, create a new SpanPathInfo instance that this navigation
                // property maps to and ensure its presence in the Children dictionary.
                nextChild = new SpanPathInfo(EntityTypeFromResultType(nextNavProp));
                parentInfo.Children[nextNavProp] = nextChild;
            }

            // If this navigation property is not the end of the span path then
            // increment the position and recursively call ConvertSpanPath, specifying
            // the (retrieved or newly-created) SpanPathInfo of this navigation property
            // as the new 'parent' info.
            if (pos < navPropNames.Count - 1)
            {
                ConvertSpanPath(nextChild, navPropNames, pos + 1);
            }
        }

        /// <summary>
        /// Retrieves the Entity (result or element) type produced by a Navigation Property.
        /// </summary>
        /// <param name="navProp">The navigation property</param>
        /// <returns>
        ///     The Entity type produced by the navigation property. 
        ///     This may be the immediate result type (if the result is at most one)
        ///     or the element type of the result type, otherwise.
        /// </returns>
        private static EntityType EntityTypeFromResultType(NavigationProperty navProp)
        {
            EntityType retType = null;
            TryGetEntityType(navProp.TypeUsage, out retType);
            // Currently, navigation properties may only return an Entity or Collection<Entity> result
            Debug.Assert(retType != null, "Navigation property has non-Entity and non-Entity collection result type?");
            return retType;
        }

        /// <summary>
        /// Retrieves the Entity (result or element) type referenced by the specified TypeUsage, if
        /// its EdmType is an Entity type or a collection type with an Entity element type.
        /// </summary>
        /// <param name="resultType">The TypeUsage that provides the EdmType to examine</param>
        /// <param name="entityType">The referenced Entity (element) type, if present.</param>
        /// <returns>
        ///     <c>true</c> if the specified <paramref name="resultType"/> is an Entity type or a 
        ///     collection type with an Entity element type; otherwise <c>false</c>.
        /// </returns>
        private static bool TryGetEntityType(TypeUsage resultType, out EntityType entityType)
        {
            // If the result type is an Entity, then simply use that type.
            if (BuiltInTypeKind.EntityType == resultType.EdmType.BuiltInTypeKind)
            {
                entityType = (EntityType)resultType.EdmType;
                return true;
            }
            else if (BuiltInTypeKind.CollectionType == resultType.EdmType.BuiltInTypeKind)
            {
                // If the result type of the query is a collection, attempt to extract
                // the element type of the collection and determine if it is an Entity type.
                EdmType elementType = ((CollectionType)resultType.EdmType).TypeUsage.EdmType;
                if (BuiltInTypeKind.EntityType == elementType.BuiltInTypeKind)
                {
                    entityType = (EntityType)elementType;
                    return true;
                }
            }

            entityType = null;
            return false;
        }

        /// <summary>
        /// Utility method to retrieve the 'To' AssociationEndMember of a NavigationProperty
        /// </summary>
        /// <param name="property">The navigation property</param>
        /// <returns>The AssociationEndMember that is the target of the navigation operation represented by the NavigationProperty</returns>
        private AssociationEndMember GetNavigationPropertyTargetEnd(NavigationProperty property)
        {
            AssociationType relationship = this.Metadata.GetItem<AssociationType>(property.RelationshipType.FullName, DataSpace.CSpace);
            Debug.Assert(relationship.AssociationEndMembers.Contains(property.ToEndMember.Name), "Association does not declare member referenced by Navigation property?");
            return relationship.AssociationEndMembers[property.ToEndMember.Name];
        }

        internal override SpanTrackingInfo CreateEntitySpanTrackingInfo(DbExpression expression, EntityType entityType)
        {
            SpanTrackingInfo tracking = new SpanTrackingInfo();

            SpanPathInfo currentInfo = _currentSpanPath.Peek();
            if (currentInfo.Children != null)
            {
                // The current SpanPathInfo instance on the top of the span path stack indicates
                // which navigation properties should be retrieved from this Entity-typed expression
                // and also specifies (in the form of child SpanPathInfo instances) which sub-paths
                // must be expanded for each of those navigation properties.
                // The SpanPathInfo instance may be the root instance or a SpanPathInfo that represents a sub-path.
                int idx = 1; // SpanRoot is always the first (zeroth) column, full- and relationship-span columns follow.
                foreach (KeyValuePair<NavigationProperty, SpanPathInfo> nextInfo in currentInfo.Children)
                {
                    // If the tracking information was not initialized yet, do so now.
                    if (null == tracking.ColumnDefinitions)
                    {
                        tracking = InitializeTrackingInfo(this.RelationshipSpan);
                    }

                    // Create a property expression that retrieves the specified navigation property from the Entity-typed expression.
                    // Note that the expression is cloned since it may be used as the instance of multiple property expressions.
                    DbExpression columnDef = expression.Property(nextInfo.Key);

                    // Rewrite the result of the navigation property. This is required for two reasons:
                    // 1. To continue spanning the current Include path.
                    // 2. To apply relationship span to the Entity or EntityCollection produced by the navigation property, if necessary.
                    //    Consider an Include path of "Order" for a query that returns OrderLines - the Include'd Orders should have
                    //    their associated Customer relationship spanned.
                    // Note that this will recursively call this method with the Entity type of the result of the
                    // navigation property, which will in turn call loop through the sub-paths of this navigation
                    // property and adjust the stack to track which Include path is being expanded and which 
                    // element of that path is considered 'current'.
                    _currentSpanPath.Push(nextInfo.Value);
                    columnDef = this.Rewrite(columnDef);
                    _currentSpanPath.Pop();

                    // Add a new column to the tracked columns using the rewritten column definition
                    tracking.ColumnDefinitions.Add(new KeyValuePair<string, DbExpression>(tracking.ColumnNames.Next(), columnDef));
                    AssociationEndMember targetEnd = GetNavigationPropertyTargetEnd(nextInfo.Key);
                    tracking.SpannedColumns[idx] = targetEnd;

                    // If full span and relationship span are both required, a relationship span may be rendered
                    // redundant by an already added full span. Therefore the association ends that have been expanded
                    // as part of full span are tracked using a dictionary.
                    if (this.RelationshipSpan)
                    {
                        tracking.FullSpannedEnds[targetEnd] = true;
                    }

                    idx++;
                }
            }

            return tracking;
        }
    }
}
