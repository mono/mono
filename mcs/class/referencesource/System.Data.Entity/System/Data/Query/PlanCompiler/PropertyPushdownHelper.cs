//---------------------------------------------------------------------
// <copyright file="PropertyPushdownHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
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

using System.Globalization;

using System.Data.Common;
using md = System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{

    /// <summary>
    /// The PropertyPushdownHelper module is a submodule of the StructuredTypeEliminator 
    /// module. It serves as a useful optimization sidekick for NominalTypeEliminator which 
    /// is the real guts of eliminating structured types.
    /// 
    /// The goal of this module is to identify a list of desired properties for each node
    /// (and Var) in the tree that is of a structured type. This list of desired properties
    /// is identified in a top-down push fashion. 
    ///
    /// While it is desirable to get as accurate information as possible, it is unnecessary
    /// for this module to be super-efficient (i.e.) it is ok for it to get a superset
    /// of the appropriate information for each node, but it is absolutely not ok for it
    /// to get a subset. Later phases (projection pruning) can help eliminate unnecessary
    /// information, but the query cannot be made incorrect.
    ///
    /// This module is implemented as a visitor - it leverages information about
    /// types in the query - made possible by the TypeFlattener module - and walks
    /// down the tree pushing properties to each child of a node. It builds two maps:
    /// 
    ///     (*) a node-property map 
    ///     (*) a var-property map
    /// 
    /// Each of these keeps trackof the properties needed from each node/var. 
    /// 
    /// These maps are returned to the caller and will be used by the NominalTypeEliminator 
    /// module to eliminate all structured types.
    /// </summary>
    internal class PropertyPushdownHelper : BasicOpVisitor
    {

        #region private state

        private readonly Dictionary<Node, PropertyRefList> m_nodePropertyRefMap;
        private readonly Dictionary<Var, PropertyRefList> m_varPropertyRefMap;
        private readonly StructuredTypeInfo m_structuredTypeInfo;

        #endregion

        #region constructor

        private PropertyPushdownHelper(StructuredTypeInfo structuredTypeInfo)
        {
            m_structuredTypeInfo = structuredTypeInfo;
            m_varPropertyRefMap = new Dictionary<Var, PropertyRefList>();
            m_nodePropertyRefMap = new Dictionary<Node, PropertyRefList>();
        }

        #endregion

        #region Process Driver

        /// <summary>
        /// The driver.
        /// Walks the tree, and "pushes" down information about required properties
        /// to every node and Var in the tree.
        /// </summary>
        /// <param name="itree">The query tree</param>
        /// <param name="structuredTypeInfo">Type info for structured types appearing in query.</param>
        /// <param name="varPropertyRefs">List of desired properties from each Var</param>
        /// <param name="nodePropertyRefs">List of desired properties from each node</param>
        internal static void Process(Command itree, StructuredTypeInfo structuredTypeInfo, out Dictionary<Var, PropertyRefList> varPropertyRefs, out Dictionary<Node, PropertyRefList> nodePropertyRefs)
        {
            PropertyPushdownHelper pph = new PropertyPushdownHelper(structuredTypeInfo);
            pph.Process(itree.Root);

            varPropertyRefs = pph.m_varPropertyRefMap;
            nodePropertyRefs = pph.m_nodePropertyRefMap;
        }

        /// <summary>
        /// the driver routine. Invokes the visitor, and then returns the collected
        /// info
        /// </summary>
        /// <param name="rootNode">node in the tree to begin processing at</param>
        private void Process(Node rootNode)
        {
            // simply invoke the visitor
            rootNode.Op.Accept(this, rootNode);
        }

        #endregion

        #region private methods

        #region state maintenance

        /// <summary>
        /// Get the list of propertyrefs for a node. If none exists, create an 
        /// empty structure and store it in the map
        /// </summary>
        /// <param name="node">Specific node</param>
        /// <returns>List of properties expected from this node</returns>
        private PropertyRefList GetPropertyRefList(Node node)
        {
            PropertyRefList propRefs;
            if (!m_nodePropertyRefMap.TryGetValue(node, out propRefs))
            {
                propRefs = new PropertyRefList();
                m_nodePropertyRefMap[node] = propRefs;
            }
            return propRefs;
        }

        /// <summary>
        /// Add a list of property references for this node
        /// </summary>
        /// <param name="node">the node</param>
        /// <param name="propertyRefs">list of property references</param>
        private void AddPropertyRefs(Node node, PropertyRefList propertyRefs)
        {
            PropertyRefList refs = GetPropertyRefList(node);
            refs.Append(propertyRefs);
        }

        /// <summary>
        /// Get the list of desired properties for a Var
        /// </summary>
        /// <param name="v">the var</param>
        /// <returns>List of desired properties</returns>
        private PropertyRefList GetPropertyRefList(Var v)
        {
            PropertyRefList propRefs;
            if (!m_varPropertyRefMap.TryGetValue(v, out propRefs))
            {
                propRefs = new PropertyRefList();
                m_varPropertyRefMap[v] = propRefs;
            }
            return propRefs;
        }

        /// <summary>
        /// Add a new set of properties to a Var
        /// </summary>
        /// <param name="v">the var</param>
        /// <param name="propertyRefs">desired properties</param>
        private void AddPropertyRefs(Var v, PropertyRefList propertyRefs)
        {
            PropertyRefList currentRefs = GetPropertyRefList(v);
            currentRefs.Append(propertyRefs);
        }

        #endregion

        #region Visitor Helpers

        /// <summary>
        /// Gets the list of "identity" properties for an entity. Gets the
        /// "entitysetid" property in addition to the "key" properties
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static PropertyRefList GetIdentityProperties(md.EntityType type)
        {
            PropertyRefList desiredProperties = GetKeyProperties(type);
            desiredProperties.Add(EntitySetIdPropertyRef.Instance);
            return desiredProperties;
        }

        /// <summary>
        /// Gets the list of key properties for an entity
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private static PropertyRefList GetKeyProperties(md.EntityType entityType)
        {
            PropertyRefList desiredProperties = new PropertyRefList();
            foreach (md.EdmMember p in entityType.KeyMembers)
            {
                md.EdmProperty edmP = p as md.EdmProperty;
                PlanCompiler.Assert(edmP != null, "EntityType had non-EdmProperty key member?");
                SimplePropertyRef pRef = new SimplePropertyRef(edmP);
                desiredProperties.Add(pRef);
            }
            return desiredProperties;
        }

        #endregion

        /// <summary>
        /// Default visitor for an Op.
        /// 
        /// Simply walks through all children looking for Ops of structured 
        /// types, and asks for all their properties.
        /// </summary>
        /// <remarks>
        /// Several of the ScalarOps take the default handling, to simply ask
        /// for all the children's properties:
        /// 
        ///     AggegateOp
        ///     ArithmeticOp
        ///     CastOp
        ///     ConditionalOp
        ///     ConstantOp
        ///     ElementOp
        ///     ExistsOp
        ///     FunctionOp
        ///     GetRefKeyOp
        ///     LikeOp
        ///     NestAggregateOp
        ///     NewInstanceOp
        ///     NewMultisetOp
        ///     NewRecordOp
        ///     RefOp
        /// 
        /// They do not exist here to eliminate noise.  
        /// 
        /// Note that the NewRecordOp and the NewInstanceOp could be optimized to only 
        /// push down the appropriate references, but it isn't clear to Murali that the 
        /// complexity is worth it.
        /// </remarks>
        /// <param name="n"></param>
        protected override void VisitDefault(Node n)
        {
            // for each child that is a complex type, simply ask for all properties
            foreach (Node chi in n.Children)
            {
                ScalarOp chiOp = chi.Op as ScalarOp;
                if (chiOp != null && TypeUtils.IsStructuredType(chiOp.Type))
                {
                    AddPropertyRefs(chi, PropertyRefList.All);
                }
            }
            VisitChildren(n);
        }

        #region ScalarOps

        /// <summary>
        /// SoftCastOp:
        /// If the input is 
        ///    Ref - ask for all properties
        ///    Entity, ComplexType - ask for the same properties I've been asked for
        ///    Record - ask for all properties (Note: This should be more optimized in the future
        ///        since we can actually "remap" the properties)
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(SoftCastOp op, Node n)
        {
            PropertyRefList childProps = null;

            if (md.TypeSemantics.IsReferenceType(op.Type))
            {
                childProps = PropertyRefList.All;
            }
            else if (md.TypeSemantics.IsNominalType(op.Type))
            {
                PropertyRefList myProps = m_nodePropertyRefMap[n];
                childProps = myProps.Clone();
            }
            else if (md.TypeSemantics.IsRowType(op.Type))
            {
                // 
                // Note: We should do a better job here (by translating  
                // our PropertyRefs to the equivalent property refs on the child
                //
                childProps = PropertyRefList.All;
            }

            if (childProps != null)
            {
                AddPropertyRefs(n.Child0, childProps);
            }
            VisitChildren(n);
        }

        /// <summary>
        /// CaseOp handling
        /// 
        /// Pushes its desired properties to each of the WHEN/ELSE clauses
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(CaseOp op, Node n)
        {
            // First find the properties that my parent expects from me
            PropertyRefList pdProps = GetPropertyRefList(n);

            // push down the same properties to my then/else clauses. 
            // the "when" clauses are irrelevant
            for (int i = 1; i < n.Children.Count - 1; i += 2)
            {
                PropertyRefList cdProps = pdProps.Clone();
                AddPropertyRefs(n.Children[i], cdProps);
            }
            AddPropertyRefs(n.Children[n.Children.Count - 1], pdProps.Clone());

            // Now visit the children
            VisitChildren(n);
        }

        /// <summary>
        /// CollectOp handling. 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(CollectOp op, Node n)
        {
            // Simply visit the children without pushing down any references to them.
            VisitChildren(n);
        }

        /// <summary>
        /// ComparisonOp handling
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(ComparisonOp op, Node n)
        {
            // Check to see if the children are structured types. Furthermore,
            // if the children are of entity types, then all we really need are
            // the key properties (and the entityset property)
            // For record and ref types, simply keep going
            md.TypeUsage childOpType = (n.Child0.Op as ScalarOp).Type;

            if (!TypeUtils.IsStructuredType(childOpType))
            {
                VisitChildren(n);
            }
            else if (md.TypeSemantics.IsRowType(childOpType) || md.TypeSemantics.IsReferenceType(childOpType))
                VisitDefault(n);
            else
            {
                PlanCompiler.Assert(md.TypeSemantics.IsEntityType(childOpType), "unexpected childOpType?");
                PropertyRefList desiredProperties = GetIdentityProperties(TypeHelpers.GetEdmType<md.EntityType>(childOpType));

                // Now push these set of properties to each child
                foreach (Node chi in n.Children)
                    AddPropertyRefs(chi, desiredProperties);

                // Visit the children
                VisitChildren(n);
            }
        }

        /// <summary>
        /// ElementOp handling
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(ElementOp op, Node n)
        {
            // Cannot occur at this stage of processing
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// GetEntityRefOp handling 
        /// 
        /// Ask for the "identity" properties from the input entity, and push that
        /// down to my child
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(GetEntityRefOp op, Node n)
        {
            ScalarOp childOp = n.Child0.Op as ScalarOp;
            PlanCompiler.Assert(childOp != null, "input to GetEntityRefOp is not a ScalarOp?");

            // bug 428542 - the child is of the entity type; not this op
            md.EntityType entityType = TypeHelpers.GetEdmType<md.EntityType>(childOp.Type);

            PropertyRefList desiredProperties = GetIdentityProperties(entityType);
            AddPropertyRefs(n.Child0, desiredProperties);

            VisitNode(n.Child0);
        }

        /// <summary>
        /// IsOfOp handling
        /// 
        /// Simply requests the "typeid" property from
        /// the input. No other property is required
        /// </summary>
        /// <param name="op">IsOf op</param>
        /// <param name="n">Node to visit</param>
        public override void Visit(IsOfOp op, Node n)
        {

            // The only property I need from my child is the typeid property;
            PropertyRefList childProps = new PropertyRefList();
            childProps.Add(TypeIdPropertyRef.Instance);
            AddPropertyRefs(n.Child0, childProps);

            VisitChildren(n);
        }

        /// <summary>
        /// Common handler for RelPropertyOp and PropertyOp. 
        /// Simply pushes down the desired set of properties to the child
        /// </summary>
        /// <param name="op">the *propertyOp</param>
        /// <param name="n">node tree corresponding to the Op</param>
        /// <param name="propertyRef">the property reference</param>
        private void VisitPropertyOp(Op op, Node n, PropertyRef propertyRef)
        {
            PropertyRefList cdProps = new PropertyRefList();
            if (!TypeUtils.IsStructuredType(op.Type))
            {
                cdProps.Add(propertyRef);
            }
            else
            {
                // Get the list of properties my parent expects from me. 
                PropertyRefList pdProps = GetPropertyRefList(n);

                // Ask my child (which is really my container type) for each of these 
                // properties

                // If I've been asked for all my properties, then get the 
                // corresponding flat list of properties from my children.
                // For now, however, simply ask for all properties in this case
                // What we really need to do is to get the "flattened" list of
                // properties from the input, and prepend each of these with
                // our property name. We don't have that info available, so 
                // I'm taking the easier route.
                if (pdProps.AllProperties)
                {
                    cdProps = pdProps;
                }
                else
                {
                    foreach (PropertyRef p in pdProps.Properties)
                    {
                        cdProps.Add(p.CreateNestedPropertyRef(propertyRef));
                    }
                }
            }

            // push down my expectations
            AddPropertyRefs(n.Child0, cdProps);
            VisitChildren(n);
        }

        /// <summary>
        /// RelPropertyOp handling.
        /// Delegates to VisitPropertyOp. Marks the rel-property as required from the
        /// child
        /// </summary>
        /// <param name="op">the RelPropertyOp</param>
        /// <param name="n">node tree corresponding to the op</param>
        public override void Visit(RelPropertyOp op, Node n)
        {
            VisitPropertyOp(op, n, new RelPropertyRef(op.PropertyInfo));
        }

        /// <summary>
        /// PropertyOp handling
        /// 
        /// Pushes down the requested properties along with the current 
        /// property to the child
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(PropertyOp op, Node n)
        {
            VisitPropertyOp(op, n, new SimplePropertyRef(op.PropertyInfo));
        }

        /// <summary>
        /// TreatOp handling
        /// 
        /// Simply passes down "my" desired properties, and additionally
        /// asks for the TypeID property
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(TreatOp op, Node n)
        {
            // First find the properties that my parent expects from me
            PropertyRefList pdProps = GetPropertyRefList(n);

            // Push down each of these, and in addition, push down the typeid property
            // to my child
            PropertyRefList childProps = pdProps.Clone();
            childProps.Add(TypeIdPropertyRef.Instance);
            AddPropertyRefs(n.Child0, childProps);
            VisitChildren(n);
        }

        /// <summary>
        /// VarRefOp handling
        /// 
        /// Simply passes along the current "desired" properties
        /// to the corresponding Var
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(VarRefOp op, Node n)
        {
            if (TypeUtils.IsStructuredType(op.Var.Type))
            {
                // Get the properties that my parent expects from me. 
                PropertyRefList myProps = GetPropertyRefList(n);
                // Add this onto the list of properties expected from the var itself
                AddPropertyRefs(op.Var, myProps);
            }
        }

        #endregion

        #region AncillaryOps

        /// <summary>
        /// VarDefOp handling
        /// 
        /// Pushes the "desired" properties to the 
        /// defining expression
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(VarDefOp op, Node n)
        {
            if (TypeUtils.IsStructuredType(op.Var.Type))
            {
                PropertyRefList myProps = GetPropertyRefList(op.Var);
                // Push this down to the expression defining the var
                AddPropertyRefs(n.Child0, myProps);
            }
            VisitChildren(n);
        }

        /// <summary>
        /// VarDefListOp handling
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(VarDefListOp op, Node n)
        {
            // Simply visit the children without pushing down any references to them.
            VisitChildren(n);
        }

        #endregion

        #region RelOps

        /// <summary>
        /// ApplyOp handling
        /// CrossApplyOp handling
        /// OuterApplyOp handling
        /// 
        /// Handling for all ApplyOps: Process the right child, and then
        /// the left child - since the right child may have references to the 
        /// left
        /// </summary>
        /// <param name="op">apply op</param>
        /// <param name="n"></param>
        protected override void VisitApplyOp(ApplyBaseOp op, Node n)
        {
            VisitNode(n.Child1); // the right input
            VisitNode(n.Child0); // the left input
        }


        /// <summary>
        /// DistinctOp handling
        /// 
        /// Require all properties out of all structured vars
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(DistinctOp op, Node n)
        {
            foreach (Var v in op.Keys)
                if (TypeUtils.IsStructuredType(v.Type))
                {
                    AddPropertyRefs(v, PropertyRefList.All);
                }
            VisitChildren(n);
        }

        /// <summary>
        /// FilterOp handling
        /// 
        /// Process the predicate child, and then the input child - since the 
        /// predicate child will probably have references to the input.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(FilterOp op, Node n)
        {
            VisitNode(n.Child1); // visit predicate first
            VisitNode(n.Child0); // then visit the relop input
        }

        /// <summary>
        /// GroupByOp handling        
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        protected override void VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            // First "request" all properties for every key (that is a structured type)
            foreach (Var v in op.Keys)
            {
                if (TypeUtils.IsStructuredType(v.Type))
                {
                    AddPropertyRefs(v, PropertyRefList.All);
                }
            }

            // Now visit the aggregate definitions, the key definitions, and 
            // the relop input in that order
            VisitChildrenReverse(n);
        }


        /// <summary>
        /// JoinOp handling
        /// CrossJoinOp handling
        /// InnerJoinOp handling
        /// LeftOuterJoinOp handling
        /// FullOuterJoinOp handling
        ///
        /// Handler for all JoinOps. For all joins except cross joins, process 
        /// the predicate first, and then the inputs - the inputs can be processed
        /// in any order.
        /// 
        /// For cross joins, simply process all the (relop) inputs
        /// </summary>
        /// <param name="op">join op</param>
        /// <param name="n"></param>
        protected override void VisitJoinOp(JoinBaseOp op, Node n)
        {
            if (n.Op.OpType == OpType.CrossJoin)
                VisitChildren(n);
            else
            {
                VisitNode(n.Child2); // the predicate first
                VisitNode(n.Child0); // then, the left input
                VisitNode(n.Child1); // the right input
            }
        }

        /// <summary>
        /// ProjectOp handling
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(ProjectOp op, Node n)
        {
            VisitNode(n.Child1); // visit projections first
            VisitNode(n.Child0); // then visit the relop input
        }


        /// <summary>
        /// ScanTableOp handler
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(ScanTableOp op, Node n)
        {
            PlanCompiler.Assert(!n.HasChild0, "scanTableOp with an input?");
        }

        /// <summary>
        /// ScanViewOp 
        /// 
        /// ask for all properties from the view definition
        /// that have currently been requested from the view itself
        /// </summary>
        /// <param name="op">current ScanViewOp</param>
        /// <param name="n">current node</param>
        public override void Visit(ScanViewOp op, Node n)
        {
            PlanCompiler.Assert(op.Table.Columns.Count == 1, "ScanViewOp with multiple columns?");
            Var columnVar = op.Table.Columns[0];
            PropertyRefList columnProps = GetPropertyRefList(columnVar);

            Var inputVar = NominalTypeEliminator.GetSingletonVar(n.Child0);
            PlanCompiler.Assert(inputVar != null, "cannot determine single Var from ScanViewOp's input");

            AddPropertyRefs(inputVar, columnProps.Clone());

            VisitChildren(n);
        }

        /// <summary>
        /// SetOp handling
        /// UnionAllOp handling
        /// IntersectOp handling
        /// ExceptOp handling
        /// 
        /// Visitor for a SetOp. Pushes desired properties to the corresponding 
        /// Vars of the input
        /// </summary>
        /// <param name="op">the setop</param>
        /// <param name="n"></param>
        protected override void VisitSetOp(SetOp op, Node n)
        {
            foreach (VarMap varMap in op.VarMap)
                foreach (KeyValuePair<Var, Var> kv in varMap)
                {
                    if (TypeUtils.IsStructuredType(kv.Key.Type))
                    {
                        // Get the set of expected properties for the unionVar, and 
                        // push it down to the inputvars
                        // For Intersect and ExceptOps, we need all properties 
                        // from the input
                        // We call GetPropertyRefList() always to initialize
                        // the map, even though we may not use it
                        // 
                        PropertyRefList myProps = GetPropertyRefList(kv.Key);
                        if (op.OpType == OpType.Intersect || op.OpType == OpType.Except)
                        {
                            myProps = PropertyRefList.All;
                            // We "want" all properties even on the output of the setop
                            AddPropertyRefs(kv.Key, myProps);
                        }
                        else
                        {
                            myProps = myProps.Clone();
                        }
                        AddPropertyRefs(kv.Value, myProps);
                    }
                }
            VisitChildren(n);
        }

        /// <summary>
        /// SortOp handling 
        /// 
        /// First, "request" that for any sort key that is a structured type, we 
        /// need all its properties. Then process any local definitions, and 
        /// finally the relop input
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        protected override void VisitSortOp(SortBaseOp op, Node n)
        {
            // foreach sort key, every single bit of the Var is needed
            foreach (InternalTrees.SortKey sk in op.Keys)
                if (TypeUtils.IsStructuredType(sk.Var.Type))
                    AddPropertyRefs(sk.Var, PropertyRefList.All);

            // if the sort has any local definitions, process those first
            if (n.HasChild1)
                VisitNode(n.Child1);
            // then process the relop input
            VisitNode(n.Child0);
        }

        /// <summary>
        /// UnnestOp handling
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(UnnestOp op, Node n)
        {
            VisitChildren(n);
        }

        #endregion

        #region PhysicalOps

        /// <summary>
        /// PhysicalProjectOp handling
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(PhysicalProjectOp op, Node n)
        {
            // Insist that we need all properties from all the outputs
            foreach (Var v in op.Outputs)
            {
                if (TypeUtils.IsStructuredType(v.Type))
                {
                    AddPropertyRefs(v, PropertyRefList.All);
                }
            }

            // simply visit the children
            VisitChildren(n);
        }

        /// <summary>
        /// MultiStreamNestOp handling
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(MultiStreamNestOp op, Node n)
        {
            // Cannot occur at this stage of processing
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// SingleStreamNestOp handling
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(SingleStreamNestOp op, Node n)
        {
            // Cannot occur at this stage of processing
            throw EntityUtil.NotSupported();
        }

        #endregion

        #endregion
    }
}
