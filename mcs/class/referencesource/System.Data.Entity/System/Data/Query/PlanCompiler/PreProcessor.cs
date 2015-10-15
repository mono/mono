//---------------------------------------------------------------------
// <copyright file="PreProcessor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.

//
// The PreProcessor module is responsible for performing any required preprocessing
// on the tree and gathering information before subsequent phases may be performed.
// The main responsibilites of the preprocessor are:
//    
//   (a) gathering information about all structured types and entitysets referenced in the
//       query
//   (b) expanding views, navigate operators, and rewritting other type related operators
//   (c) gathering information about which subsequent phases may be requried
//   (d) pulling sort over project, and removing unnecessary sorts
//   (e) eliminates certain operations by translating them into equivalent subtrees 
//              (ElementOp, NewMultisetOp)
//

namespace System.Data.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Data.Query.InternalTrees;

    internal class DiscriminatorMapInfo
    {
        internal EntityTypeBase RootEntityType;
        internal bool IncludesSubTypes;
        internal ExplicitDiscriminatorMap DiscriminatorMap;

        internal DiscriminatorMapInfo(EntityTypeBase rootEntityType, bool includesSubTypes, ExplicitDiscriminatorMap discriminatorMap)
        {
            RootEntityType = rootEntityType;
            IncludesSubTypes = includesSubTypes;
            DiscriminatorMap = discriminatorMap;
        }

        /// <summary>
        /// Merge the discriminatorMap info we just found with what we've already found.
        /// 
        /// In practice, if either the current or the new map is from an OfTypeOnly view, we
        /// have to avoid the optimizations.
        /// 
        /// If we have a new map that is a superset of the current map, then we can just swap
        /// the new map for the current one.
        /// 
        /// If the current map is tha super set of the new one ther's nothing to do.
        /// 
        /// (Of course, if neither has changed, then we really don't need to look)
        /// </summary>
        internal void Merge(EntityTypeBase neededRootEntityType, bool includesSubtypes, ExplicitDiscriminatorMap discriminatorMap)
        {
            // If what we've found doesn't exactly match what we are looking for we have more work to do
            if (RootEntityType != neededRootEntityType || IncludesSubTypes != includesSubtypes)
            {
                if (!IncludesSubTypes || !includesSubtypes)
                {
                    // If either the original or the new map is from an of-type-only view we can't
                    // merge, we just have to not optimize this case.
                    DiscriminatorMap = null;

                }
                if (TypeSemantics.IsSubTypeOf(RootEntityType, neededRootEntityType))
                {
                    // we're asking for a super type of existing type, and what we had is a proper 
                    // subset of it -we can replace the existing item.
                    RootEntityType = neededRootEntityType;
                    DiscriminatorMap = discriminatorMap;
                }
                if (!TypeSemantics.IsSubTypeOf(neededRootEntityType, RootEntityType))
                {
                    // If either the original or the new map is from an of-type-only view we can't
                    // merge, we just have to not optimize this case.
                    DiscriminatorMap = null;
                }
            }
        }
    }

    /// <summary>
    /// The PreProcessor module is responsible for performing any required preprocessing
    /// on the tree and gathering information before subsequent phases may be performed.
    /// </summary>
    internal class PreProcessor : SubqueryTrackingVisitor
    {
        #region private state
        /// <summary>
        /// Tracks affinity of entity constructors to entity sets (aka scoped entity type constructors).
        /// Scan view ops and entityset-bound tvfs push corresponding entity sets so that their child nodes representing entity constructors could
        /// determine the entity set to which the constructed entity belongs.
        /// </summary>
        private readonly Stack<EntitySet> m_entityTypeScopes = new Stack<EntitySet>();

        // Track referenced types, entitysets, entitycontainers, free floating entity constructor types 
        // and types needing a null sentinel.
        private readonly HashSet<EntityContainer> m_referencedEntityContainers = new HashSet<EntityContainer>();
        private readonly HashSet<EntitySet> m_referencedEntitySets = new HashSet<EntitySet>();
        private readonly HashSet<TypeUsage> m_referencedTypes = new HashSet<TypeUsage>();
        private readonly HashSet<EntityType> m_freeFloatingEntityConstructorTypes = new HashSet<EntityType>();
        private readonly HashSet<string> m_typesNeedingNullSentinel = new HashSet<string>();
        private readonly Dictionary<EdmFunction, EdmProperty[]> m_tvfResultKeys = new Dictionary<EdmFunction, EdmProperty[]>();

        /// <summary>
        /// Helper for rel properties
        /// </summary>
        private RelPropertyHelper m_relPropertyHelper;

        // Track discriminator metadata.
        private bool m_suppressDiscriminatorMaps;
        private readonly Dictionary<EntitySetBase, DiscriminatorMapInfo> m_discriminatorMaps = new Dictionary<EntitySetBase, DiscriminatorMapInfo>();
        #endregion

        #region constructors
        private PreProcessor(PlanCompiler planCompilerState)
            : base(planCompilerState)
        {
            m_relPropertyHelper = new RelPropertyHelper(m_command.MetadataWorkspace, m_command.ReferencedRelProperties);
        }
        #endregion

        #region public methods
        /// <summary>
        /// The driver routine.
        /// </summary>
        /// <param name="planCompilerState">plan compiler state</param>
        /// <param name="typeInfo">type information about all types/sets referenced in the query</param>
        /// <param name="tvfResultKeys">inferred key columns of tvfs return types</param>
        internal static void Process(
            PlanCompiler planCompilerState,
            out StructuredTypeInfo typeInfo,
            out Dictionary<EdmFunction, EdmProperty[]> tvfResultKeys)
        {
            PreProcessor preProcessor = new PreProcessor(planCompilerState);
            preProcessor.Process(out tvfResultKeys);

            StructuredTypeInfo.Process(planCompilerState.Command,
                preProcessor.m_referencedTypes,
                preProcessor.m_referencedEntitySets,
                preProcessor.m_freeFloatingEntityConstructorTypes,
                preProcessor.m_suppressDiscriminatorMaps ? null : preProcessor.m_discriminatorMaps,
                preProcessor.m_relPropertyHelper,
                preProcessor.m_typesNeedingNullSentinel,
                out typeInfo);
        }

        #endregion

        #region private methods

        #region driver
        internal void Process(out Dictionary<EdmFunction, EdmProperty[]> tvfResultKeys)
        {
            m_command.Root = VisitNode(m_command.Root);
            //
            // Add any Vars that are of structured type - if the Vars aren't
            // referenced via a VarRefOp, we end up losing them...
            //
            foreach (Var v in m_command.Vars)
            {
                AddTypeReference(v.Type);
            }

            //
            // If we have any "structured" types, then we need to run through NTE
            //
            if (m_referencedTypes.Count > 0)
            {
                m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.NTE);

                //
                // Find any structured types that are projected at the top level, and
                // ensure that we can handle their nullability.
                //
                PhysicalProjectOp ppOp = (PhysicalProjectOp)m_command.Root.Op; // this better be the case or we have other problems.
                ppOp.ColumnMap.Accept(StructuredTypeNullabilityAnalyzer.Instance, m_typesNeedingNullSentinel);
            }

            tvfResultKeys = m_tvfResultKeys;
        }
        #endregion

        #region private state maintenance - type and set information

        /// <summary>
        /// Mark this EntitySet as referenced in the query
        /// </summary>
        /// <param name="entitySet"></param>
        private void AddEntitySetReference(EntitySet entitySet)
        {
            m_referencedEntitySets.Add(entitySet);
            if (!m_referencedEntityContainers.Contains(entitySet.EntityContainer))
            {
                m_referencedEntityContainers.Add(entitySet.EntityContainer);
            }
        }

        /// <summary>
        /// Mark this type as being referenced in the query, if it is a structured, collection or enum type.
        /// </summary>
        /// <param name="type">type to reference</param>
        private void AddTypeReference(TypeUsage type)
        {
            if (TypeUtils.IsStructuredType(type) || TypeUtils.IsCollectionType(type) || TypeUtils.IsEnumerationType(type))
            {
                m_referencedTypes.Add(type);
            }
        }

        /// <summary>
        /// Get the list of relationshipsets that can hold instances of the given relationshiptype
        /// 
        /// We identify the list of relationshipsets in the current list of entitycontainers that are 
        /// of the given type. Since we don't yet support relationshiptype subtyping, this is a little
        /// easier than the entity version
        /// </summary>
        /// <param name="relType">the relationship type to look for</param>
        /// <returns>the list of relevant relationshipsets</returns>
        private List<RelationshipSet> GetRelationshipSets(RelationshipType relType)
        {
            List<RelationshipSet> relSets = new List<RelationshipSet>();
            foreach (EntityContainer entityContainer in m_referencedEntityContainers)
            {
                foreach (EntitySetBase set in entityContainer.BaseEntitySets)
                {
                    RelationshipSet relSet = set as RelationshipSet;
                    if (relSet != null &&
                        relSet.ElementType.Equals(relType))
                    {
                        relSets.Add(relSet);
                    }
                }
            }
            return relSets;
        }

        /// <summary>
        /// Find all entitysets (that are reachable in the current query) that can hold instances that 
        /// are *at least* of type "entityType".
        /// An entityset ES of type T1 can hold instances that are at least of type T2, if one of the following
        /// is true
        ///   - T1 is a subtype of T2
        ///   - T2 is a subtype of T1
        ///   - T1 is equal to T2
        /// </summary>
        /// <param name="entityType">the desired entity type</param>
        /// <returns>list of all entitysets of the desired shape</returns>
        private List<EntitySet> GetEntitySets(TypeUsage entityType)
        {
            List<EntitySet> sets = new List<EntitySet>();
            foreach (EntityContainer container in m_referencedEntityContainers)
            {
                foreach (EntitySetBase baseSet in container.BaseEntitySets)
                {
                    EntitySet set = baseSet as EntitySet;
                    if (set != null &&
                        (set.ElementType.Equals(entityType.EdmType) ||
                         TypeSemantics.IsSubTypeOf(entityType.EdmType, set.ElementType) ||
                         TypeSemantics.IsSubTypeOf(set.ElementType, entityType.EdmType)))
                    {
                        sets.Add(set);
                    }
                }
            }

            return sets;
        }

        #endregion

        #region View Expansion
        /// <summary>
        /// Gets the "expanded" query mapping view for the specified C-Space entity set
        /// </summary>
        /// <param name="node">The current node</param>
        /// <param name="scanTableOp">The scanTableOp that references the entity set</param>
        /// <param name="typeFilter">
        ///     An optional type filter to apply to the generated view. 
        ///     Set to <c>null</c> on return if the generated view renders the type filter superfluous.
        /// </param>
        /// <returns>A node that is the root of the new expanded view</returns>
        private Node ExpandView(Node node, ScanTableOp scanTableOp, ref IsOfOp typeFilter)
        {
            EntitySetBase entitySet = scanTableOp.Table.TableMetadata.Extent;
            PlanCompiler.Assert(entitySet != null, "The target of a ScanTableOp must reference an EntitySet to be used with ExpandView");
            PlanCompiler.Assert(entitySet.EntityContainer.DataSpace == DataSpace.CSpace, "Store entity sets cannot have Query Mapping Views and should not be used with ExpandView");

            if (typeFilter != null &&
               !typeFilter.IsOfOnly &&
                TypeSemantics.IsSubTypeOf(entitySet.ElementType, typeFilter.IsOfType.EdmType))
            {
                //
                // If a type filter is being applied to the ScanTableOp, but that filter is asking
                // for all elements that are the same type or a supertype of the element type of the
                // target entity set, then the type filter is a no-op and can safely be discarded -
                // IF AND ONLY IF the type filter is 'OfType' - which includes subtypes - and NOT
                // 'IsOfOnly' - which requires an exact type match, and so does not include subtypes.
                //
                typeFilter = null;
            }

            //
            // Call the GetGeneratedView method to retrieve the query mapping view for the extent referenced
            // by the ScanTableOp. The actual method used to do this differs depending on whether the default
            // Query Mapping View is sufficient or a targeted view that only filters by element type is required.
            //
            System.Data.Mapping.ViewGeneration.GeneratedView definingQuery = null;
            EntityTypeBase requiredType = scanTableOp.Table.TableMetadata.Extent.ElementType;
            bool includeSubtypes = true;
            if (typeFilter != null)
            {
                // 
                // A type filter is being applied to the ScanTableOp; it may be possible to produce
                // an optimized expansion of the view based on type-specific views generated for the
                // C-Space entity set. 
                // The type for which the view should be tuned is the 'OfType' specified on the type filter.
                // If the type filter is an 'IsOfOnly' filter then the view should NOT include subtypes of the required type.
                //
                requiredType = (EntityTypeBase)typeFilter.IsOfType.EdmType;
                includeSubtypes = !typeFilter.IsOfOnly;
                if (m_command.MetadataWorkspace.TryGetGeneratedViewOfType(entitySet, requiredType, includeSubtypes, out definingQuery))
                {
                    //
                    // At this point a type-specific view was found that satisifies the type filter's
                    // constraints in terms of required type and whether subtypes should be included;
                    // the type filter itself is now unnecessary and should be set to null indicating
                    // that it can be safely removed (see ProcessScanTableOp and Visit(FilterOp) for this).
                    //
                    typeFilter = null;
                }
            }

            //
            // If a generated view has not been obtained at this point then either:
            // - A type filter was specified but no type-specific view exists that satisfies its constraints.
            //   OR
            // - No type filter was specified.
            // In either case the default query mapping view for the referenced entity set should now be retrieved.
            //
            if (null == definingQuery)
            {
                definingQuery = m_command.MetadataWorkspace.GetGeneratedView(entitySet);
            }

            //
            // If even the default query mapping view was not found then we cannot continue.
            // This implies that the set was not mapped, which should not be allowed, therefore
            // a retail assert is used here instead of a regular exception.
            //
            PlanCompiler.Assert(definingQuery != null, Strings.ADP_NoQueryMappingView(entitySet.EntityContainer.Name, entitySet.Name));

            //
            // At this point we're guaranteed to have found a defining query for the view.
            // We're now going to convert this into an IQT, and then copy it into our own IQT.
            //
            Node ret = definingQuery.GetInternalTree(m_command);

            //
            // Make sure we're tracking what we've asked any discriminator maps to contain.
            //
            DetermineDiscriminatorMapUsage(ret, entitySet, requiredType, includeSubtypes);

            //
            // Build up a ScanViewOp to "cap" the defining query below
            //
            ScanViewOp scanViewOp = m_command.CreateScanViewOp(scanTableOp.Table);
            ret = m_command.CreateNode(scanViewOp, ret);

            return ret;
        }


        /// <summary>
        /// If the discrminator map we're already tracking for this type (in this entityset)
        /// isn't already rooted at our required type, then we have to suppress the use of 
        /// the descriminator maps when we constrct the structuredtypes; see SQLBUDT #615744
        /// </summary>
        private void DetermineDiscriminatorMapUsage(Node viewNode, EntitySetBase entitySet, EntityTypeBase rootEntityType, bool includeSubtypes)
        {
            ExplicitDiscriminatorMap discriminatorMap = null;

            // we expect the view to be capped with a project; we're just being careful here.
            if (viewNode.Op.OpType == OpType.Project)
            {
                DiscriminatedNewEntityOp discriminatedNewEntityOp = viewNode.Child1.Child0.Child0.Op as DiscriminatedNewEntityOp;

                if (null != discriminatedNewEntityOp)
                {
                    discriminatorMap = discriminatedNewEntityOp.DiscriminatorMap;
                }
            }

            DiscriminatorMapInfo discriminatorMapInfo;
            if (!m_discriminatorMaps.TryGetValue(entitySet, out discriminatorMapInfo))
            {
                if (null == rootEntityType)
                {
                    rootEntityType = entitySet.ElementType;
                    includeSubtypes = true;
                }
                discriminatorMapInfo = new DiscriminatorMapInfo(rootEntityType, includeSubtypes, discriminatorMap);
                m_discriminatorMaps.Add(entitySet, discriminatorMapInfo);
            }
            else
            {
                discriminatorMapInfo.Merge(rootEntityType, includeSubtypes, discriminatorMap);
            }
        }

        #endregion

        #region NavigateOp rewrites
        /// <summary>
        /// Rewrites a NavigateOp tree in the following fashion
        ///   SELECT VALUE r.ToEnd
        ///   FROM (SELECT VALUE r1 FROM RS1 as r1
        ///         UNION ALL
        ///         SELECT VALUE r2 FROM RS2 as r2
        ///         ...
        ///         SELECT VALUE rN FROM RSN as rN) as r
        ///   WHERE r.FromEnd = sourceRef
        ///   
        ///  RS1, RS2 etc. are the set of all relationshipsets that can hold instances of the specified
        ///  relationship type. "sourceRef" is the single (ref-type) argument to the NavigateOp that 
        ///  represents the from-end of the navigation traversal
        /// If the toEnd is multi-valued, then we stick a Collect(PhysicalProject( over the subquery above
        /// 
        /// A couple of special cases. 
        ///    If no relationship sets can be found, we return a NULL (if the 
        /// toEnd is single-valued), or an empty multiset (if the toEnd is multi-valued)
        ///
        ///    If the toEnd is single-valued, *AND* the input Op is a GetEntityRefOp, then 
        /// we convert the NavigateOp into a RelPropertyOp over the entity.
        /// </summary>
        /// <param name="navigateOpNode">the navigateOp tree</param>
        /// <param name="navigateOp">the navigateOp</param>
        /// <param name="outputVar">the output var produced by the subquery (ONLY if the to-End is single-valued)</param>
        /// <returns>the resulting node</returns>
        private Node RewriteNavigateOp(Node navigateOpNode, NavigateOp navigateOp, out Var outputVar)
        {
            outputVar = null;

            //
            // Currently, navigation of composition relationships is not supported.
            //
            if (!Helper.IsAssociationType(navigateOp.Relationship))
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.Cqt_RelNav_NoCompositions);
            }

            //
            // If the input to the navigateOp is a GetEntityRefOp, and the navigation
            // is to the 1-end of the relationship, convert this into a RelPropertyOp instead - operating on the
            // input child to the GetEntityRefOp
            //
            if (navigateOpNode.Child0.Op.OpType == OpType.GetEntityRef &&
                (navigateOp.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne ||
                navigateOp.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.One))
            {
                PlanCompiler.Assert(m_command.IsRelPropertyReferenced(navigateOp.RelProperty),
                    "Unreferenced rel property? " + navigateOp.RelProperty);
                Op relPropertyOp = m_command.CreateRelPropertyOp(navigateOp.RelProperty);
                Node relPropertyNode = m_command.CreateNode(relPropertyOp,
                    navigateOpNode.Child0.Child0);
                return relPropertyNode;
            }

            List<RelationshipSet> relationshipSets = GetRelationshipSets(navigateOp.Relationship);

            //
            // Special case: when no relationshipsets can be found. Return NULL or an empty multiset,
            //   depending on the multiplicity of the toEnd
            //
            if (relationshipSets.Count == 0)
            {
                // 
                // If we're navigating to the 1-end of the relationship, then simply return a null constant
                //
                if (navigateOp.ToEnd.RelationshipMultiplicity != RelationshipMultiplicity.Many)
                {
                    return m_command.CreateNode(m_command.CreateNullOp(navigateOp.Type));
                }
                else // return an empty set
                {
                    return m_command.CreateNode(m_command.CreateNewMultisetOp(navigateOp.Type));
                }
            }

            //
            // Build up a UNION-ALL ladder over all the relationshipsets
            // 
            List<Node> scanTableNodes = new List<Node>();
            List<Var> scanTableVars = new List<Var>();
            foreach (RelationshipSet relSet in relationshipSets)
            {
                TableMD tableMD = Command.CreateTableDefinition(relSet);
                ScanTableOp tableOp = m_command.CreateScanTableOp(tableMD);
                Node branchNode = m_command.CreateNode(tableOp);
                Var branchVar = tableOp.Table.Columns[0];
                scanTableVars.Add(branchVar);
                scanTableNodes.Add(branchNode);
            }

            Node unionAllNode = null;
            Var unionAllVar;
            m_command.BuildUnionAllLadder(scanTableNodes, scanTableVars, out unionAllNode, out unionAllVar);

            //
            // Now build up the predicate
            //
            Node targetEnd = m_command.CreateNode(m_command.CreatePropertyOp(navigateOp.ToEnd),
                m_command.CreateNode(m_command.CreateVarRefOp(unionAllVar)));
            Node sourceEnd = m_command.CreateNode(m_command.CreatePropertyOp(navigateOp.FromEnd),
                m_command.CreateNode(m_command.CreateVarRefOp(unionAllVar)));
            Node predicateNode = m_command.BuildComparison(OpType.EQ, navigateOpNode.Child0, sourceEnd);
            Node filterNode = m_command.CreateNode(m_command.CreateFilterOp(),
                unionAllNode, predicateNode);
            Var projectVar;
            Node projectNode = m_command.BuildProject(filterNode, targetEnd, out projectVar);

            //
            // Finally, some magic about single-valued vs collection-valued ends
            //
            Node ret;
            if (navigateOp.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                ret = m_command.BuildCollect(projectNode, projectVar);
            }
            else
            {
                ret = projectNode;
                outputVar = projectVar;
            }

            return ret;
        }
        #endregion

        #region DerefOp Rewrites
        /// <summary>
        /// Build up a node tree that represents the set of instances from the given table that are at least
        /// of the specified type ("ofType"). If "ofType" is NULL, then all rows are returned
        /// 
        /// Return the outputVar from the nodetree
        /// </summary>
        /// <param name="entitySet">the entityset or relationshipset to scan over</param>
        /// <param name="ofType">the element types we're interested in</param>
        /// <param name="resultVar">the output var produced by this node tree</param>
        /// <returns>the node tree</returns>
        private Node BuildOfTypeTable(EntitySetBase entitySet, TypeUsage ofType, out Var resultVar)
        {
            TableMD tableMetadata = Command.CreateTableDefinition(entitySet);
            ScanTableOp tableOp = m_command.CreateScanTableOp(tableMetadata);
            Node tableNode = m_command.CreateNode(tableOp);
            Var tableVar = tableOp.Table.Columns[0];

            Node resultNode;
            // 
            // Build a logical "oftype" expression - simply a filter predicate
            //
            if ((ofType != null) && !entitySet.ElementType.EdmEquals(ofType.EdmType))
            {
                m_command.BuildOfTypeTree(tableNode, tableVar, ofType, true, out resultNode, out resultVar);
            }
            else
            {
                resultNode = tableNode;
                resultVar = tableVar;
            }

            return resultNode;
        }

        /// <summary>
        /// Produces a relop tree that "logically" produces the target of the derefop. In essence, this gets rewritten
        /// into 
        ///      SELECT VALUE e
        ///      FROM (SELECT VALUE e0 FROM OFTYPE(ES0, T) as e0
        ///            UNION ALL
        ///            SELECT VALUE e1 FROM OFTYPE(ES1, T) as e1
        ///            ...
        ///            SELECT VALUE eN from OFTYPE(ESN, T) as eN)) as e
        ///      WHERE REF(e) = myRef
        ///      
        /// "T" is the target type of the Deref, and myRef is the (single) argument to the DerefOp
        /// 
        /// ES0, ES1 etc. are all the EntitySets that could hold instances that are at least of type "T". We identify this list of sets 
        /// by looking at all entitycontainers referenced in the query, and looking at all entitysets in those
        /// containers that are of the right type
        /// An EntitySet ES (of entity type X) can hold instances of T, if one of the following is true
        ///   - T is a subtype of X 
        ///   - X is equal to T
        /// Our situation is a little trickier, since we also need to look for cases where X is a subtype of T. 
        /// </summary>
        /// <param name="derefOpNode">the derefOp subtree</param>
        /// <param name="derefOp">the derefOp</param>
        /// <param name="outputVar">output var produced</param>
        /// <returns>the subquery described above</returns>
        private Node RewriteDerefOp(Node derefOpNode, DerefOp derefOp, out Var outputVar)
        {
            TypeUsage entityType = derefOp.Type;
            List<EntitySet> targetEntitySets = GetEntitySets(entityType);
            if (targetEntitySets.Count == 0)
            {
                // We didn't find any entityset that could match this. Simply return a null-value
                outputVar = null;
                return m_command.CreateNode(m_command.CreateNullOp(entityType));
            }

            List<Node> scanTableNodes = new List<Node>();
            List<Var> scanTableVars = new List<Var>();
            foreach (EntitySet entitySet in targetEntitySets)
            {
                Var tableVar;
                Node tableNode = BuildOfTypeTable(entitySet, entityType, out tableVar);

                scanTableNodes.Add(tableNode);
                scanTableVars.Add(tableVar);
            }
            Node unionAllNode;
            Var unionAllVar;
            m_command.BuildUnionAllLadder(scanTableNodes, scanTableVars, out unionAllNode, out unionAllVar);

            //
            // Finally build up the key comparison predicate
            //
            Node entityRefNode = m_command.CreateNode(
                m_command.CreateGetEntityRefOp(derefOpNode.Child0.Op.Type),
                m_command.CreateNode(m_command.CreateVarRefOp(unionAllVar)));
            Node keyComparisonPred = m_command.BuildComparison(OpType.EQ, derefOpNode.Child0, entityRefNode);
            Node filterNode = m_command.CreateNode(
                m_command.CreateFilterOp(),
                unionAllNode,
                keyComparisonPred);

            outputVar = unionAllVar;
            return filterNode;
        }
        #endregion

        #region NavigationProperty Rewrites

        /// <summary>
        /// Find the entityset that corresponds to the specified end of the relationship.
        /// 
        /// We must find one - else we assert.
        /// </summary>
        /// <param name="relationshipSet">the relationshipset</param>
        /// <param name="targetEnd">the destination end of the relationship traversal</param>
        /// <returns>the entityset corresponding to the target end</returns>
        private static EntitySetBase FindTargetEntitySet(RelationshipSet relationshipSet, RelationshipEndMember targetEnd)
        {
            EntitySetBase entitySet = null;

            AssociationSet associationSet = (AssociationSet)relationshipSet;
            // find the corresponding entityset
            entitySet = null;
            foreach (AssociationSetEnd e in associationSet.AssociationSetEnds)
            {
                if (e.CorrespondingAssociationEndMember.EdmEquals(targetEnd))
                {
                    entitySet = e.EntitySet;
                    break;
                }
            }
            PlanCompiler.Assert(entitySet != null, "Could not find entityset for relationshipset " + relationshipSet + ";association end " + targetEnd);
            return entitySet;
        }


        /// <summary>
        /// Builds up a join between the relationshipset and the entityset corresponding to its toEnd. In essence,
        /// we produce
        ///    SELECT r, e
        ///    FROM RS as r, OFTYPE(ES, T) as e
        ///    WHERE r.ToEnd = Ref(e)
        ///    
        /// "T" is the entity type of the toEnd of the relationship.  
        /// </summary>
        /// <param name="relSet">the relationshipset</param>
        /// <param name="end">the toEnd of the relationship</param>
        /// <param name="rsVar">the var representing the relationship instance ("r") in the output subquery</param>
        /// <param name="esVar">the var representing the entity instance ("e") in the output subquery</param>
        /// <returns>the join subquery described above</returns>
        private Node BuildJoinForNavProperty(RelationshipSet relSet, RelationshipEndMember end,
            out Var rsVar, out Var esVar)
        {
            EntitySetBase entitySet = FindTargetEntitySet(relSet, end);

            //
            // Build out the ScanTable ops for the relationshipset and the entityset. Add the 
            //
            Node asTableNode = BuildOfTypeTable(relSet, null, out rsVar);
            Node esTableNode = BuildOfTypeTable(entitySet, TypeHelpers.GetElementTypeUsage(end.TypeUsage), out esVar);

            // 
            // Build up a join between the entityset and the associationset; join on the to-end
            //
            Node joinPredicate = m_command.BuildComparison(OpType.EQ,
                m_command.CreateNode(m_command.CreateGetEntityRefOp(end.TypeUsage), m_command.CreateNode(m_command.CreateVarRefOp(esVar))),
                m_command.CreateNode(m_command.CreatePropertyOp(end), m_command.CreateNode(m_command.CreateVarRefOp(rsVar)))
                );

            Node joinNode = m_command.CreateNode(m_command.CreateInnerJoinOp(),
                asTableNode, esTableNode, joinPredicate);

            return joinNode;
        }

        /// <summary>
        /// Rewrite a navigation property when the target end has multiplicity
        /// of one (or zero..one) and the source end has multiplicity of many.
        /// 
        /// Note that this translation is also valid for a navigation property when the target 
        /// end has multiplicity of one (or zero..one) and the source end has multiplicity of one
        /// (or zero..one), but a different translation is used because it yields a simpler query in some cases.
        /// 
        /// We simply pick up the corresponding rel property from the input entity, and 
        /// apply a deref operation
        ///     NavProperty(e, n) => deref(relproperty(e, r))
        /// where e is the entity expression, n is the nav-property, and r is the corresponding
        /// rel-property
        /// </summary>
        /// <param name="relProperty">the rel-property describing the navigation</param>
        /// <param name="sourceEntityNode">entity instance that we're starting the traversal from</param>
        /// <param name="resultType">type of the target entity</param>
        /// <returns>a rewritten subtree</returns>
        private Node RewriteManyToOneNavigationProperty(RelProperty relProperty,
            Node sourceEntityNode, TypeUsage resultType)
        {
            RelPropertyOp relPropertyOp = m_command.CreateRelPropertyOp(relProperty);
            Node relPropertyNode = m_command.CreateNode(relPropertyOp, sourceEntityNode);
            DerefOp derefOp = m_command.CreateDerefOp(resultType);
            Node derefNode = m_command.CreateNode(derefOp, relPropertyNode);

            return derefNode;
        }

        /// <summary>
        /// Rewrite a navigation property when the source end has multiplicity
        /// of one (or zero..one) and the target end has multiplicity of many.
        /// 
        /// <see cref="RewriteFromOneNavigationProperty"/>
        /// We also build out a CollectOp over the subquery above, and return that
        /// </summary>
        /// <param name="relProperty">the rel-property describing the relationship traversal</param>
        /// <param name="relationshipSets">the list of relevant relationshipsets</param>
        /// <param name="sourceRefNode">node tree corresponding to the source entity ref</param>
        /// <returns>the rewritten subtree</returns>
        private Node RewriteOneToManyNavigationProperty(RelProperty relProperty,
            List<RelationshipSet> relationshipSets,
            Node sourceRefNode)
        {
            Var outputVar;
            Node ret = RewriteFromOneNavigationProperty(relProperty, relationshipSets, sourceRefNode, out outputVar);

            // The return value is a collection, but used as a property, thus it needs to be capped with a collect
            ret = m_command.BuildCollect(ret, outputVar);

            return ret;
        }

        /// <summary>
        /// Rewrite a navigation property when the target end has multiplicity
        /// of one (or zero..one) and the source end has multiplicity of one (or zero..one).
        /// 
        /// <see cref="RewriteFromOneNavigationProperty"/>
        /// We add the translation as a subquery to the parent rel op and return a reference to
        /// the corresponding var
        /// </summary>
        /// <param name="relProperty">the rel-property describing the relationship traversal</param>
        /// <param name="relationshipSets">the list of relevant relationshipsets</param>
        /// <param name="sourceRefNode">node tree corresponding to the source entity ref</param>
        /// <returns>the rewritten subtree</returns>
        private Node RewriteOneToOneNavigationProperty(RelProperty relProperty,
            List<RelationshipSet> relationshipSets,
            Node sourceRefNode)
        {
            Var outputVar;
            Node ret = RewriteFromOneNavigationProperty(relProperty, relationshipSets, sourceRefNode, out outputVar);

            ret = VisitNode(ret);
            ret = AddSubqueryToParentRelOp(outputVar, ret); 

            return ret;
        }

        /// <summary>
        /// Translation for Navigation Properties with a 0 or 0..1 source end
        /// In essence, we find all the relevant target entitysets, and then compare the
        /// rel-property on the target end with the source ref
        /// 
        /// Converts
        ///   NavigationProperty(e, r)
        /// into 
        ///   SELECT VALUE t
        ///   FROM (SELECT VALUE e1 FROM ES1 as e1
        ///         UNION ALL 
        ///         SELECT VALUE e2 FROM ES2 as e2
        ///         UNION ALL 
        ///         ...
        ///         ) as t
        ///   WHERE RelProperty(t, r') = GetEntityRef(e)
        ///   
        /// r' is the inverse-relproperty for r
        /// </summary>
        /// <param name="relProperty">the rel-property describing the relationship traversal</param>
        /// <param name="relationshipSets">the list of relevant relationshipsets</param>
        /// <param name="sourceRefNode">node tree corresponding to the source entity ref</param>
        /// <param name="outputVar">the var representing the output</param>
        /// <returns>the rewritten subtree</returns>
        private Node RewriteFromOneNavigationProperty(RelProperty relProperty, List<RelationshipSet> relationshipSets, Node sourceRefNode,  out Var outputVar)
        {
            PlanCompiler.Assert(relationshipSets.Count > 0, "expected at least one relationshipset here");
            PlanCompiler.Assert(relProperty.FromEnd.RelationshipMultiplicity != RelationshipMultiplicity.Many,
                "Expected source end multiplicity to be one. Found 'Many' instead " + relProperty);
 
            TypeUsage entityType = TypeHelpers.GetElementTypeUsage(relProperty.ToEnd.TypeUsage);
            List<Node> scanTableNodes = new List<Node>(relationshipSets.Count);
            List<Var> scanTableVars = new List<Var>(relationshipSets.Count);
            foreach (RelationshipSet r in relationshipSets)
            {
                EntitySetBase entitySet = FindTargetEntitySet(r, relProperty.ToEnd);
                Var tableVar;
                Node tableNode = BuildOfTypeTable(entitySet, entityType, out tableVar);

                scanTableNodes.Add(tableNode);
                scanTableVars.Add(tableVar);
            }

            // 
            // Build the union-all node
            //
            Node unionAllNode;

            m_command.BuildUnionAllLadder(scanTableNodes, scanTableVars, out unionAllNode, out outputVar);

            //
            // Now build up the appropriate filter. Select out the relproperty from the other end
            //
            RelProperty inverseRelProperty = new RelProperty(relProperty.Relationship, relProperty.ToEnd, relProperty.FromEnd);
            PlanCompiler.Assert(m_command.IsRelPropertyReferenced(inverseRelProperty),
                "Unreferenced rel property? " + inverseRelProperty);
            Node inverseRelPropertyNode = m_command.CreateNode(
                m_command.CreateRelPropertyOp(inverseRelProperty),
                m_command.CreateNode(m_command.CreateVarRefOp(outputVar)));
            Node predicateNode = m_command.BuildComparison(OpType.EQ,
                sourceRefNode, inverseRelPropertyNode);
            Node ret = m_command.CreateNode(m_command.CreateFilterOp(), unionAllNode, predicateNode);

            return ret;
        }

        /// <summary>
        /// Rewrite a navigation property when the target end has multiplicity
        /// many and the source end has multiplicity of many.
        /// 
        /// Consider this a rewrite of DEREF(NAVIGATE(r)) where "r" is a many-to-many relationship
        /// 
        /// We essentially produce the following subquery
        ///   SELECT VALUE x.e
        ///   FROM (SELECT r1 as r, e1 as e FROM RS1 as r1 INNER JOIN OFTYPE(ES1, T) as e1 on r1.ToEnd = Ref(e1)
        ///         UNION ALL
        ///         SELECT r1 as r, e1 as e FROM RS1 as r1 INNER JOIN OFTYPE(ES1, T) as e1 on r1.ToEnd = Ref(e1)
        ///         ...
        ///         ) as x 
        ///   WHERE x.r.FromEnd = sourceRef
        ///   
        /// RS1, RS2 etc. are the relevant relationshipsets
        /// ES1, ES2 etc. are the corresponding entitysets for the toEnd of the relationship
        /// sourceRef is the ref argument
        /// T is the type of the target-end of the relationship
        /// 
        /// We then build a CollectOp over the subquery above
        /// </summary>
        /// <param name="relProperty">the rel property to traverse</param>
        /// <param name="relationshipSets">list of relevant relationshipsets</param>
        /// <param name="sourceRefNode">source ref</param>
        /// <returns></returns>
        private Node RewriteManyToManyNavigationProperty(RelProperty relProperty,
            List<RelationshipSet> relationshipSets,
            Node sourceRefNode)
        {
            PlanCompiler.Assert(relationshipSets.Count > 0, "expected at least one relationshipset here");
            PlanCompiler.Assert(relProperty.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many &&
                relProperty.FromEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many,
                "Expected target end multiplicity to be 'many'. Found " + relProperty + "; multiplicity = " + relProperty.ToEnd.RelationshipMultiplicity);

            Node ret = null;

            List<Node> joinNodes = new List<Node>(relationshipSets.Count);
            List<Var> outputVars = new List<Var>(relationshipSets.Count * 2);
            foreach (RelationshipSet r in relationshipSets)
            {
                Var rsVar;
                Var esVar;
                Node joinNode = BuildJoinForNavProperty(r, relProperty.ToEnd, out rsVar, out esVar);
                joinNodes.Add(joinNode);
                outputVars.Add(rsVar);
                outputVars.Add(esVar);
            }

            // 
            // Build the union-all node
            //
            Node unionAllNode;
            IList<Var> unionAllVars;
            m_command.BuildUnionAllLadder(joinNodes, outputVars, out unionAllNode, out unionAllVars);

            //
            // Now build out the filterOp over the left-side var
            //
            Node rsSourceRefNode = m_command.CreateNode(m_command.CreatePropertyOp(relProperty.FromEnd),
                m_command.CreateNode(m_command.CreateVarRefOp(unionAllVars[0])));
            Node predicate = m_command.BuildComparison(OpType.EQ,
                sourceRefNode, rsSourceRefNode);
            Node filterNode = m_command.CreateNode(m_command.CreateFilterOp(),
                unionAllNode, predicate);

            //
            // Finally, build out a project node that only projects out the entity side
            //
            Node projectNode = m_command.BuildProject(filterNode, new Var[] { unionAllVars[1] }, new Node[] { });

            //
            // Build a collectOp over the project node
            //
            ret = m_command.BuildCollect(projectNode, unionAllVars[1]);

            return ret;
        }

        /// <summary>
        /// Rewrite a NavProperty; more generally, consider this a rewrite of DEREF(NAVIGATE(r))
        /// 
        /// We handle four cases here, depending on the kind of relationship we're
        /// dealing with.
        ///   - 1:1 relationships
        ///   - 1:M relationships
        ///   - N:1 relationships
        ///   - N:M relationships
        /// 
        /// </summary>
        /// <param name="navProperty">the navigation property</param>
        /// <param name="sourceEntityNode">the input ref to start the traversal</param>
        /// <param name="resultType">the result type of the expression</param>
        /// <returns>the rewritten tree</returns>
        private Node RewriteNavigationProperty(NavigationProperty navProperty,
            Node sourceEntityNode, TypeUsage resultType)
        {
            RelProperty relProperty = new RelProperty(navProperty.RelationshipType, navProperty.FromEndMember, navProperty.ToEndMember);
            PlanCompiler.Assert(m_command.IsRelPropertyReferenced(relProperty) || (relProperty.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many),
                "Unreferenced rel property? " + relProperty);

            // Handle N:1
            if ((relProperty.FromEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many) &&
                (relProperty.ToEnd.RelationshipMultiplicity != RelationshipMultiplicity.Many))
            {
                return RewriteManyToOneNavigationProperty(relProperty, sourceEntityNode, resultType);
            }

            //
            // Find the list of all relationships that could satisfy this relationship
            // If we find no matching relationship set, simply return a null node / empty collection
            //
            List<RelationshipSet> relationshipSets = GetRelationshipSets(relProperty.Relationship);
            if (relationshipSets.Count == 0)
            {
                // return an empty set / null node
                if (relProperty.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                {
                    return m_command.CreateNode(m_command.CreateNewMultisetOp(resultType));
                }
                return m_command.CreateNode(m_command.CreateNullOp(resultType));
            }

            // Build out a ref over the source entity 
            Node sourceRefNode = m_command.CreateNode(
                m_command.CreateGetEntityRefOp(relProperty.FromEnd.TypeUsage),
                sourceEntityNode);

            // Hanlde the 1:M and N:M cases
            if (relProperty.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                // Handle N:M
                if (relProperty.FromEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                {
                    return RewriteManyToManyNavigationProperty(relProperty, relationshipSets, sourceRefNode);
                }
                // Handle 1:M
                return RewriteOneToManyNavigationProperty(relProperty, relationshipSets, sourceRefNode);
            }

            // Handle 1:1
            return RewriteOneToOneNavigationProperty(relProperty, relationshipSets,sourceRefNode);
        }

        #endregion

        #region visitor methods

        #region ScalarOps

        /// <summary>
        /// Default handler for scalar Ops. Simply traverses the children,
        /// and also identifies any structured types along the way
        /// </summary>
        /// <param name="op">the ScalarOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>the possibly modified node</returns>
        protected override Node VisitScalarOpDefault(ScalarOp op, Node n)
        {
            VisitChildren(n); // visit my children

            // keep track of referenced types
            AddTypeReference(op.Type);

            return n;
        }

        /// <summary>
        /// Rewrite a DerefOp subtree. We have two cases to consider here. 
        /// We call RewriteDerefOp to return a subtree (and an optional outputVar). 
        /// If the outputVar is null, then we simply return the subtree produced by those calls. 
        /// Otherwise, we add the subtree to the "parent" relop (to be outer-applied), and then use the outputVar
        /// in its place. 
        /// 
        /// As an example, 
        ///    select deref(e) from T
        /// gets rewritten into
        ///    select v from T OuterApply X
        /// where X is the subtree returned from the RewriteXXX calls, and "v" is the output var produced by X
        /// 
        /// </summary>
        /// <param name="op">the derefOp</param>
        /// <param name="n">the deref subtree</param>
        /// <returns>the rewritten tree</returns>
        public override Node Visit(DerefOp op, Node n)
        {
            Var outputVar;

            VisitScalarOpDefault(op, n);

            Node ret = RewriteDerefOp(n, op, out outputVar);
            ret = VisitNode(ret);

            if (outputVar != null)
            {
                ret = AddSubqueryToParentRelOp(outputVar, ret);
            }

            return ret;
        }

        /// <summary>
        /// Processing for an ElementOp. Replaces this by the corresponding Var from
        /// the subquery, and adds the subquery to the list of currently tracked subqueries
        /// </summary>
        /// <param name="op">the elementOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>the Var from the subquery</returns>
        public override Node Visit(ElementOp op, Node n)
        {
            VisitScalarOpDefault(op, n); // default processing

            // get to the subquery...
            Node subQueryRelOp = n.Child0;
            ProjectOp projectOp = (ProjectOp)subQueryRelOp.Op;
            PlanCompiler.Assert(projectOp.Outputs.Count == 1, "input to ElementOp has more than one output var?");
            Var projectVar = projectOp.Outputs.First;

            Node ret = AddSubqueryToParentRelOp(projectVar, subQueryRelOp);
            return ret;
        }

        /// <summary>
        /// Mark Normalization as needed
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ExistsOp op, Node n)
        {
            m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.Normalization);
            return base.Visit(op, n);
        }

        /// <summary>
        /// Visit a function call expression. If function is mapped, expand and visit the mapping expression.
        /// If this is TVF or a collection aggregate function, NestPullUp and Normalization are needed.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(FunctionOp op, Node n)
        {
            if (op.Function.IsFunctionImport)
            {
                PlanCompiler.Assert(op.Function.IsComposableAttribute, "Cannot process a non-composable function inside query tree composition.");

                FunctionImportMapping functionImportMapping = null;
                if (!m_command.MetadataWorkspace.TryGetFunctionImportMapping(op.Function, out functionImportMapping))
                {
                    throw EntityUtil.Metadata(System.Data.Entity.Strings.EntityClient_UnmappedFunctionImport(op.Function.FullName));
                }
                PlanCompiler.Assert(functionImportMapping is FunctionImportMappingComposable, "Composable function import must have corresponding mapping.");
                var functionImportMappingComposable = (FunctionImportMappingComposable)functionImportMapping;

                // Visit children (function call arguments) before processing the function view.
                // Visiting argument trees before the view tree is required because we want to process them first
                // outside of the context of the view. For example if an argument tree contains a free-floating entity-type constructor
                // and the function mapping scopes the function results to a particular entity set, we don't want 
                // the free-floating constructor to be auto-scoped to this set. So we process the argument first, it will
                // scope the constructor to the null scope and which guarantees that this constructor will not be rescoped after the argument
                // tree is embedded into the function view inside the functionMapping.GetInternalTree(...) call.
                VisitChildren(n);

                // Get the mapping view of the function.
                Node ret = functionImportMappingComposable.GetInternalTree(m_command, n.Children);
                
                // Push the entity type scope, if any, before processing the view.
                if (op.Function.EntitySet != null)
                {
                    m_entityTypeScopes.Push(op.Function.EntitySet);
                    AddEntitySetReference(op.Function.EntitySet);
                    PlanCompiler.Assert(functionImportMappingComposable.TvfKeys != null && functionImportMappingComposable.TvfKeys.Length > 0, "Function imports returning entities must have inferred keys.");
                    if (!m_tvfResultKeys.ContainsKey(functionImportMappingComposable.TargetFunction))
                    {
                        m_tvfResultKeys.Add(functionImportMappingComposable.TargetFunction, functionImportMappingComposable.TvfKeys);
                    }
                }
                
                // Rerun the processor over the resulting subtree.
                ret = VisitNode(ret);

                // Remove the entity type scope, if any.
                if (op.Function.EntitySet != null)
                {
                    var scope = m_entityTypeScopes.Pop();
                    PlanCompiler.Assert(scope == op.Function.EntitySet, "m_entityTypeScopes stack is broken");
                }

                return ret;
            }
            else
            {
                PlanCompiler.Assert(op.Function.EntitySet == null, "Entity type scope is not supported on functions that aren't mapped.");

                // If this is TVF or a collection aggregate, function NestPullUp and Normalization are needed.
                if (TypeSemantics.IsCollectionType(op.Type) || PlanCompilerUtil.IsCollectionAggregateFunction(op, n))
                {
                    m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.NestPullup);
                    m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.Normalization);
                }
                return base.Visit(op, n);
            }
        }

        /// <summary>
        /// Default processing. 
        /// In addition, if the case statement is of the shape 
        ///     case when X then NULL else Y, or
        ///     case when X then Y else NULL,
        /// where Y is of row type and the types of the input CaseOp, the NULL and Y are the same,
        /// marks that type as needing a null sentinel.
        /// This allows in NominalTypeElimination the case op to be pushed inside Y's null sentinel.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(CaseOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            //special handling to enable optimization
            bool thenClauseIsNull;
            if (PlanCompilerUtil.IsRowTypeCaseOpWithNullability(op, n, out thenClauseIsNull))
            {
                //Add a null sentinel for the row type
                m_typesNeedingNullSentinel.Add(op.Type.EdmType.Identity);
            }
            return n;
        }

        /// <summary>
        /// Special processing for ConditionalOp is handled by <see cref="ProcessConditionalOp"/>
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ConditionalOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            ProcessConditionalOp(op, n);
            return n;
        }

        /// <summary>
        /// If it is a IsNull op over a row type or a complex type mark the type as needing a null sentinel.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        private void ProcessConditionalOp(ConditionalOp op, Node n)
        {
            if (op.OpType == OpType.IsNull && TypeSemantics.IsRowType(n.Child0.Op.Type) || TypeSemantics.IsComplexType(n.Child0.Op.Type))
            {
                StructuredTypeNullabilityAnalyzer.MarkAsNeedingNullSentinel(m_typesNeedingNullSentinel, n.Child0.Op.Type);
            }
        }

        #region PropertyOp Handling

        /// <summary>
        /// Validates that the nav property agrees with the underlying relationship
        /// </summary>
        /// <param name="op">the Nav PropertyOp</param>
        /// <param name="n">the subtree</param>
        private void ValidateNavPropertyOp(PropertyOp op, Node n)
        {
            NavigationProperty navProperty = (NavigationProperty)op.PropertyInfo;

            //
            // If the result of the expanded form of the navigation property is not compatible with
            // the declared type of the property, then the navigation property is invalid in the
            // context of this command tree's metadata workspace.
            //
            TypeUsage resultType = navProperty.ToEndMember.TypeUsage;
            if (TypeSemantics.IsReferenceType(resultType))
            {
                resultType = TypeHelpers.GetElementTypeUsage(resultType);
            }
            if (navProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                resultType = TypeUsage.Create(resultType.EdmType.GetCollectionType());
            }
            if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(resultType, op.Type))
            {
                throw EntityUtil.Metadata(System.Data.Entity.Strings.EntityClient_IncompatibleNavigationPropertyResult(
                        navProperty.DeclaringType.FullName,
                        navProperty.Name
                    )
                );
            }
        }

        /// <summary>
        /// Rewrite a PropertyOp subtree for a nav property
        /// <see cref="RewriteNavigationProperty"/> does the heavy lifting
        /// </summary>
        /// <param name="op">the PropertyOp</param>
        /// <param name="n">the current node</param>
        /// <returns>the rewritten subtree</returns>
        private Node VisitNavPropertyOp(PropertyOp op, Node n)
        {
            ValidateNavPropertyOp(op, n);

            //
            // In this special case we visit the parent before the child to avoid TSQL regressions. 
            // In particular, a subquery coming out of the child would need to be attached to the closest rel-op parent
            // and if the parent is already visited that rel op parent would be part of the subtree resulting from the parent.
            // If the parent is not visited it would be a rel op parent higher in the tree (also valid), and leaves less room 
            // for join elimination. 
            // The original out-of-order visitation was put in place to work around a 

            bool visitChildLater = IsNavigationPropertyOverVarRef(n.Child0);
            if (!visitChildLater)
            {
                VisitScalarOpDefault(op, n);
            }

            NavigationProperty navProperty = (NavigationProperty)op.PropertyInfo;
            Node ret = RewriteNavigationProperty(navProperty, n.Child0, op.Type);
            ret = VisitNode(ret);

            return ret;
        }

        /// <summary>
        /// Is the given node of shape NavigationProperty(SoftCast(VarRef)), or NavigationProperty(VarRef)
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static bool IsNavigationPropertyOverVarRef(Node n)
        {
            if (n.Op.OpType != OpType.Property || (!Helper.IsNavigationProperty(((PropertyOp)n.Op).PropertyInfo)))
            {
                return false;
            }
            
            Node currentNode = n.Child0;
            if (currentNode.Op.OpType == OpType.SoftCast)
            {
                currentNode = currentNode.Child0;
            }
            return currentNode.Op.OpType == OpType.VarRef;
        }

        /// <summary>
        /// Rewrite a PropertyOp subtree.  
        /// 
        /// If the PropertyOp represents a simple property (ie) not a navigation property, we simply call
        /// VisitScalarOpDefault() and return. Otherwise, we call VisitNavPropertyOp and return the result from
        /// that function
        /// 
        /// </summary>
        /// <param name="op">the PropertyOp</param>
        /// <param name="n">the PropertyOp subtree</param>
        /// <returns>the rewritten tree</returns>
        public override Node Visit(PropertyOp op, Node n)
        {
            Node ret;
            if (Helper.IsNavigationProperty(op.PropertyInfo))
            {
                ret = VisitNavPropertyOp(op, n);
            }
            else
            {
                ret = VisitScalarOpDefault(op, n);
            }
            return ret;
        }

        #endregion

        /// <summary>
        /// Handler for a RefOp. 
        /// Keeps track of the entityset
        /// </summary>
        /// <param name="op">the RefOp</param>
        /// <param name="n">current RefOp subtree</param>
        /// <returns>current subtree</returns>
        public override Node Visit(RefOp op, Node n)
        {
            VisitScalarOpDefault(op, n); // use default processing
            AddEntitySetReference(op.EntitySet); // add to list of references
            return n;
        }

        /// <summary>
        /// Handler for a TreatOp.
        /// Rewrites the operator if the argument is guaranteed to be of type
        /// op.
        /// </summary>
        /// <param name="op">Current TreatOp</param>
        /// <param name="n">Current subtree</param>
        /// <returns>Current subtree</returns>
        public override Node Visit(TreatOp op, Node n)
        {
            n = base.Visit(op, n);

            // See if TreatOp can be rewritten (if it's not polymorphic)
            if (CanRewriteTypeTest(op.Type.EdmType, n.Child0.Op.Type.EdmType))
            {
                // Return argument directly (if the argument is null, 'treat as' also returns null;
                // if the argument is not null, it's guaranteed to be of the correct type)
                return n.Child0;
            }

            return n;
        }

        /// <summary>
        /// Handler for an IsOfOp.
        /// Keeps track of the IsOfType (if it is a structured type) and rewrites the
        /// operator if the argument is guaranteed to be of type op.IsOfType
        /// </summary>
        /// <param name="op">Current IsOfOp</param>
        /// <param name="n">Current subtree</param>
        /// <returns>Current subtree</returns>
        public override Node Visit(IsOfOp op, Node n)
        {
            VisitScalarOpDefault(op, n); // default handling first
            // keep track of any structured types
            AddTypeReference(op.IsOfType);

            // See if the IsOfOp can be rewritten (if it's not polymorphic)
            if (CanRewriteTypeTest(op.IsOfType.EdmType, n.Child0.Op.Type.EdmType))
            {
                n = RewriteIsOfAsIsNull(op, n);
            }

            // For IsOfOnly(abstract type), suppress DiscriminatorMaps since no explicit type id is available for
            // abstract types.
            if (op.IsOfOnly && op.IsOfType.EdmType.Abstract)
            {
                m_suppressDiscriminatorMaps = true;
            }

            return n;
        }

        // Determines whether a type test expression can be rewritten. Returns true of the
        // argument type is guaranteed to implement "testType" (if the argument is non-null).
        private bool CanRewriteTypeTest(EdmType testType, EdmType argumentType)
        {
            // The rewrite only proceeds if the types are the same. If they are not,
            // it suggests either that the input result is polymorphic (in which case if OfType
            // should be preserved) or the types are incompatible (which is caught
            // elsewhere)
            if (!testType.EdmEquals(argumentType))
            {
                return false;
            }

            // If the IsOfType is non-polymorphic (no base or derived types) the rewrite
            // is possible.
            if (null != testType.BaseType)
            {
                return false;
            }

            // Count sub types
            int subTypeCount = 0;
            foreach (EdmType subType in MetadataHelper.GetTypeAndSubtypesOf(testType, m_command.MetadataWorkspace, true /*includeAbstractTypes*/))
            {
                subTypeCount++;
                if (2 == subTypeCount) { break; }
            }

            return 1 == subTypeCount; // no children types
        }

        // Translates 
        //      'R is of T' 
        // to 
        //      '(case when not (R is null) then True else null end) = True'
        //
        // Input requirements:
        //
        //      - IsOfOp and argument to same must be in the same hierarchy.
        //      - IsOfOp and argument must have the same type
        //      - IsOfOp.IsOfType may not have super- or sub- types (validate
        //        using CanRewriteTypeTest)
        //
        // Design requirements:
        //
        //      - Must return true if the record exists
        //      - Must return null if it does not
        //      - Must be in predicate form to avoid confusing SQL gen
        //
        // The translation assumes R is of T when R is non null.
        private Node RewriteIsOfAsIsNull(IsOfOp op, Node n)
        {
            // construct 'R is null' predicate
            ConditionalOp isNullOp = m_command.CreateConditionalOp(OpType.IsNull);
            Node isNullNode = m_command.CreateNode(isNullOp, n.Child0);

            // Process the IsNull node to make sure a null sentinel gets added if needed
            ProcessConditionalOp(isNullOp, isNullNode);

            // construct 'not (R is null)' predicate
            ConditionalOp notOp = m_command.CreateConditionalOp(OpType.Not);
            Node notNode = m_command.CreateNode(notOp, isNullNode);

            // construct 'True' result
            ConstantBaseOp trueOp = m_command.CreateConstantOp(op.Type, true);
            Node trueNode = m_command.CreateNode(trueOp);

            // construct 'null' default result
            NullOp nullOp = m_command.CreateNullOp(op.Type);
            Node nullNode = m_command.CreateNode(nullOp);

            // create case statement
            CaseOp caseOp = m_command.CreateCaseOp(op.Type);
            Node caseNode = m_command.CreateNode(caseOp, notNode, trueNode, nullNode);

            // create 'case = true' operator
            ComparisonOp equalsOp = m_command.CreateComparisonOp(OpType.EQ);
            Node equalsNode = m_command.CreateNode(equalsOp, caseNode, trueNode);

            return equalsNode;
        }

        /// <summary>
        /// Rewrite a NavigateOp subtree.  
        /// We call RewriteNavigateOp to return a subtree (and an optional outputVar). 
        /// If the outputVar is null, then we simply return the subtree produced by those calls. 
        /// Otherwise, we add the subtree to the "parent" relop (to be outer-applied), and then use the outputVar
        /// in its place. 
        /// 
        /// As an example, 
        ///    select navigate(e) from T
        /// gets rewritten into
        ///    select v from T OuterApply X
        /// where X is the subtree returned from the RewriteXXX calls, and "v" is the output var produced by X
        /// 
        /// </summary>
        /// <param name="op">the navigateOp</param>
        /// <param name="n">the navigateOp subtree</param>
        /// <returns>the rewritten tree</returns>
        public override Node Visit(NavigateOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            Var outputVar;
            Node ret = RewriteNavigateOp(n, op, out outputVar);
            ret = VisitNode(ret);

            // Move subquery to parent relop if necessary
            if (outputVar != null)
            {
                ret = AddSubqueryToParentRelOp(outputVar, ret);
            }
            return ret;
        }

        /// <summary>
        /// Returns the current entity set scope, if any, for an entity type constructor.
        /// The scope defines the result of the construtor as a scoped entity type.
        /// </summary>
        private EntitySet GetCurrentEntityTypeScope()
        {
            if (m_entityTypeScopes.Count == 0)
            {
                return null;
            }
            return m_entityTypeScopes.Peek();
        }

        /// <summary>
        /// Find the relationshipset that matches the current entityset + from/to roles
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="relProperty"></param>
        /// <returns></returns>
        private RelationshipSet FindRelationshipSet(EntitySetBase entitySet, RelProperty relProperty)
        {
            foreach (EntitySetBase es in entitySet.EntityContainer.BaseEntitySets)
            {
                AssociationSet rs = es as AssociationSet;
                if (rs != null &&
                    rs.ElementType.EdmEquals(relProperty.Relationship) &&
                    rs.AssociationSetEnds[relProperty.FromEnd.Identity].EntitySet.EdmEquals(entitySet))
                {
                    return rs;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the position of a property in a type. 
        /// Positions start at zero, and a supertype's properties precede the current
        /// type's properties
        /// </summary>
        /// <param name="type">the type in question</param>
        /// <param name="member">the member to lookup</param>
        /// <returns>the position of the member in the type (0-based)</returns>
        private int FindPosition(EdmType type, EdmMember member)
        {
            int pos = 0;
            foreach (EdmMember m in TypeHelpers.GetAllStructuralMembers(type))
            {
                if (m.EdmEquals(member))
                {
                    return pos;
                }
                pos++;
            }
            PlanCompiler.Assert(false, "Could not find property " + member + " in type " + type.Name);
            return -1;
        }

        /// <summary>
        /// Build out an expression (NewRecord) that corresponds to the key properties
        /// of the passed-in entity constructor
        /// 
        /// This function simply looks up the key properties of the entity type, and then
        /// identifies the arguments to the constructor corresponding to those 
        /// properties, and then slaps on a record wrapper over those expressions.
        /// 
        /// No copies/clones are performed. That's the responsibility of the caller
        /// 
        /// </summary>
        /// <param name="op">the entity constructor op</param>
        /// <param name="n">the corresponding subtree</param>
        /// <returns>the key expression</returns>
        private Node BuildKeyExpressionForNewEntityOp(Op op, Node n)
        {
            PlanCompiler.Assert(op.OpType == OpType.NewEntity || op.OpType == OpType.DiscriminatedNewEntity,
                "BuildKeyExpression: Unexpected OpType:" + op.OpType);
            int offset = (op.OpType == OpType.DiscriminatedNewEntity) ? 1 : 0;
            EntityTypeBase entityType = (EntityTypeBase)op.Type.EdmType;
            List<Node> keyFields = new List<Node>();
            List<KeyValuePair<string, TypeUsage>> keyFieldTypes = new List<KeyValuePair<string, TypeUsage>>();
            foreach (EdmMember k in entityType.KeyMembers)
            {
                int pos = FindPosition(entityType, k) + offset;
                PlanCompiler.Assert(n.Children.Count > pos, "invalid position " + pos + "; total count = " + n.Children.Count);
                keyFields.Add(n.Children[pos]);
                keyFieldTypes.Add(new KeyValuePair<string, TypeUsage>(k.Name, k.TypeUsage));
            }
            TypeUsage keyExprType = TypeHelpers.CreateRowTypeUsage(keyFieldTypes, true);
            NewRecordOp keyOp = m_command.CreateNewRecordOp(keyExprType);
            Node keyNode = m_command.CreateNode(keyOp, keyFields);
            return keyNode;
        }

        /// <summary>
        /// Build out an expression corresponding to the rel-property. 
        /// 
        /// We create a subquery that looks like
        ///    (select r
        ///     from RS r
        ///     where GetRefKey(r.FromEnd) = myKey)
        ///  
        /// RS is the single relationship set that corresponds to the given entityset/rel-property pair
        /// FromEnd - is the source end of the relationship
        /// myKey - is the key expression of the entity being constructed
        /// 
        /// NOTE: We always clone "myKey" before use.
        /// 
        /// We then convert it into a scalar subquery, and extract out the ToEnd property from
        /// the output var of the subquery. (Should we do this inside the subquery itself?)
        /// 
        /// If no single relationship-set is found, we return a NULL instead.
        /// </summary>
        /// <param name="entitySet">entity set that logically holds instances of the entity we're building</param>
        /// <param name="relProperty">the rel-property we're trying to build up</param>
        /// <param name="keyExpr">the "key" of the entity instance</param>
        /// <returns>the rel-property expression</returns>
        private Node BuildRelPropertyExpression(EntitySetBase entitySet, RelProperty relProperty,
            Node keyExpr)
        {
            //
            // Make a copy of the current key expression
            //
            keyExpr = OpCopier.Copy(m_command, keyExpr);

            //
            // Find the relationship set corresponding to this entityset (and relProperty)
            // Return a null ref, if we can't find one
            //
            RelationshipSet relSet = FindRelationshipSet(entitySet, relProperty);
            if (relSet == null)
            {
                return m_command.CreateNode(m_command.CreateNullOp(relProperty.ToEnd.TypeUsage));
            }

            ScanTableOp scanTableOp = m_command.CreateScanTableOp(Command.CreateTableDefinition(relSet));
            PlanCompiler.Assert(scanTableOp.Table.Columns.Count == 1,
                "Unexpected column count for table:" + scanTableOp.Table.TableMetadata.Extent + "=" + scanTableOp.Table.Columns.Count);
            Var scanTableVar = scanTableOp.Table.Columns[0];
            Node scanNode = m_command.CreateNode(scanTableOp);

            Node sourceEndNode = m_command.CreateNode(
                m_command.CreatePropertyOp(relProperty.FromEnd),
                m_command.CreateNode(m_command.CreateVarRefOp(scanTableVar)));
            Node predicateNode = m_command.BuildComparison(OpType.EQ,
                keyExpr,
                m_command.CreateNode(m_command.CreateGetRefKeyOp(keyExpr.Op.Type), sourceEndNode));
            Node filterNode = m_command.CreateNode(m_command.CreateFilterOp(),
                scanNode, predicateNode);

            //
            // Process the node, and then add this as a subquery to the parent relop
            //
            Node ret = VisitNode(filterNode);
            ret = AddSubqueryToParentRelOp(scanTableVar, ret);

            //
            // Now extract out the target end property
            //
            ret = m_command.CreateNode(
                m_command.CreatePropertyOp(relProperty.ToEnd),
                ret);

            return ret;
        }

        /// <summary>
        /// Given an entity constructor (NewEntityOp, DiscriminatedNewEntityOp), build up
        /// the list of rel-property expressions. 
        /// 
        /// Walks through the list of relevant rel-properties, and builds up expressions
        /// (using BuildRelPropertyExpression) for each rel-property that does not have
        /// an expression already built (preBuiltExpressions)
        /// </summary>
        /// <param name="entitySet">entity set that holds instances of the entity we're building</param>
        /// <param name="relPropertyList">the list of relevant rel-properties for this entity type</param>
        /// <param name="prebuiltExpressions">the prebuilt rel-property expressions</param>
        /// <param name="keyExpr">the key of the entity instance</param>
        /// <returns>a list of rel-property expressions (lines up 1-1 with 'relPropertyList')</returns>
        private IEnumerable<Node> BuildAllRelPropertyExpressions(EntitySetBase entitySet,
            List<RelProperty> relPropertyList,
            Dictionary<RelProperty, Node> prebuiltExpressions,
            Node keyExpr)
        {
            foreach (RelProperty r in relPropertyList)
            {
                Node relPropNode;
                if (!prebuiltExpressions.TryGetValue(r, out relPropNode))
                {
                    relPropNode = BuildRelPropertyExpression(entitySet, r, keyExpr);
                }
                yield return relPropNode;
            }
        }

        /// <summary>
        /// Handler for NewEntityOp.
        /// Assignes scope to the entity constructor if it hasn't been assigned before.
        /// </summary>
        /// <param name="op">the NewEntityOp</param>
        /// <param name="n">the node tree corresponding to the op</param>
        /// <returns>rewritten tree</returns>
        public override Node Visit(NewEntityOp op, Node n)
        {
            // If this is not an entity type constructor, or it's been already scoped, 
            // then just do the default processing.
            if (op.Scoped || op.Type.EdmType.BuiltInTypeKind != BuiltInTypeKind.EntityType)
            {
                return base.Visit(op, n);
            }

            EntityType entityType = (EntityType)op.Type.EdmType;
            EntitySet scope = GetCurrentEntityTypeScope();

            List<RelProperty> relProperties;
            List<Node> newChildren;

            if (scope == null)
            {
                m_freeFloatingEntityConstructorTypes.Add(entityType);

                // SQLBUDT #546546: Qmv/Umv tests Assert and throws in plan compiler in association tests.
                // If this Entity constructor is not within a view then there should not be any RelProps
                // specified on the NewEntityOp - the eSQL WITH RELATIONSHIP clauses that would cause such
                // RelProps to be added is only enabled when parsing in the user or generated view mode.
                PlanCompiler.Assert(op.RelationshipProperties == null ||
                                    op.RelationshipProperties.Count == 0,
                                    "Related Entities cannot be specified for Entity constructors that are not part of the Query Mapping View for an Entity Set.");

                // Default processing.
                VisitScalarOpDefault(op, n);

                relProperties = op.RelationshipProperties;
                newChildren = n.Children;
            }
            else
            {
                //
                // Note: We don't do the default processing first to avoid adding references to types and entity sets
                // that may only be used in pre-built rel property expressions that may not be needed.
                //

                // 
                // Find the relationship properties for this entitytype (and entity set)
                //
                relProperties = new List<RelProperty>(m_relPropertyHelper.GetRelProperties(entityType));

                // Remove pre-built rel property expressions that would not be needed to avoid 
                // unnecessary adding references to types and entity sets during default processing
                int j = op.RelationshipProperties.Count - 1;
                List<RelProperty> copiedRelPropList = new List<RelProperty>(op.RelationshipProperties);
                for (int i = n.Children.Count - 1; i >= entityType.Properties.Count; i--, j--)
                {
                    if (!relProperties.Contains(op.RelationshipProperties[j]))
                    {
                        n.Children.RemoveAt(i);
                        copiedRelPropList.RemoveAt(j);
                    }
                }

                // Default processing.
                VisitScalarOpDefault(op, n);

                //
                // Ok, now, I have to build out some relationship properties that 
                // haven't been specified
                //
                Node keyExpr = BuildKeyExpressionForNewEntityOp(op, n);

                // 
                // Find the list of rel properties that have already been specified
                // 
                Dictionary<RelProperty, Node> prebuiltRelPropertyExprs = new Dictionary<RelProperty, Node>();
                j = 0;
                for (int i = entityType.Properties.Count; i < n.Children.Count; i++, j++)
                {
                    prebuiltRelPropertyExprs[copiedRelPropList[j]] = n.Children[i];
                }

                //
                // Next, rebuild the list of children - includes expressions for each rel property
                //
                newChildren = new List<Node>();
                for (int i = 0; i < entityType.Properties.Count; i++)
                {
                    newChildren.Add(n.Children[i]);
                }

                foreach (Node relPropNode in BuildAllRelPropertyExpressions(scope, relProperties, prebuiltRelPropertyExprs, keyExpr))
                {
                    newChildren.Add(relPropNode);
                }
            }

            //
            // Finally, build out the newOp.
            //
            Op newEntityOp = m_command.CreateScopedNewEntityOp(op.Type, relProperties, scope);
            Node newNode = m_command.CreateNode(newEntityOp, newChildren);
            return newNode;
        }

        /// <summary>
        /// Tracks discriminator metadata so that is can be used when constructing
        /// StructuredTypeInfo.
        /// </summary>
        public override Node Visit(DiscriminatedNewEntityOp op, Node n)
        {
            HashSet<RelProperty> relPropertyHashSet = new HashSet<RelProperty>();
            List<RelProperty> relProperties = new List<RelProperty>();
            //
            // add references to each type produced by this node
            // Also, get the set of rel-properties for each of the types
            //
            foreach (var discriminatorTypePair in op.DiscriminatorMap.TypeMap)
            {
                EntityTypeBase entityType = discriminatorTypePair.Value;
                AddTypeReference(TypeUsage.Create(entityType));
                foreach (RelProperty relProperty in m_relPropertyHelper.GetRelProperties(entityType))
                {
                    relPropertyHashSet.Add(relProperty);
                }
            }
            relProperties = new List<RelProperty>(relPropertyHashSet);
            VisitScalarOpDefault(op, n);

            //
            // Now build out the set of missing rel-properties (if any)
            //

            // first, build the key expression
            Node keyExpr = BuildKeyExpressionForNewEntityOp(op, n);

            List<Node> newChildren = new List<Node>();
            int firstRelPropertyNodeOffset = n.Children.Count - op.RelationshipProperties.Count;
            for (int i = 0; i < firstRelPropertyNodeOffset; i++)
            {
                newChildren.Add(n.Children[i]);
            }
            // 
            // Find the list of rel properties that have already been specified
            // 
            Dictionary<RelProperty, Node> prebuiltRelPropertyExprs = new Dictionary<RelProperty, Node>();
            for (int i = firstRelPropertyNodeOffset, j = 0; i < n.Children.Count; i++, j++)
            {
                prebuiltRelPropertyExprs[op.RelationshipProperties[j]] = n.Children[i];
            }

            //
            // Fill in the missing pieces
            //
            foreach (Node relPropNode in BuildAllRelPropertyExpressions(op.EntitySet, relProperties, prebuiltRelPropertyExprs, keyExpr))
            {
                newChildren.Add(relPropNode);
            }

            Op newEntityOp = m_command.CreateDiscriminatedNewEntityOp(op.Type, op.DiscriminatorMap, op.EntitySet, relProperties);
            Node newNode = m_command.CreateNode(newEntityOp, newChildren);

            return newNode;
        }

        /// <summary>
        /// Handles a newMultiset constructor. Converts this into 
        ///   select a from dual union all select b from dual union all ...
        /// Handles a NewMultiset constructor, i.e. {x, y, z}
        ///   1. Empty multiset constructors are simply converted into:
        ///    
        ///        select x from singlerowtable as x where false
        ///   
        ///   2. Mulltset constructors with only one element or with multiple elements all of 
        ///   which are constants or nulls are converted into: 
        ///   
        ///     select x from dual union all select y from dual union all select z
        ///     
        ///   3. All others are converted into:
        ///   
        ///      select case when d = 0 then x when d = 1 then y else z end
        ///      from (  select 0 as d from single_row_table
        ///              union all 
        ///              select 1 as d from single_row_table
        ///              union all
        ///              select 2 as d  from single_row_table )
        ///              
        ///       NOTE: The  translation for 2 is valid for 3 too. We choose different translation 
        ///       in order to avoid correlation inside the union all,
        ///       which would prevent us from removing apply operators
        /// 
        /// Do this before processing the children, and then 
        /// call Visit on the result to handle the elements
        /// </summary>
        /// <param name="op">the new instance op</param>
        /// <param name="n">the current subtree</param>
        /// <returns>the modified subtree</returns>
        public override Node Visit(NewMultisetOp op, Node n)
        {
            Node resultNode = null;
            Var resultVar = null;

            CollectionType collectionType = TypeHelpers.GetEdmType<CollectionType>(op.Type);

            // 
            // Empty multiset constructors are simply converted into 
            //    Project(Filter(SingleRowTableOp(), false)
            // 
            if (!n.HasChild0)
            {
                Node singleRowTableNode = m_command.CreateNode(m_command.CreateSingleRowTableOp());
                Node filterNode = m_command.CreateNode(m_command.CreateFilterOp(),
                    singleRowTableNode,
                    m_command.CreateNode(m_command.CreateFalseOp()));
                Node fakeChild = m_command.CreateNode(m_command.CreateNullOp(collectionType.TypeUsage));
                Var newVar;
                Node projectNode = m_command.BuildProject(filterNode, fakeChild, out newVar);

                resultNode = projectNode;
                resultVar = newVar;
            }

            //
            // Multiset constructors with only one elment or with multiple elments all of 
            //   which are constants or nulls are converted into: 
            //    
            // UnionAll(Project(SingleRowTable, e1), Project(SingleRowTable, e2), ...)
            // 
            // The degenerate case when the collection has only one element does not require an
            // outer unionAll node
            //
            else if (n.Children.Count == 1 || AreAllConstantsOrNulls(n.Children))
            {
                List<Node> inputNodes = new List<Node>();
                List<Var> inputVars = new List<Var>();
                foreach (Node chi in n.Children)
                {
                    Node singleRowTableNode = m_command.CreateNode(m_command.CreateSingleRowTableOp());
                    Var newVar;
                    Node projectNode = m_command.BuildProject(singleRowTableNode, chi, out newVar);
                    inputNodes.Add(projectNode);
                    inputVars.Add(newVar);
                }
                // Build the union-all ladder
                m_command.BuildUnionAllLadder(inputNodes, inputVars, out resultNode, out resultVar);

            }
            //
            //   All other cases:
            //
            //  select case when d = 0 then x when d = 1 then y else z end
            //  from (  select 0 as d from single_row_table
            //          union all 
            //          select 1 as d from single_row_table
            //          union all
            //          select 2 as d  from single_row_table )
            //
            else
            {
                List<Node> inputNodes = new List<Node>();
                List<Var> inputVars = new List<Var>();
                //Create the union all lather first
                for (int i = 0; i < n.Children.Count; i++)
                {
                    Node singleRowTableNode = m_command.CreateNode(m_command.CreateSingleRowTableOp());
                    // the discriminator for this branch
                    Node discriminatorNode = m_command.CreateNode(m_command.CreateInternalConstantOp(m_command.IntegerType, i));
                    Var newVar;
                    Node projectNode = m_command.BuildProject(singleRowTableNode, discriminatorNode, out newVar);

                    inputNodes.Add(projectNode);
                    inputVars.Add(newVar);
                }
                // Build the union-all ladder now
                m_command.BuildUnionAllLadder(inputNodes, inputVars, out resultNode, out resultVar);

                //Now create the case statement for the projection
                List<Node> caseArgNodes = new List<Node>(n.Children.Count * 2 + 1);
                for (int i = 0; i < n.Children.Count; i++)
                {
                    //For all but the last we need a when
                    if (i != (n.Children.Count - 1))
                    {
                        ComparisonOp equalsOp = m_command.CreateComparisonOp(OpType.EQ);
                        Node whenNode = m_command.CreateNode(equalsOp,
                            m_command.CreateNode(m_command.CreateVarRefOp(resultVar)),
                            m_command.CreateNode(
                                m_command.CreateConstantOp(m_command.IntegerType, i)));
                        caseArgNodes.Add(whenNode);
                    }

                    //Add the then/else node
                    caseArgNodes.Add(n.Children[i]);
                }

                //Create the project
                Node caseNode = m_command.CreateNode(m_command.CreateCaseOp(collectionType.TypeUsage), caseArgNodes);
                resultNode = m_command.BuildProject(resultNode, caseNode, out resultVar);
            }

            // So, I've finally built up a complex query corresponding to the constructor.
            // Now, cap this with a physicalprojectOp, and then with a CollectOp
            PhysicalProjectOp physicalProjectOp = m_command.CreatePhysicalProjectOp(resultVar);
            Node physicalProjectNode = m_command.CreateNode(physicalProjectOp, resultNode);

            CollectOp collectOp = m_command.CreateCollectOp(op.Type);
            Node collectNode = m_command.CreateNode(collectOp, physicalProjectNode);

            return VisitNode(collectNode);
        }

        /// <summary>
        /// Returns true if each node in the list is either a constant or a null
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private bool AreAllConstantsOrNulls(List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.Op.OpType != OpType.Constant && node.Op.OpType != OpType.Null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Default processing for a CollectOp. But make sure that we 
        /// go through the NestPullUp phase
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(CollectOp op, Node n)
        {
            m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.NestPullup);
            return VisitScalarOpDefault(op, n);
        }

        #endregion

        #region RelOps

        private void HandleTableOpMetadata(ScanTableBaseOp op)
        {
            // add to the list of referenced entitysets
            EntitySet entitySet = op.Table.TableMetadata.Extent as EntitySet;
            if (entitySet != null)
            {
                // If entitySet is an association set, the appropriate entity set references will be registered inside Visit(RefOp, Node).
                AddEntitySetReference(entitySet);
            }

            TypeUsage elementType = TypeUsage.Create(op.Table.TableMetadata.Extent.ElementType);
            // add to the list of structured types
            AddTypeReference(elementType);
        }

        /// <summary>
        /// Visits a "table" expression - performs view expansion on the table (if appropriate), 
        /// and then some additional book-keeping. 
        /// 
        /// The "ofType" and "includeSubtypes" parameters are optional hints for view expansion, allowing
        /// for more customized (and hopefully, more optimal) views. The wasOfTypeSatisfied out parameter
        /// tells whether the ofType filter was already handled by the view expansion, or if the caller still
        /// needs to deal with it.
        /// 
        /// If the "table" is a C-space entityset, then we produce a ScanViewOp 
        /// tree with the defining query as the only child of the ScanViewOp
        /// 
        /// If the table is an S-space entityset, then we still produce a ScanViewOp, but this
        /// time, we produce a simple "select * from BaseTable" as the defining 
        /// query
        /// </summary>
        /// <param name="scanTableNode">the scanTable node tree</param>
        /// <param name="scanTableOp">the scanTableOp</param>
        /// <param name="typeFilter">
        ///     An optional IsOfOp representing a type filter to apply to the scan table; will be set to <c>null</c> 
        ///     if the scan target is expanded to a view that renders the type filter superfluous.
        /// </param>
        /// <returns></returns>
        private Node ProcessScanTable(Node scanTableNode, ScanTableOp scanTableOp, ref IsOfOp typeFilter)
        {
            HandleTableOpMetadata(scanTableOp);

            PlanCompiler.Assert(scanTableOp.Table.TableMetadata.Extent != null, "ScanTableOp must reference a table with an extent");

            Node ret = null;

            //
            // Get simple things out of the way. If we're dealing with an S-space entityset, 
            // simply return the node
            // 
            if (scanTableOp.Table.TableMetadata.Extent.EntityContainer.DataSpace == DataSpace.SSpace)
            {
                return scanTableNode;
            }
            else
            {
                // "Expand" the C-Space view
                ret = ExpandView(scanTableNode, scanTableOp, ref typeFilter);
            }

            // Rerun the processor over the resulting subtree
            ret = VisitNode(ret);

            return ret;
        }

        /// <summary>
        /// Processes a ScanTableOp - simply delegates to ProcessScanTableOp
        /// </summary>
        /// <param name="op">the view op</param>
        /// <param name="n">current node tree</param>
        /// <returns>the transformed view-op</returns>
        public override Node Visit(ScanTableOp op, Node n)
        {
            IsOfOp nullFilter = null;
            return ProcessScanTable(n, op, ref nullFilter);
        }

        /// <summary>
        /// Visitor for a ScanViewOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ScanViewOp op, Node n)
        {
            bool entityTypeScopePushed = false;
            if (op.Table.TableMetadata.Extent.BuiltInTypeKind == BuiltInTypeKind.EntitySet)
            {
                m_entityTypeScopes.Push((EntitySet)op.Table.TableMetadata.Extent);
                entityTypeScopePushed = true;
            }

            HandleTableOpMetadata(op);
            // Ideally, I should call this as the first statement, but that was causing too
            // many test diffs - because of the order in which the entitytypes/sets
            // were being added. There is no semantic difference in calling this here
            VisitRelOpDefault(op, n);

            if (entityTypeScopePushed)
            {
                var scope = m_entityTypeScopes.Pop();
                PlanCompiler.Assert(scope == op.Table.TableMetadata.Extent, "m_entityTypeScopes stack is broken");
            }

            return n;
        }

        /// <summary>
        /// Processing for all JoinOps
        /// </summary>
        /// <param name="op">JoinOp</param>
        /// <param name="n">Current subtree</param>
        /// <returns></returns>
        protected override Node VisitJoinOp(JoinBaseOp op, Node n)
        {
            // Only LeftOuterJoin and InnerJoin are handled by JoinElimination
            if (op.OpType == OpType.InnerJoin || op.OpType == OpType.LeftOuterJoin)
            {
                m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.JoinElimination);
            }

            // If a subquery was added with an exists node, we have to go througth Normalization
            if (base.ProcessJoinOp(op, n))
            {
                m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.Normalization);
            }
            return n;
        }

        /// <summary>
        /// Perform default relop processing; Also "require" the join-elimination phase
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override Node VisitApplyOp(ApplyBaseOp op, Node n)
        {
            m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.JoinElimination);
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Can I eliminate this sort? I can, if the current path is *not* one of the 
        /// following
        ///   TopN(Sort)
        ///   PhysicalProject(Sort)
        /// 
        /// We don't yet handle the TopN variant
        /// </summary>
        /// <returns></returns>
        private bool IsSortUnnecessary()
        {
            Node ancestor = m_ancestors.Peek();
            PlanCompiler.Assert(ancestor != null, "unexpected SortOp as root node?");

            if (ancestor.Op.OpType == OpType.PhysicalProject)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Visit a SortOp. Eliminate it if the path to this node is not one of 
        /// PhysicalProject(Sort) or
        /// TopN(Sort)
        /// 
        /// Otherwise, simply visit the child RelOp
        /// 
        /// </summary>
        /// <param name="op">Current sortOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>possibly transformed subtree</returns>
        public override Node Visit(SortOp op, Node n)
        {
            // can I eliminate this sort
            if (this.IsSortUnnecessary())
            {
                return VisitNode(n.Child0);
            }

            // perform default processing
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Checks to see if this filterOp represents an IS OF (or IS OF ONLY) filter over a ScanTableOp
        /// </summary>
        /// <param name="n">the filterOp node</param>
        /// <param name="ofType">(OUT) the Type to restrict to</param>
        /// <param name="isOfOnly">(OUT) was an ONLY clause specified</param>
        /// <returns></returns>
        private bool IsOfTypeOverScanTable(Node n, out IsOfOp typeFilter)
        {
            typeFilter = null;

            // 
            // Is the predicate an IsOf predicate
            //
            IsOfOp isOfOp = n.Child1.Op as IsOfOp;
            if (isOfOp == null)
            {
                return false;
            }
            //
            // Is the Input RelOp a ScanTableOp
            //
            ScanTableOp scanTableOp = n.Child0.Op as ScanTableOp;
            if (scanTableOp == null || scanTableOp.Table.Columns.Count != 1)
            {
                return false;
            }
            //
            // Is the argument to the IsOfOp the single column of the table?
            //
            VarRefOp varRefOp = n.Child1.Child0.Op as VarRefOp;
            if (varRefOp == null || varRefOp.Var != scanTableOp.Table.Columns[0])
            {
                return false;
            }

            //
            // All conditions match. Return the info from the IsOf predicate
            //
            typeFilter = isOfOp;
            return true;
        }

        /// <summary>
        /// Handler for a FilterOp. Usually delegates to VisitRelOpDefault. 
        /// 
        /// There's one special case - where we have an ISOF predicate over a ScanTable. In that case, we attempt 
        /// to get a more "optimal" view; and return that optimal view
        /// 
        /// </summary>
        /// <param name="op">the filterOp</param>
        /// <param name="n">the node tree</param>
        /// <returns></returns>
        public override Node Visit(FilterOp op, Node n)
        {
            IsOfOp typeFilter;
            if (IsOfTypeOverScanTable(n, out typeFilter))
            {
                Node ret = ProcessScanTable(n.Child0, (ScanTableOp)n.Child0.Op, ref typeFilter);
                if (typeFilter != null)
                {
                    n.Child1 = VisitNode(n.Child1);
                    n.Child0 = ret;
                    ret = n;
                }
                return ret;
            }
            else
            {
                return VisitRelOpDefault(op, n);
            }
        }

        /// <summary>
        /// Visit a ProjectOp; if the input is a SortOp, we pullup the sort over 
        /// the ProjectOp to ensure that we don't have nested sorts;
        /// Note: This transformation cannot be moved in the normalizer, 
        /// because it needs to happen before any subquery augmentation happens. 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ProjectOp op, Node n)
        {
            PlanCompiler.Assert(n.HasChild0, "projectOp without input?");

            if (OpType.Sort == n.Child0.Op.OpType || OpType.ConstrainedSort == n.Child0.Op.OpType)
            {
                SortBaseOp sort = (SortBaseOp)n.Child0.Op;

                // Don't pullup the sort if it doesn't have any keys.
                // An example of such sort is "ctx.Products.Take(1)".
                if (sort.Keys.Count > 0)
                {
                    IList<Node> sortChildren = new List<Node>();
                    sortChildren.Add(n);

                    //A ConstrainedSort has two other children besides the input and it needs to keep them.  
                    for (int i = 1; i < n.Child0.Children.Count; i++)
                    {
                        sortChildren.Add(n.Child0.Children[i]);
                    }

                    // Replace the ProjectOp input (currently the Sort node) with the input to the Sort.
                    n.Child0 = n.Child0.Child0;

                    // Vars produced by the Sort input and used as SortKeys should be considered outputs
                    // of the ProjectOp that now operates over what was the Sort input.
                    foreach (SortKey key in sort.Keys)
                    {
                        op.Outputs.Set(key.Var);
                    }

                    // Finally, pull the Sort over the Project by creating a new Sort node with the original
                    // Sort as its Op and the Project node as its only child. This is sufficient because
                    // the ITreeGenerator ensures that the SortOp does not have any local VarDefs.
                    return VisitNode(m_command.CreateNode(sort, sortChildren));
                }
            }

            // perform default processing
            Node newNode = VisitRelOpDefault(op, n);
            return newNode;
        }

        /// <summary>
        /// Mark AggregatePushdown as needed
        /// </summary>
        /// <param name="op">the groupByInto op</param>
        /// <param name="n">the node tree</param>
        /// <returns></returns>
        public override Node Visit(GroupByIntoOp op, Node n)
        {
            this.m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.AggregatePushdown);
            return base.Visit(op, n);
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Finds the record (Row) types that we're projecting out of the query, and
    /// ensures that we mark them as needing a nullable sentinel, so when we
    /// flatten them later we'll have one added.
    /// </summary>
    internal class StructuredTypeNullabilityAnalyzer : ColumnMapVisitor<HashSet<string>>
    {
        static internal StructuredTypeNullabilityAnalyzer Instance = new StructuredTypeNullabilityAnalyzer();

        /// <summary>
        /// VarRefColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="typesNeedingNullSentinel"></param>
        /// <returns></returns>
        internal override void Visit(VarRefColumnMap columnMap, HashSet<string> typesNeedingNullSentinel)
        {
            AddTypeNeedingNullSentinel(typesNeedingNullSentinel, columnMap.Type);
            base.Visit(columnMap, typesNeedingNullSentinel);
        }

        /// <summary>
        /// Recursively add any Row types to the list of types needing a sentinel.
        /// </summary>
        /// <param name="typesNeedingNullableSentinel"></param>
        /// <param name="typeUsage"></param>
        private static void AddTypeNeedingNullSentinel(HashSet<string> typesNeedingNullSentinel, TypeUsage typeUsage)
        {
            if (TypeSemantics.IsCollectionType(typeUsage))
            {
                AddTypeNeedingNullSentinel(typesNeedingNullSentinel, TypeHelpers.GetElementTypeUsage(typeUsage));
            }
            else
            {
                if (TypeSemantics.IsRowType(typeUsage) || TypeSemantics.IsComplexType(typeUsage))
                {
                    MarkAsNeedingNullSentinel(typesNeedingNullSentinel, typeUsage);
                }
                foreach (EdmMember m in TypeHelpers.GetAllStructuralMembers(typeUsage))
                {
                    AddTypeNeedingNullSentinel(typesNeedingNullSentinel, m.TypeUsage);
                }
            }
        }

        /// <summary>
        /// Marks the given typeUsage as needing a null sentinel. 
        /// Call this method instead of calling Add over the HashSet directly, to ensure consistency.
        /// </summary>
        /// <param name="typesNeedingNullSentinel"></param>
        /// <param name="typeUsage"></param>
        internal static void MarkAsNeedingNullSentinel(HashSet<string> typesNeedingNullSentinel, TypeUsage typeUsage)
        {
            typesNeedingNullSentinel.Add(typeUsage.EdmType.Identity);
        }
    }

}
