//---------------------------------------------------------------------
// <copyright file="OpCopier.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

// Interesting cases: Unnest
// More generally, note that any subtree that is left-correlated will stay as such.
//

namespace System.Data.Query.InternalTrees
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Handles copying of operators
    /// </summary>
    internal class OpCopier : BasicOpVisitorOfNode
    {
        #region (pseudo) Public API
        internal static Node Copy(Command cmd, Node n)
        {
            VarMap varMap;
            return Copy(cmd, n, out varMap);
        }

        /// <summary>
        /// Make a copy of the current node. Also return an ordered list of the new
        /// Vars corresponding to the vars in "varList"
        /// </summary>
        /// <param name="cmd">current command</param>
        /// <param name="node">the node to clone</param>
        /// <param name="varList">list of Vars</param>
        /// <param name="newVarList">list of "new" Vars</param>
        /// <returns>the cloned node</returns>
        internal static Node Copy(Command cmd, Node node, VarList varList, out VarList newVarList)
        {
            VarMap varMap;
            Node newNode = Copy(cmd, node, out varMap);
            newVarList = Command.CreateVarList();
            foreach (Var v in varList)
            {
                Var newVar = varMap[v];
                newVarList.Add(newVar);
            }
            return newNode;
        }

        internal static Node Copy(Command cmd, Node n, out VarMap varMap)
        {
            OpCopier oc = new OpCopier(cmd);
            Node newNode = oc.CopyNode(n);
            varMap = oc.m_varMap;
            return newNode;
        }

        internal static List<SortKey> Copy(Command cmd, List<SortKey> sortKeys)
        {
            OpCopier oc = new OpCopier(cmd);
            return oc.Copy(sortKeys);
        }
        #endregion

        // WARNING
        // Everything below this line should be local to this class
        // WARNING

        #region Private State
        private Command m_srcCmd;
        protected Command m_destCmd;
        // Map of var to cloned Var
        protected VarMap m_varMap;
        #endregion

        #region Constructors (private)
        /// <summary>
        /// Constructor. Allows for cloning of nodes within the same command
        /// </summary>
        /// <param name="cmd">The command</param>
        protected OpCopier(Command cmd) : this(cmd, cmd) {}

        /// <summary>
        /// Constructor. Allows for cloning of nodes across commands
        /// </summary>
        /// <param name="destCommand">The Command to which Nodes to be cloned must belong</param>
        /// <param name="sourceCommand">The Command to which cloned Nodes will belong</param>
        private OpCopier(Command destCommand, Command sourceCommand)
        {
            m_srcCmd = sourceCommand;
            m_destCmd = destCommand;
            m_varMap = new VarMap();
        }
        #endregion

        #region Private State Management

        /// <summary>
        /// Get the "cloned" var for a given Var.
        /// If no cloned var exists, return the input Var itself
        /// </summary>
        /// <param name="v">The Var for which the cloned Var should be retrieved</param>
        /// <returns>The cloned Var that corresponds to the specified Var if this OpCopier is cloning across two different Commands; otherwise it is safe to return the specified Var itself</returns>
        private Var GetMappedVar(Var v)
        {
            Var mappedVar;

            //
            // Return a mapping if there is one
            //
            if (m_varMap.TryGetValue(v, out mappedVar))
            {
                return mappedVar;
            }

            //
            // No mapping found.
            // If we're cloning to a different command, this is an error
            //
            if (m_destCmd != m_srcCmd)
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UnknownVar, 6); 
            }

            //
            // otherwise return the current Var itself
            //
            return v;
        }

        /// <summary>
        /// Set the "cloned" var for a given Var
        /// WARNING: If a mapping already exists, an exception is raised
        /// </summary>
        /// <param name="v">The original Var</param>
        /// <param name="mappedVar">The cloned Var</param>
        private void SetMappedVar(Var v, Var mappedVar)
        {
            m_varMap.Add(v, mappedVar);
        }

        /// <summary>
        /// Maps columns of an existing table to those of the cloned table
        /// </summary>
        /// <param name="newTable">The original Table</param>
        /// <param name="oldTable">The cloned Table</param>
        private void MapTable(Table newTable, Table oldTable)
        {
            // Map the corresponding columns of the table
            // Now set up the column map
            for (int i = 0; i < oldTable.Columns.Count; i++)
            {
                SetMappedVar(oldTable.Columns[i], newTable.Columns[i]);
            }
        }

        /// <summary>
        /// Produce the "mapped" Vars for each Var in the input sequence, while
        /// preserving the original order
        /// </summary>
        /// <param name="vars">input var sequence</param>
        /// <returns>output mapped vars</returns>
        private IEnumerable<Var> MapVars(IEnumerable<Var> vars)
        {
            foreach (Var v in vars)
            {
                Var mappedVar = GetMappedVar(v);
                yield return mappedVar;
            }
        }

        /// <summary>
        /// Create a mapped varvec. A new varvec that "maps" all the Vars from
        /// the original Varvec
        /// </summary>
        /// <param name="vars">the varvec to clone</param>
        /// <returns>a mapped varvec</returns>
        private VarVec Copy(VarVec vars)
        {
            VarVec newVarVec = m_destCmd.CreateVarVec(MapVars(vars));
            return newVarVec;
        }

        /// <summary>
        /// Create a mapped copy of the input VarList - each var from the input varlist
        /// is represented by its mapped var (and in exactly the same order) in the output
        /// varlist
        /// </summary>
        /// <param name="varList">varList to map</param>
        /// <returns>mapped varlist</returns>
        private VarList Copy(VarList varList)
        {
            VarList newVarList = Command.CreateVarList(MapVars(varList));
            return newVarList;
        }

        /// <summary>
        /// Copies a sortkey
        /// </summary>
        /// <param name="sortKey">The SortKey to clone</param>
        /// <returns>A new SortKey that is a clone of sortKey</returns>
        private SortKey Copy(SortKey sortKey)
        {
            return Command.CreateSortKey(
                GetMappedVar(sortKey.Var),
                sortKey.AscendingSort,
                sortKey.Collation
            );
        }

        /// <summary>
        /// Copies a list of Sortkeys
        /// </summary>
        /// <param name="sortKeys">The list of SortKeys</param>
        /// <returns>A new list containing clones of the specified SortKeys</returns>
        private List<SortKey> Copy(List<SortKey> sortKeys)
        {
            List<SortKey> newSortKeys = new List<SortKey>();
            foreach (SortKey k in sortKeys)
            {
                newSortKeys.Add(Copy(k));
            }
            return newSortKeys;
        }

        #endregion

        #region Visitor Helpers

        /// <summary>
        /// Simple wrapper for all copy operations
        /// </summary>
        /// <param name="n">The Node to copy</param>
        /// <returns>A new Node that is a copy of the specified Node</returns>
        protected Node CopyNode(Node n)
        {
            return n.Op.Accept<Node>(this, n);
        }

        /// <summary>
        /// Copies all the Child Nodes of the specified Node
        /// </summary>
        /// <param name="n">The Node for which the child Nodes should be copied</param>
        /// <returns>A new list containing copies of the specified Node's children</returns>
        private List<Node> ProcessChildren(Node n)
        {
            List<Node> children = new List<Node>();
            foreach (Node chi in n.Children)
            {
                children.Add(CopyNode(chi));
            }
            return children;
        }

        /// <summary>
        /// Creates a new Node with the specified Op as its Op and the result of visiting the specified Node's children as its children
        /// </summary>
        /// <param name="op">The Op that the new Node should reference</param>
        /// <param name="original">The Node for which the children should be visited and the resulting cloned Nodes used as the children of the new Node returned by this method</param>
        /// <returns>A new Node with the specified Op as its Op and the cloned child Nodes as its children</returns>
        private Node CopyDefault(Op op, Node original)
        {
            return m_destCmd.CreateNode(op, ProcessChildren(original));
        }
        #endregion

        #region IOpVisitor<Node> Members

        /// <summary>
        /// Default Visitor pattern method for unrecognized Ops
        /// </summary>
        /// <param name="op">The unrecognized Op</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>This method always throws NotSupportedException</returns>
        /// <exception cref="NotSupportedException">By design to indicate that the Op was not recognized and is therefore unsupported</exception>
        public override Node Visit(Op op, Node n)
        {
            throw new NotSupportedException(System.Data.Entity.Strings.Iqt_General_UnsupportedOp(op.GetType().FullName));
        }

        #region ScalarOps

        /// <summary>
        /// Copies a ConstantOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ConstantOp op, Node n)
        {
            ConstantBaseOp newOp = m_destCmd.CreateConstantOp(op.Type, op.Value);
            return m_destCmd.CreateNode(newOp);
        }

        /// <summary>
        /// Copies a NullOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(NullOp op, Node n)
        {
            return m_destCmd.CreateNode(m_destCmd.CreateNullOp(op.Type));
        }

        /// <summary>
        /// Copies a ConstantPredicateOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ConstantPredicateOp op, Node n)
        {
            return m_destCmd.CreateNode(m_destCmd.CreateConstantPredicateOp(op.Value));
        }

        /// <summary>
        /// Copies an InternalConstantOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(InternalConstantOp op, Node n)
        {
            InternalConstantOp newOp = m_destCmd.CreateInternalConstantOp(op.Type, op.Value);
            return m_destCmd.CreateNode(newOp);
        }

        /// <summary>
        /// Copies a NullSentinelOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(NullSentinelOp op, Node n)
        {
            NullSentinelOp newOp = m_destCmd.CreateNullSentinelOp();
            return m_destCmd.CreateNode(newOp);
        }

        /// <summary>
        /// Copies a FunctionOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(FunctionOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateFunctionOp(op.Function), n);
        }

        /// <summary>
        /// Copies a PropertyOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(PropertyOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreatePropertyOp(op.PropertyInfo), n);
        }

        /// <summary>
        /// Copies a RelPropertyOp
        /// </summary>
        /// <param name="op">the RelPropertyOp to copy</param>
        /// <param name="n">node tree corresponding to 'op'</param>
        /// <returns>a copy of the node tree</returns>
        public override Node Visit(RelPropertyOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateRelPropertyOp(op.PropertyInfo), n);
        }

        /// <summary>
        /// Copies a CaseOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(CaseOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateCaseOp(op.Type), n);
        }

        /// <summary>
        /// Copies a ComparisonOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ComparisonOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateComparisonOp(op.OpType), n);
        }

        /// <summary>
        /// Copies a like-op
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(LikeOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateLikeOp(), n);
        }

        /// <summary>
        /// Clone an aggregateop
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(AggregateOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateAggregateOp(op.AggFunc, op.IsDistinctAggregate), n);
        }

        /// <summary>
        /// Copies a type constructor
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(NewInstanceOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateNewInstanceOp(op.Type), n);
        }

        /// <summary>
        /// Copies a NewEntityOp
        /// </summary>
        /// <param name="op">the NewEntityOp to copy</param>
        /// <param name="n">node tree corresponding to the NewEntityOp</param>
        /// <returns>a copy of the node tree</returns>
        public override Node Visit(NewEntityOp op, Node n)
        {
            NewEntityOp opCopy;
            if (op.Scoped)
            {
                opCopy = m_destCmd.CreateScopedNewEntityOp(op.Type, op.RelationshipProperties, op.EntitySet);
            }
            else
            {
                Debug.Assert(op.EntitySet == null, "op.EntitySet must be null for the constructor that hasn't been scoped yet.");
                opCopy = m_destCmd.CreateNewEntityOp(op.Type, op.RelationshipProperties);
            }
            return CopyDefault(opCopy, n);
        }

        /// <summary>
        /// Copies a discriminated type constructor
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>        
        public override Node Visit(DiscriminatedNewEntityOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateDiscriminatedNewEntityOp(op.Type, op.DiscriminatorMap, op.EntitySet, op.RelationshipProperties), n);
        }

        /// <summary>
        /// Copies a multiset constructor
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(NewMultisetOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateNewMultisetOp(op.Type), n);
        }

        /// <summary>
        /// Copies a record constructor
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(NewRecordOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateNewRecordOp(op.Type), n);
        }

        /// <summary>
        /// Copies a RefOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(RefOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateRefOp(op.EntitySet, op.Type), n);
        }

        /// <summary>
        /// Copies a VarRefOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(VarRefOp op, Node n)
        {
            // Look up the newVar.
            // If no var is available in the map, that implies that the Var is defined
            // outside this subtree (and it is therefore safe to use it).
            Var newVar;
            if (!m_varMap.TryGetValue(op.Var, out newVar))
                newVar = op.Var;
            // no children for a VarRef
            return m_destCmd.CreateNode(m_destCmd.CreateVarRefOp(newVar));
        }

        /// <summary>
        /// Copies a ConditionalOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ConditionalOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateConditionalOp(op.OpType), n);
        }

        /// <summary>
        /// Copies an ArithmeticOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ArithmeticOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateArithmeticOp(op.OpType, op.Type), n);
        }

        /// <summary>
        /// Copies a TreatOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(TreatOp op, Node n)
        {
            TreatOp newTreatOp = op.IsFakeTreat ? m_destCmd.CreateFakeTreatOp(op.Type) : m_destCmd.CreateTreatOp(op.Type);
            return CopyDefault(newTreatOp, n);
        }

        /// <summary>
        /// Copies a CastOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(CastOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateCastOp(op.Type), n);
        }

        /// <summary>
        /// Copies a SoftCastOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(SoftCastOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateSoftCastOp(op.Type), n);
        }

        /// <summary>
        /// Copies a DerefOp
        /// </summary>
        /// <param name="op">the derefOp to copy</param>
        /// <param name="n">the subtree</param>
        /// <returns>a copy of the subtree</returns>
        public override Node Visit(DerefOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateDerefOp(op.Type), n);
        }

        /// <summary>
        /// Copies a NavigateOp
        /// </summary>
        /// <param name="op">the NavigateOp</param>
        /// <param name="n">the subtree</param>
        /// <returns>a copy of the subtree</returns>
        public override Node Visit(NavigateOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateNavigateOp(op.Type, op.RelProperty), n);
        }

        /// <summary>
        /// Clone an IsOfOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(IsOfOp op, Node n)
        {
            if (op.IsOfOnly)
                return CopyDefault(m_destCmd.CreateIsOfOnlyOp(op.IsOfType), n);
            else
                return CopyDefault(m_destCmd.CreateIsOfOp(op.IsOfType), n);
        }

        /// <summary>
        /// Clone an ExistsOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ExistsOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateExistsOp(), n);
        }

        /// <summary>
        /// Clone an ElementOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ElementOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateElementOp(op.Type), n);
        }

        /// <summary>
        /// Copies a GetRefKeyOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(GetRefKeyOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateGetRefKeyOp(op.Type), n);
        }

        /// <summary>
        /// Copies a GetEntityRefOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(GetEntityRefOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateGetEntityRefOp(op.Type), n);
        }

        /// <summary>
        /// Copies a CollectOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(CollectOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateCollectOp(op.Type), n);
        }

        #endregion

        #region RelOps

        /// <summary>
        /// Copies a ScanTableOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ScanTableOp op, Node n)
        {
            // First create a new ScanTableOp based on the metadata of the existing Op
            ScanTableOp newScan = m_destCmd.CreateScanTableOp(op.Table.TableMetadata);
            // Map the corresponding tables/columns
            MapTable(newScan.Table, op.Table);

            // Create the new node
            Debug.Assert(!n.HasChild0);
            return m_destCmd.CreateNode(newScan);
        }

        /// <summary>
        /// Copies a ScanViewOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ScanViewOp op, Node n)
        {
            // First create a new ScanViewOp based on the metadata of the existing Op
            ScanViewOp newScan = m_destCmd.CreateScanViewOp(op.Table.TableMetadata);
            // Map the corresponding tables/columns
            MapTable(newScan.Table, op.Table);

            // Create the new node
            Debug.Assert(n.HasChild0);
            List<Node> children = ProcessChildren(n);
            return m_destCmd.CreateNode(newScan, children);
        }

        /// <summary>
        /// Clone an UnnestOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(UnnestOp op, Node n)
        {
            // Visit the Node's children and map their Vars
            List<Node> children = ProcessChildren(n);

            // Get the mapped unnest-var
            Var mappedVar = GetMappedVar(op.Var);

            // Create a new unnestOp
            Table newTable = m_destCmd.CreateTableInstance(op.Table.TableMetadata);
            UnnestOp newUnnest = m_destCmd.CreateUnnestOp(mappedVar, newTable);

            // Map the corresponding tables/columns
            MapTable(newUnnest.Table, op.Table);

            // create the unnest node
            return m_destCmd.CreateNode(newUnnest, children);
        }

        /// <summary>
        /// Copies a ProjectOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ProjectOp op, Node n)
        {
            // Visit the Node's children and map their Vars
            List<Node> children = ProcessChildren(n);

            // Copy the ProjectOp's VarSet
            VarVec newVarSet = Copy(op.Outputs);

            // Create a new ProjectOp based on the copied VarSet
            ProjectOp newProject = m_destCmd.CreateProjectOp(newVarSet);

            // Return a new Node that references the copied ProjectOp and has the copied child Nodes as its children
            return m_destCmd.CreateNode(newProject, children);
        }

        /// <summary>
        /// Copies a filterOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(FilterOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateFilterOp(), n);
        }

        /// <summary>
        /// Copies a sort node
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(SortOp op, Node n)
        {
            // Visit the Node's children and map their Vars
            List<Node> children = ProcessChildren(n);

            // Copy the SortOp's SortKeys
            List<SortKey> newSortKeys = Copy(op.Keys);

            // Create a new SortOp that uses the copied SortKeys
            SortOp newSortOp = m_destCmd.CreateSortOp(newSortKeys);

            // Return a new Node that references the copied SortOp and has the copied child Nodes as its children
            return m_destCmd.CreateNode(newSortOp, children);
        }

        /// <summary>
        /// Copies a constrained sort node
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ConstrainedSortOp op, Node n)
        {
            // Visit the Node's children and map their Vars
            List<Node> children = ProcessChildren(n);

            // Copy the ConstrainedSortOp's SortKeys
            List<SortKey> newSortKeys = Copy(op.Keys);

            // Create a new ConstrainedSortOp that uses the copied SortKeys and the original Op's WithTies value
            ConstrainedSortOp newSortOp = m_destCmd.CreateConstrainedSortOp(newSortKeys, op.WithTies);

            // Return a new Node that references the copied SortOp and has the copied child Nodes as its children
            return m_destCmd.CreateNode(newSortOp, children);
        }

        /// <summary>
        /// Copies a group-by node
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(GroupByOp op, Node n)
        {
            // Visit the Node's children and map their Vars
            List<Node> children = ProcessChildren(n);

            // Create a new GroupByOp that uses copies of the Key and Output VarSets of the original GroupByOp
            GroupByOp newGroupOp = m_destCmd.CreateGroupByOp(Copy(op.Keys), Copy(op.Outputs));

            // Return a new Node that references the copied GroupByOp and has the copied child Nodes as its children
            return m_destCmd.CreateNode(newGroupOp, children);
        }

        /// <summary>
        /// Copies a group by into node
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(GroupByIntoOp op, Node n)
        {
            // Visit the Node's children and map their Vars
            List<Node> children = ProcessChildren(n);

            // Create a new GroupByOp that uses copies of the Key and Output VarSets of the original GroupByOp
            GroupByIntoOp newGroupOp = m_destCmd.CreateGroupByIntoOp(Copy(op.Keys), Copy(op.Inputs),  Copy(op.Outputs));

            // Return a new Node that references the copied GroupByOp and has the copied child Nodes as its children
            return m_destCmd.CreateNode(newGroupOp, children);
        }

        /// <summary>
        /// Copies a CrossJoinOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(CrossJoinOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateCrossJoinOp(), n);
        }

        /// <summary>
        /// Copies an InnerJoinOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(InnerJoinOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateInnerJoinOp(), n);
        }

        /// <summary>
        /// Copies a LeftOuterJoinOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(LeftOuterJoinOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateLeftOuterJoinOp(), n);
        }

        /// <summary>
        /// Copies a FullOuterJoinOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(FullOuterJoinOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateFullOuterJoinOp(), n);
        }

        /// <summary>
        /// Copies a crossApplyOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(CrossApplyOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateCrossApplyOp(), n);
        }

        /// <summary>
        /// Clone an OuterApplyOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(OuterApplyOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateOuterApplyOp(), n);
        }

        /// <summary>
        /// Common copy path for all SetOps
        /// </summary>
        /// <param name="op">The SetOp to Copy (must be one of ExceptOp, IntersectOp, UnionAllOp)</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        private Node CopySetOp(SetOp op, Node n)
        {
            // Visit the Node's children and map their Vars
            List<Node> children = ProcessChildren(n);

            VarMap leftMap = new VarMap();
            VarMap rightMap = new VarMap();

            
            foreach (KeyValuePair<Var, Var> kv in op.VarMap[0])
            {
                // Create a new output Var that is a copy of the original output Var
                Var outputVar = m_destCmd.CreateSetOpVar(kv.Key.Type);

                // Add a mapping for the new output var we've just created
                SetMappedVar(kv.Key, outputVar);

                // Add this output var's entries to the new VarMaps
                leftMap.Add(outputVar, GetMappedVar(kv.Value));
                rightMap.Add(outputVar, GetMappedVar((op.VarMap[1])[kv.Key]));
            }

            SetOp newSetOp = null;
            switch(op.OpType)
            {
                case OpType.UnionAll:
                    {
                        Var branchDiscriminator = ((UnionAllOp)op).BranchDiscriminator;
                        if (null != branchDiscriminator) 
                        {
                            branchDiscriminator = GetMappedVar(branchDiscriminator);
                        }
                        newSetOp = m_destCmd.CreateUnionAllOp(leftMap, rightMap, branchDiscriminator);
                    }
                    break;

                case OpType.Intersect:
                    {
                        newSetOp = m_destCmd.CreateIntersectOp(leftMap, rightMap);
                    }
                    break;

                case OpType.Except:
                    {
                        newSetOp = m_destCmd.CreateExceptOp(leftMap, rightMap);
                    }
                    break;

                default:
                    {
                        Debug.Assert(false, "Unexpected SetOpType");
                    }
                    break;
            }

            return m_destCmd.CreateNode(newSetOp, children);
        }

        /// <summary>
        /// Copies a UnionAllOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(UnionAllOp op, Node n)
        {
            return CopySetOp(op, n);
        }

        /// <summary>
        /// Copies an IntersectOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(IntersectOp op, Node n)
        {
            return CopySetOp(op, n);
        }

        /// <summary>
        /// Copies an ExceptOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(ExceptOp op, Node n)
        {
            return CopySetOp(op, n);
        }

        /// <summary>
        /// Copies a DistinctOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(DistinctOp op, Node n)
        {
            // Visit the Node's children and map their Vars
            List<Node> children = ProcessChildren(n);

            // Copy the DistinctOp's Keys
            VarVec newDistinctKeys = Copy(op.Keys);

            // Create a new DistinctOp that uses the copied keys
            DistinctOp newDistinctOp = m_destCmd.CreateDistinctOp(newDistinctKeys);

            // Return a new Node that references the copied DistinctOp and has the copied child Nodes as its children
            return m_destCmd.CreateNode(newDistinctOp, children);
        }

        public override Node Visit(SingleRowOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateSingleRowOp(), n);
        }

        public override Node Visit(SingleRowTableOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateSingleRowTableOp(), n);
        }

        #endregion

        #region AncillaryOps
        /// <summary>
        /// Copies a VarDefOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(VarDefOp op, Node n)
        {
            // First create a new Var
            List<Node> children = ProcessChildren(n);
            Debug.Assert(op.Var.VarType == VarType.Computed, "Unexpected VarType");
            Var newVar = m_destCmd.CreateComputedVar(op.Var.Type);
            SetMappedVar(op.Var, newVar);
            return m_destCmd.CreateNode(m_destCmd.CreateVarDefOp(newVar), children);
        }

        /// <summary>
        /// Copies a VarDefListOp
        /// </summary>
        /// <param name="op">The Op to Copy</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns>A copy of the original Node that references a copy of the original Op</returns>
        public override Node Visit(VarDefListOp op, Node n)
        {
            return CopyDefault(m_destCmd.CreateVarDefListOp(), n);
        }
        #endregion

        #region RulePatternOps
        #endregion

        #region PhysicalOps
        private ColumnMap Copy(ColumnMap columnMap)
        {
            return ColumnMapCopier.Copy(columnMap, m_varMap);
        }

        /// <summary>
        /// Copies a PhysicalProjectOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(PhysicalProjectOp op, Node n)
        {
            // Visit the Node's children and map their Vars
            List<Node> children = ProcessChildren(n);

            // Copy the ProjectOp's VarSet
            VarList newVarList = Copy(op.Outputs);

            SimpleCollectionColumnMap newColumnMap = Copy(op.ColumnMap) as SimpleCollectionColumnMap;
            Debug.Assert(newColumnMap != null, "Coping of a physical project's columnMap did not return a SimpleCollectionColumnMap" );
            // Create a new ProjectOp based on the copied VarSet
            PhysicalProjectOp newProject = m_destCmd.CreatePhysicalProjectOp(newVarList, newColumnMap);

            // Return a new Node that references the copied ProjectOp and has the copied child Nodes as its children
            return m_destCmd.CreateNode(newProject, children);
        }

        private Node VisitNestOp(Node n)
        {
            NestBaseOp op = n.Op as NestBaseOp;
            SingleStreamNestOp ssnOp = op as SingleStreamNestOp;
            Debug.Assert(op != null);

            // Visit the Node's children and map their Vars
            List<Node> newChildren = ProcessChildren(n);

            Var newDiscriminator = null;
            if (ssnOp != null)
            {
                newDiscriminator = GetMappedVar(ssnOp.Discriminator);
            }
            List<CollectionInfo> newCollectionInfoList = new List<CollectionInfo>();
            foreach (CollectionInfo ci in op.CollectionInfo)
            {
                ColumnMap newColumnMap = Copy(ci.ColumnMap);

                Var newCollectionVar = m_destCmd.CreateComputedVar(ci.CollectionVar.Type);
                SetMappedVar(ci.CollectionVar, newCollectionVar);

                VarList newFlattendElementVars = Copy(ci.FlattenedElementVars);
                VarVec newKeys = Copy(ci.Keys);
                List<SortKey> newSortKeys = Copy(ci.SortKeys);
                CollectionInfo newCollectionInfo = Command.CreateCollectionInfo(newCollectionVar, newColumnMap, newFlattendElementVars, newKeys, newSortKeys, ci.DiscriminatorValue);
                newCollectionInfoList.Add(newCollectionInfo);
            }

            VarVec newOutputs = Copy(op.Outputs);

            NestBaseOp newOp = null;
            List<SortKey> newPrefixSortKeys = Copy(op.PrefixSortKeys);
            if (ssnOp != null)
            {
                VarVec newKeys = Copy(ssnOp.Keys);
                // Copy the SortOp's SortKeys
                List<SortKey> newPostfixSortKeys = Copy(ssnOp.PostfixSortKeys);
                newOp = m_destCmd.CreateSingleStreamNestOp(newKeys, newPrefixSortKeys, newPostfixSortKeys, newOutputs, newCollectionInfoList, newDiscriminator);
            }
            else
            {
                newOp = m_destCmd.CreateMultiStreamNestOp(newPrefixSortKeys, newOutputs, newCollectionInfoList);
            }

            return m_destCmd.CreateNode(newOp, newChildren);
        }

        /// <summary>
        /// Copies a singleStreamNestOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(SingleStreamNestOp op, Node n)
        {
            return VisitNestOp(n);
        }

        /// <summary>
        /// Copies a multiStreamNestOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(MultiStreamNestOp op, Node n)
        {
            return VisitNestOp(n);
        }
        #endregion

        #endregion
    }
}
