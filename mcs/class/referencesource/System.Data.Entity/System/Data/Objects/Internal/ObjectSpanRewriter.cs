//---------------------------------------------------------------------
// <copyright file="ObjectSpanRewriter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupowner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
using System.Globalization;
using System.Data.Common.CommandTrees.ExpressionBuilder;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// Responsible for performing Relationship-span only rewrites over a Command Tree rooted
    /// by the <see cref="Query"/> property. Virtual methods provide an opportunity for derived
    /// classes to implement Full-span rewrites.
    /// </summary>
    internal class ObjectSpanRewriter
    {
        internal static bool EntityTypeEquals(EntityTypeBase entityType1, EntityTypeBase entityType2)
        {
            return object.ReferenceEquals(entityType1, entityType2);
        }

        #region Private members

        private int _spanCount;
        private SpanIndex _spanIndex;
        private DbExpression _toRewrite;
        private bool _relationshipSpan;
        private DbCommandTree _tree;
        private Stack<NavigationInfo> _navSources = new Stack<NavigationInfo>();
        private readonly AliasGenerator _aliasGenerator;
        
        #endregion

        #region 'Public' API

        internal static bool TryRewrite(DbQueryCommandTree tree, Span span, MergeOption mergeOption, AliasGenerator aliasGenerator, out DbExpression newQuery, out SpanIndex spanInfo)
        {
            newQuery = null;
            spanInfo = null;

            ObjectSpanRewriter rewriter = null;
            bool requiresRelationshipSpan = Span.RequiresRelationshipSpan(mergeOption);

            // Potentially perform a rewrite for span.
            // Note that the public 'Span' property is NOT used to retrieve the Span instance
            // since this forces creation of a Span object that may not be required.
            if (span != null && span.SpanList.Count > 0)
            {
                rewriter = new ObjectFullSpanRewriter(tree, tree.Query, span, aliasGenerator);
            }
            else if (requiresRelationshipSpan)
            {
                rewriter = new ObjectSpanRewriter(tree, tree.Query, aliasGenerator);
            }

            if (rewriter != null)
            {
                rewriter.RelationshipSpan = requiresRelationshipSpan;
                newQuery = rewriter.RewriteQuery();
                if (newQuery != null)
                {
                    Debug.Assert(rewriter.SpanIndex != null || tree.Query.ResultType.EdmEquals(newQuery.ResultType), "Query was rewritten for Span but no SpanIndex was created?");
                    spanInfo = rewriter.SpanIndex;
                }
            }

            return (spanInfo != null);
        }

        /// <summary>
        /// Constructs a new ObjectSpanRewriter that will attempt to apply spanning to the specified query
        /// (represented as a DbExpression) when <see cref="RewriteQuery"/> is called.
        /// </summary>
        /// <param name="toRewrite">A <see cref="DbExpression"/> representing the query to span.</param>
        internal ObjectSpanRewriter(DbCommandTree tree, DbExpression toRewrite, AliasGenerator aliasGenerator)
        {
            Debug.Assert(toRewrite != null, "Expression to rewrite cannot be null");

            _toRewrite = toRewrite;
            _tree = tree;
            _aliasGenerator = aliasGenerator;
        }

        /// <summary>
        /// Gets the metadata workspace the will be used to retrieve required metadata, for example association types.
        /// </summary>
        internal MetadataWorkspace Metadata { get { return _tree.MetadataWorkspace; } }
                
        /// <summary>
        /// Gets a DbExpression representing the query that should be spanned.
        /// </summary>
        internal DbExpression Query { get { return _toRewrite; } }
        
        /// <summary>
        /// Gets a value indicating whether relationship span is required (ObjectQuery sets this to 'false' for NoTracking queries).
        /// </summary>
        internal bool RelationshipSpan { get { return _relationshipSpan; } set { _relationshipSpan = value; } }
        
        /// <summary>
        /// Gets a dictionary that indicates, for a given result row type produced by a span rewrite, 
        /// which columns represent which association end members.
        /// This dictionary is initially empty before <see cref="RewriteQuery"/> is called and will remain so
        /// if no rewrites are required.
        /// </summary>
        internal SpanIndex SpanIndex { get { return _spanIndex; } }

        /// <summary>
        /// Main 'public' entry point called by ObjectQuery.
        /// </summary>
        /// <returns>The rewritten version of <see cref="Query"/> if spanning was required; otherwise <c>null</c>.</returns>
        internal DbExpression RewriteQuery()
        {
            DbExpression retExpr = Rewrite(_toRewrite);
            if (object.ReferenceEquals(_toRewrite, retExpr))
            {
                return null;
            }
            else
            {
                return retExpr;
            }
        }

        #endregion

        #region 'Protected' API
        
        internal struct SpanTrackingInfo
        {
            public List<KeyValuePair<string, DbExpression>> ColumnDefinitions;
            public AliasGenerator ColumnNames;
            public Dictionary<int, AssociationEndMember> SpannedColumns;
            public Dictionary<AssociationEndMember, bool> FullSpannedEnds;
        }

        internal SpanTrackingInfo InitializeTrackingInfo(bool createAssociationEndTrackingInfo)
        {
            SpanTrackingInfo info = new SpanTrackingInfo();
            info.ColumnDefinitions = new List<KeyValuePair<string, DbExpression>>();
            info.ColumnNames = new AliasGenerator(string.Format(CultureInfo.InvariantCulture, "Span{0}_Column", _spanCount));
            info.SpannedColumns = new Dictionary<int, AssociationEndMember>();
            if (createAssociationEndTrackingInfo)
            {
                info.FullSpannedEnds = new Dictionary<AssociationEndMember, bool>();
            }

            return info;
        }

        internal virtual SpanTrackingInfo CreateEntitySpanTrackingInfo(DbExpression expression, EntityType entityType) { return new SpanTrackingInfo(); }
        
        protected DbExpression Rewrite(DbExpression expression)
        {
            //SQLBUDT #554182: This is special casing for expressions below which it is safe to push the span
            // info without having to rebind.  By pushing the span info down (i.e. possible extra projections),
            // we potentially end up with simpler generated command. 
            switch(expression.ExpressionKind)
            {
                case DbExpressionKind.Element:
                    return RewriteElementExpression((DbElementExpression)expression);
                case DbExpressionKind.Limit:
                    return RewriteLimitExpression((DbLimitExpression)expression);
            }

            switch(expression.ResultType.EdmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntityType:
                    return RewriteEntity(expression, (EntityType)expression.ResultType.EdmType);

                case BuiltInTypeKind.CollectionType:
                    return RewriteCollection(expression, (CollectionType)expression.ResultType.EdmType);

                case BuiltInTypeKind.RowType:
                    return RewriteRow(expression, (RowType)expression.ResultType.EdmType);

                default:
                    return expression;
            }
        }

        #endregion

        private void AddSpannedRowType(RowType spannedType, TypeUsage originalType)
        {
            if (null == _spanIndex)
            {
                _spanIndex = new SpanIndex();
            }

            _spanIndex.AddSpannedRowType(spannedType, originalType);
        }

        private void AddSpanMap(RowType rowType, Dictionary<int, AssociationEndMember> columnMap)
        {
            if (null == _spanIndex)
            {
                _spanIndex = new SpanIndex();
            }

            _spanIndex.AddSpanMap(rowType, columnMap);
        }

        private DbExpression RewriteEntity(DbExpression expression, EntityType entityType)
        {
            // If the expression is an Entity constructor, spanning will not produce any useful results
            // (null for an Entity/Ref navigation property, or an empty collection for a Collection 
            // of Entity/Ref navigation property) since a Ref produced from the constructed Entity
            // will not indicate an Entity set, and therefore no Ref created against any Entity set
            // in the container can possibly be a match for it.
            if (DbExpressionKind.NewInstance == expression.ExpressionKind)
            {
                return expression;
            }

            // Save the span count for later use.
            _spanCount++;
            int thisSpan = _spanCount;

            SpanTrackingInfo tracking = CreateEntitySpanTrackingInfo(expression, entityType);
 
            // If relationship span is required then attempt to span any appropriate relationship ends.
            List<KeyValuePair<AssociationEndMember, AssociationEndMember>> relationshipSpans = null;
            relationshipSpans = GetRelationshipSpanEnds(entityType);
            // Is the Entity type of this expression valid as the source of at least one relationship span?
            if (relationshipSpans != null)
            {
                // If the span tracking information was not initialized by CreateEntitySpanTrackingInfo,
                // then do so now as relationship span rewrites need to be tracked.
                if (null == tracking.ColumnDefinitions)
                {
                    tracking = InitializeTrackingInfo(false);
                }
                                
                // Track column index to span information, starting at the current column count (which could be zero) plus 1.
                // 1 is added because the column containing the root entity will be added later to provide column zero.
                int idx = tracking.ColumnDefinitions.Count + 1;
                // For all applicable relationship spans that were identified...
                foreach (KeyValuePair<AssociationEndMember, AssociationEndMember> relSpan in relationshipSpans)
                {
                    // If the specified association end member was already full-spanned then the full entity
                    // will be returned in the query and there is no need to relationship-span this end to produce
                    // another result column that contains the Entity key of the full entity.
                    // Hence the relationship span is only added if there are no full-span columns or the full-span
                    // columns do not indicate that they include the target association end member of this relationship span.
                    if( null == tracking.FullSpannedEnds ||
                        !tracking.FullSpannedEnds.ContainsKey(relSpan.Value))
                    {
                        // If the source Ref is already available, because the currently spanned Entity is
                        // the result of a Relationship Navigation operation from that Ref, then use the source
                        // Ref directly rather than introducing a new Navigation operation.
                        DbExpression columnDef = null;
                        if(!TryGetNavigationSource(relSpan.Value, out columnDef))
                        {
                            // Add a new column defined by the navigation required to reach the targeted association end
                            // and update the column -> association end map to include an entry for this new column.
                            DbExpression navSource = expression.GetEntityRef();
                            columnDef = navSource.NavigateAllowingAllRelationshipsInSameTypeHierarchy(relSpan.Key, relSpan.Value);
                        }
                        
                        tracking.ColumnDefinitions.Add(
                            new KeyValuePair<string, DbExpression>(
                                tracking.ColumnNames.Next(),
                                columnDef
                            )
                        );

                        tracking.SpannedColumns[idx] = relSpan.Value;

                        // Increment the tracked column count
                        idx++;
                    }
                }
            }

            // If no spanned columns have been added then simply return the original expression
            if (null == tracking.ColumnDefinitions)
            {
                _spanCount--;
                return expression;
            }

            // Add the original entity-producing expression as the first (root) span column.
            tracking.ColumnDefinitions.Insert(
                0,
                new KeyValuePair<string, DbExpression>(
                    string.Format(CultureInfo.InvariantCulture, "Span{0}_SpanRoot", thisSpan),
                    expression
                )
            );

            // Create the span row-producing NewInstanceExpression from which the span RowType can be retrieved.
            DbExpression spannedExpression = DbExpressionBuilder.NewRow(tracking.ColumnDefinitions);
            
            // Update the rowtype -> spaninfo map for the newly created row type instance.
            RowType spanRowType = (RowType)spannedExpression.ResultType.EdmType;
            AddSpanMap(spanRowType, tracking.SpannedColumns);
                       
            // Return the rewritten expression
            return spannedExpression;
        }

        private DbExpression RewriteElementExpression(DbElementExpression expression)
        {
            DbExpression rewrittenInput = Rewrite(expression.Argument);
            if (!object.ReferenceEquals(expression.Argument, rewrittenInput))
            {
                expression = rewrittenInput.Element();
            }
            return expression;
        }

        private DbExpression RewriteLimitExpression(DbLimitExpression expression)
        {
            DbExpression rewrittenInput = Rewrite(expression.Argument);
            if (!object.ReferenceEquals(expression.Argument, rewrittenInput))
            {
                // Note that here we use the original expression.Limit. It is safe to do so, 
                //  because we only allow physical paging (i.e. Limit can only be a constant or parameter)
                expression = rewrittenInput.Limit(expression.Limit);
            }
            return expression;
        }

        private DbExpression RewriteRow(DbExpression expression, RowType rowType)
        {
            DbLambdaExpression lambdaExpression = expression as DbLambdaExpression;
            DbNewInstanceExpression newRow;

            if (lambdaExpression != null)
            {
                // NOTE: We rely on the fact that today span cannot be done over queries containing DbLambdaExpressions
                // created by users, because user-created expressions cannot be used for querying in O-space.
                // If that were to change, pushing span beyond a LambdaExpression could cause variable name 
                // collisions between the variable names used in the Lambda and the names generated by the
                // RelationshipNavigationVisitor.           
                newRow = lambdaExpression.Lambda.Body as DbNewInstanceExpression;
            }
            else
            {
                newRow = expression as DbNewInstanceExpression;
            }

            Dictionary<int, DbExpression> unmodifiedColumns = null;
            Dictionary<int, DbExpression> spannedColumns = null;
            for(int idx = 0; idx < rowType.Properties.Count; idx++)
            {
                // Retrieve the property that represents the current column
                EdmProperty columnProp = rowType.Properties[idx];

                // Construct an expression that defines the current column.
                DbExpression columnExpr = null;
                if(newRow != null)
                {
                    // For a row-constructing NewInstance expression, the corresponding argument can simply be used
                    columnExpr = newRow.Arguments[idx];
                }
                else
                {
                    // For all other expressions the property corresponding to the column name must be retrieved
                    // from the row-typed expression
                    columnExpr = expression.Property(columnProp.Name);
                }

                DbExpression spannedColumn = this.Rewrite(columnExpr);
                if (!object.ReferenceEquals(spannedColumn, columnExpr))
                {
                    // If so, then update the dictionary of column index to span information
                    if (null == spannedColumns)
                    {
                        spannedColumns = new Dictionary<int, DbExpression>();
                    }

                    spannedColumns[idx] = spannedColumn;
                }
                else
                {
                    // Otherwise, update the dictionary of column index to unmodified expression
                    if(null == unmodifiedColumns)
                    {
                        unmodifiedColumns = new Dictionary<int, DbExpression>();
                    }

                    unmodifiedColumns[idx] = columnExpr;
                }
            }
            
            // A new expression need only be built if at least one column was spanned
            if(null == spannedColumns)
            {
                // No columns were spanned, indicate that the original expression should remain.
                return expression;
            }
            else
            {
                // At least one column was spanned, so build a new row constructor that defines the new row, including spanned columns.
                List<DbExpression> columnArguments = new List<DbExpression>(rowType.Properties.Count);
                List<EdmProperty> properties = new List<EdmProperty>(rowType.Properties.Count);
                for (int idx = 0; idx < rowType.Properties.Count; idx++)
                {
                    EdmProperty columnProp = rowType.Properties[idx];
                    DbExpression columnDef = null;
                    if (!spannedColumns.TryGetValue(idx, out columnDef))
                    {
                        columnDef = unmodifiedColumns[idx];
                    }
                    columnArguments.Add(columnDef);
                    properties.Add(new EdmProperty(columnProp.Name, columnDef.ResultType));
                }

                // Copy over any eLinq initializer metadata (if present, or null if not).
                // Note that this initializer metadata does not strictly match the new row type
                // that includes spanned columns, but will be correct once the object materializer
                // has interpreted the query results to produce the correct value for each colum.
                RowType rewrittenRow = new RowType(properties, rowType.InitializerMetadata);
                TypeUsage rewrittenRowTypeUsage = TypeUsage.Create(rewrittenRow);
                DbExpression rewritten = rewrittenRowTypeUsage.New(columnArguments);
                
                // SQLBUDT #554182: If we insert a new projection we should should make sure to 
                // not interfere with the nullability of the input. 
                // In particular, if the input row is null and we construct a new row as a projection over its columns
                // we would get a row consisting of nulls, instead of a null row. 
                // Thus, given an input X, we rewritte it as:  if (X is null) then NULL else rewritten.
                if (newRow == null)
                {
                    DbExpression condition = DbExpressionBuilder.CreateIsNullExpressionAllowingRowTypeArgument(expression);
                    DbExpression nullExpression = DbExpressionBuilder.Null(rewrittenRowTypeUsage);
                    rewritten = DbExpressionBuilder.Case(
                        new List<DbExpression>(new DbExpression[] { condition }),
                        new List<DbExpression>(new DbExpression[] { nullExpression }),
                        rewritten);
                }
                
                // Add an entry to the spanned row type => original row type map for the new row type.
                AddSpannedRowType(rewrittenRow, expression.ResultType);
                
                if (lambdaExpression != null && newRow != null)
                {
                    rewritten = DbLambda.Create(rewritten, lambdaExpression.Lambda.Variables).Invoke(lambdaExpression.Arguments);
                }

                return rewritten;
            }
        }
        
        private DbExpression RewriteCollection(DbExpression expression, CollectionType collectionType)
        {
            DbExpression target = expression;

            // If the collection expression is a project expression, get a strongly typed reference to it for later use.
            DbProjectExpression project = null;
            if (DbExpressionKind.Project == expression.ExpressionKind)
            {
                project = (DbProjectExpression)expression;
                target = project.Input.Expression;
            }

            // If Relationship span is enabled and the source of this collection is (directly or indirectly)
            // a RelationshipNavigation operation, it may be possible to optimize the relationship span rewrite
            // for the Entities produced by the navigation. 
            NavigationInfo navInfo = null;
            if (this.RelationshipSpan)
            {
                // Attempt to find a RelationshipNavigationExpression in the collection-defining expression
                target = RelationshipNavigationVisitor.FindNavigationExpression(target, _aliasGenerator, out navInfo);
            }

            // If a relationship navigation expression defines this collection, make the Ref that is the navigation source
            // and the source association end available for possible use when the projection over the collection is rewritten.
            if (navInfo != null)
            {
                this.EnterNavigationCollection(navInfo);
            }
            else
            {
                // Otherwise, add a null navigation info instance to the stack to indicate that relationship navigation
                // cannot be optimized for the entities produced by this collection expression (if it is a collection of entities).
                this.EnterCollection();
            }
            
            // If the expression is already a DbProjectExpression then simply visit the projection,
            // instead of introducing another projection over the existing one.
            DbExpression result = expression;
            if (project != null)
            {
                DbExpression newProjection = this.Rewrite(project.Projection);
                if (!object.ReferenceEquals(project.Projection, newProjection))
                {
                    result = target.BindAs(project.Input.VariableName).Project(newProjection);
                }
            }
            else
            {
                // This is not a recognized special case, so simply add the span projection over the original
                // collection-producing expression, if it is required.
                DbExpressionBinding collectionBinding = target.BindAs(_aliasGenerator.Next());
                DbExpression projection = collectionBinding.Variable;

                DbExpression spannedProjection = this.Rewrite(projection);

                if (!object.ReferenceEquals(projection, spannedProjection))
                {
                    result = collectionBinding.Project(spannedProjection);
                }
            }

            // Remove any navigation information from scope, if it was added
            this.ExitCollection();

            // If a navigation expression defines this collection and its navigation information was used to
            // short-circuit relationship span rewrites, then enclose the entire rewritten expression in a
            // Lambda binding that brings the source Ref of the navigation operation into scope. This ref is
            // refered to by VariableReferenceExpressions in the original navigation expression as well as any
            // short-circuited relationship span columns in the rewritten expression.
            if (navInfo != null && navInfo.InUse)
            {
                // Create a Lambda function that binds the original navigation source expression under the variable name
                // used in the navigation expression and the relationship span columns, and which has its Lambda body
                // defined by the rewritten collection expression.
                List<DbVariableReferenceExpression> formals = new List<DbVariableReferenceExpression>(1);
                formals.Add(navInfo.SourceVariable);

                List<DbExpression> args = new List<DbExpression>(1);
                args.Add(navInfo.Source);

                result = DbExpressionBuilder.Invoke(DbExpressionBuilder.Lambda(result, formals), args);
            }

            // Return the (possibly rewritten) collection expression.
            return result;
        }
        
        private void EnterCollection()
        {
            _navSources.Push(null);
        }

        private void EnterNavigationCollection(NavigationInfo info)
        {
            _navSources.Push(info);
        }

        private void ExitCollection()
        {
            _navSources.Pop();
        }

        private bool TryGetNavigationSource(AssociationEndMember wasSourceNowTargetEnd, out DbExpression source)
        {
            source = null;

            NavigationInfo info = null;
            if (_navSources.Count > 0)
            {
                info = _navSources.Peek();
                if (info != null && !object.ReferenceEquals(wasSourceNowTargetEnd, info.SourceEnd))
                {
                    info = null;
                }
            }

            if (info != null)
            {
                source = info.SourceVariable;
                info.InUse = true;
                return true;
            }
            else
            {
                return false;
            }
        }
                      
        /// <summary>
        /// Gathers the applicable { from, to } relationship end pairings for the specified entity type.
        /// Note that it is possible for both { x, y } and { y, x } - where x and y are relationship ends - 
        /// to be returned if the relationship is symmetric (in the sense that it has multiplicity of at
        /// most one in each direction and the type of each end is Ref to the same Entity type, or a supertype).
        /// </summary>
        /// <param name="entityType">The Entity type for which the applicable { from, to } end pairings should be retrieved.</param>
        /// <returns>
        ///     A List of association end members pairings that describes the available { from, to } navigations
        ///     for the specified Entity type that are valid for Relationship Span; or <c>null</c> if no such pairings exist.
        /// </returns>
        private List<KeyValuePair<AssociationEndMember, AssociationEndMember>> GetRelationshipSpanEnds(EntityType entityType)
        {
            // The list to be returned; initially null.
            List<KeyValuePair<AssociationEndMember, AssociationEndMember>> retList = null;

            // If relationship span is not enabled then do not attempt to retrieve the applicable navigations.
            if (_relationshipSpan)
            {
                // Consider all Association types...
                foreach (AssociationType association in _tree.MetadataWorkspace.GetItems<AssociationType>(DataSpace.CSpace))
                {
                    // ... which have exactly two ends
                    if (2 == association.AssociationEndMembers.Count)
                    {
                        AssociationEndMember end0 = association.AssociationEndMembers[0];
                        AssociationEndMember end1 = association.AssociationEndMembers[1];

                        // If end0 -> end1 is valid for relationship span then add { end0, end1 }
                        // to the list of end pairings.
                        if (IsValidRelationshipSpan(entityType, association, end0, end1))
                        {
                            // If the list has not been instantiated, do so now.
                            if (null == retList)
                            {
                                retList = new List<KeyValuePair<AssociationEndMember, AssociationEndMember>>();
                            }

                            retList.Add(new KeyValuePair<AssociationEndMember, AssociationEndMember>(end0, end1));
                        }

                        // Similarly if the inverse navigation is also or instead valid for relationship span
                        // then add the { end1, end0 } pairing to the list of valid end pairings.
                        if (IsValidRelationshipSpan(entityType, association, end1, end0))
                        {
                            // Again, if the list has not been instantiated, do so now.
                            if (null == retList)
                            {
                                retList = new List<KeyValuePair<AssociationEndMember, AssociationEndMember>>();
                            }

                            retList.Add(new KeyValuePair<AssociationEndMember, AssociationEndMember>(end1, end0));
                        }
                    }
                }
            }

            // Return the list (which may still be null at this point)
            return retList;
        }

        /// <summary>
        /// Determines whether the specified { from, to } relationship end pairing represents a navigation that is
        /// valid for a relationship span sourced by an instance of the specified entity type.
        /// </summary>
        /// <param name="compareType">The Entity type which valid 'from' ends must reference (or a supertype of that Entity type)</param>
        /// <param name="associationType">The Association type to consider.</param>
        /// <param name="fromEnd">The candidate 'from' end, which will be checked based on the Entity type it references</param>
        /// <param name="toEnd">The candidate 'to' end, which will be checked base on the upper bound of its multiplicity</param>
        /// <returns>
        ///     <c>True</c> if the end pairing represents a valid navigation from an instance of the specified entity type
        ///     to an association end with a multiplicity upper bound of at most 1; otherwise <c>false</c>
        /// </returns>
        private static bool IsValidRelationshipSpan(EntityType compareType, AssociationType associationType, AssociationEndMember fromEnd, AssociationEndMember toEnd)
        {
            // Only a relationship end with a multiplicity of AT MOST one may be
            // considered as the 'to' end, so that the cardinality of the result
            // of the relationship span has an upper bound of 1. 
            // Therefore ends with RelationshipMultiplicity of EITHER One OR ZeroOrOne
            // are the only ends that should be considered as target ends.
            // Note that a relationship span can be sourced by an Entity that is of the same type
            // as the Entity type referenced by the 'from' end OR any type in the same branch of 
            // the type hierarchy.
            //
            // For example, in the following hierarchy:
            //
            // A  (*<-->?) AOwner
            // |_B  (*<-->1) BOwner
            // |_A1  (*<-->?) A1Owner
            //   |_A2
            //     |_A3_1  (1<-->?) A3_1Owner
            //     |_A3_2  (*<-->1) A3_2Owner
            //
            // An instance of 'A' would need ALL the 'AOwner', 'BOwner', 'A1Owner', 'A3_1Owner' and 'A3_2Owner' ends
            // spanned in because an instance of 'A' could actually be an instance of A, B, A1, A2, A3_1 or A3_2. 
            // An instance of 'B' would only need 'AOwner' and 'BOwner' spanned in.
            // An instance of A2 would need 'AOwner', 'A1Owner', 'A3_1Owner' and 'A3_2Owner' spanned in.
            // An instance of A3_1 would only need 'AOwner', 'A1Owner' and 'A3_1Owner' spanned in.
            //
            // In general, the rule for relationship span is:
            // - 'To' end cardinality AT MOST one
            //   AND
            //   - Referenced Entity type of 'From' end is equal to instance Entity type
            //     OR
            //   - Referenced Entity type of 'From' end is a supertype of instance Entity type
            //     OR
            //   - Referenced Entity type of 'From' end is a subtype of instance Entity type
            //     (this follows from the fact that an instance of 'A' may be an instance of any of its derived types.
            //      Navigation for a subtype relationship will return null if the Entity instance navigation source
            //      is not actually of the required subtype).
            //
            if(!associationType.IsForeignKey &&
               (RelationshipMultiplicity.One == toEnd.RelationshipMultiplicity ||
                RelationshipMultiplicity.ZeroOrOne == toEnd.RelationshipMultiplicity))
            {
                EntityType fromEntityType = (EntityType)((RefType)fromEnd.TypeUsage.EdmType).ElementType;
                return (ObjectSpanRewriter.EntityTypeEquals(compareType, fromEntityType) ||
                        TypeSemantics.IsSubTypeOf(compareType, fromEntityType) ||
                        TypeSemantics.IsSubTypeOf(fromEntityType, compareType));
            }

            return false;
        }

        #region Nested types used for Relationship span over Relationship Navigation optimizations

        private class NavigationInfo
        {
            private readonly DbRelationshipNavigationExpression _original;
            private readonly DbRelationshipNavigationExpression _rewritten;
            private DbVariableReferenceExpression _sourceRef;
            private AssociationEndMember _sourceEnd;
            private DbExpression _source;            

            public NavigationInfo(DbRelationshipNavigationExpression originalNavigation, DbRelationshipNavigationExpression rewrittenNavigation)
            {
                Debug.Assert(originalNavigation != null, "originalNavigation cannot be null");
                Debug.Assert(rewrittenNavigation != null, "rewrittenNavigation cannot be null");

                this._original = originalNavigation;
                this._rewritten = rewrittenNavigation;
                this._sourceEnd = (AssociationEndMember)originalNavigation.NavigateFrom;
                this._sourceRef = (DbVariableReferenceExpression)rewrittenNavigation.NavigationSource;
                this._source = originalNavigation.NavigationSource;
            }

            public bool InUse;

            public AssociationEndMember SourceEnd { get { return _sourceEnd; } }
            public DbExpression Source { get { return _source; } }
            public DbVariableReferenceExpression SourceVariable { get { return _sourceRef; } }
        }

        private class RelationshipNavigationVisitor : DefaultExpressionVisitor
        {
            internal static DbExpression FindNavigationExpression(DbExpression expression, AliasGenerator aliasGenerator, out NavigationInfo navInfo)
            {
                Debug.Assert(TypeSemantics.IsCollectionType(expression.ResultType), "Non-collection input to projection?");

                navInfo = null;

                TypeUsage elementType = ((CollectionType)expression.ResultType.EdmType).TypeUsage;
                if (!TypeSemantics.IsEntityType(elementType) && !TypeSemantics.IsReferenceType(elementType))
                {
                    return expression;
                }

                RelationshipNavigationVisitor visitor = new RelationshipNavigationVisitor(aliasGenerator);
                DbExpression rewrittenExpression = visitor.Find(expression);
                if (!object.ReferenceEquals(expression, rewrittenExpression))
                {
                    Debug.Assert(visitor._original != null && visitor._rewritten != null, "Expression was rewritten but no navigation was found?");
                    navInfo = new NavigationInfo(visitor._original, visitor._rewritten);
                    return rewrittenExpression;
                }
                else
                {
                    return expression;
                }
            }

            private readonly AliasGenerator _aliasGenerator;
            private DbRelationshipNavigationExpression _original;
            private DbRelationshipNavigationExpression _rewritten;

            private RelationshipNavigationVisitor(AliasGenerator aliasGenerator)
            {
                _aliasGenerator = aliasGenerator;
            }

            private DbExpression Find(DbExpression expression)
            {
                return this.VisitExpression(expression);
            }

            protected override DbExpression VisitExpression(DbExpression expression)
            {
                switch (expression.ExpressionKind)
                {
                    case DbExpressionKind.RelationshipNavigation:
                    case DbExpressionKind.Distinct:
                    case DbExpressionKind.Filter:
                    case DbExpressionKind.Limit:
                    case DbExpressionKind.OfType:
                    case DbExpressionKind.Project:
                    case DbExpressionKind.Sort:
                    case DbExpressionKind.Skip:
                        return base.VisitExpression(expression);

                    default:
                        return expression;
                }
            }

            public override DbExpression Visit(DbRelationshipNavigationExpression expression)
            {
                this._original = expression;

                // Ensure a unique variable name when the expression is used in a command tree
                string varName = _aliasGenerator.Next();
                DbVariableReferenceExpression sourceRef = new DbVariableReferenceExpression(expression.NavigationSource.ResultType, varName);

                this._rewritten = sourceRef.Navigate(expression.NavigateFrom, expression.NavigateTo);

                return this._rewritten;
            }

            // For Distinct, Limit, OfType there is no need to override the base visitor behavior.
            
            public override DbExpression Visit(DbFilterExpression expression)
            {
                // Only consider the Filter input
                DbExpression found = Find(expression.Input.Expression);
                if(!object.ReferenceEquals(found, expression.Input.Expression))
                {
                    return found.BindAs(expression.Input.VariableName).Filter(expression.Predicate);
                }
                else
                {
                    return expression;
                }
            }
            
            public override DbExpression Visit(DbProjectExpression expression)
            {
                // Only allowed cases:
                // SELECT Deref(x) FROM <expression> AS x
                // SELECT x FROM <expression> as x
                DbExpression testExpr = expression.Projection;
                if (DbExpressionKind.Deref == testExpr.ExpressionKind)
                {
                    testExpr = ((DbDerefExpression)testExpr).Argument;
                }

                if (DbExpressionKind.VariableReference == testExpr.ExpressionKind)
                {
                    DbVariableReferenceExpression varRef = (DbVariableReferenceExpression)testExpr;
                    if (varRef.VariableName.Equals(expression.Input.VariableName, StringComparison.Ordinal))
                    {
                        DbExpression found = Find(expression.Input.Expression);
                        if (!object.ReferenceEquals(found, expression.Input.Expression))
                        {
                            return found.BindAs(expression.Input.VariableName).Project(expression.Projection);
                        }
                    }
                }

                return expression;
            }

            public override DbExpression Visit(DbSortExpression expression)
            {
                DbExpression found = Find(expression.Input.Expression);
                if(!object.ReferenceEquals(found, expression.Input.Expression))
                {
                    return found.BindAs(expression.Input.VariableName).Sort(expression.SortOrder);
                }
                else
                {
                    return expression;
                }
            }

            public override DbExpression Visit(DbSkipExpression expression)
            {
                DbExpression found = Find(expression.Input.Expression);
                if (!object.ReferenceEquals(found, expression.Input.Expression))
                {
                    return found.BindAs(expression.Input.VariableName).Skip(expression.SortOrder, expression.Count);
                }
                else
                {
                    return expression;
                }
            }
        }
        #endregion
    }
}
