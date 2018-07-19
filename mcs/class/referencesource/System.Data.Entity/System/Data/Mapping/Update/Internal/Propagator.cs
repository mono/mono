//---------------------------------------------------------------------
// <copyright file="Propagator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping.Update.Internal
{
    using System.Data.Common.CommandTrees;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// <para>
    /// Comments assume there is a map between the CDM and store. Other maps are possible, but
    /// for simplicity, we discuss the 'from' portion of the map as the C-Space and the 'to' portion
    /// of the map as the S-Space.
    /// </para>
    /// <para>
    /// This class translates C-Space change requests into S-Space change requests given a C-Space change
    /// request, an update view loader, and a target table. It has precisely one entry
    /// point, the static <see cref="Propagate"/> method. It performs the translation by evaluating an update
    /// mapping view w.r.t. change requests (propagating a change request through the view).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements propagation rules for the following relational operators in the update mapping 
    /// view:
    /// </para>
    /// <list>
    /// <item>Projection</item>
    /// <item>Selection (filter)</item>
    /// <item>Union all</item>
    /// <item>Inner equijoin</item>
    /// <item>Left outer equijoin</item>
    /// </list>
    /// </remarks>
    internal partial class Propagator : UpdateExpressionVisitor<ChangeNode>
    {
        #region Constructors
        /// <summary>
        /// Construct a new propagator.
        /// </summary>
        /// <param name="parent">UpdateTranslator supporting retrieval of changes for C-Space
        /// extents referenced in the update mapping view.</param>
        /// <param name="table">Table for which updates are being produced.</param>
        private Propagator(UpdateTranslator parent, EntitySet table)
        {
            // Initialize propagator state.
            EntityUtil.CheckArgumentNull(parent, "parent");
            EntityUtil.CheckArgumentNull(table, "table");

            m_updateTranslator = parent;
            m_table = table;
        }
        #endregion

        #region Fields
        private readonly UpdateTranslator m_updateTranslator;
        private readonly EntitySet m_table;
        private static readonly string s_visitorName = typeof(Propagator).FullName;
        #endregion

        #region Properties
        /// <summary>
        /// Gets context for updates performed by this propagator.
        /// </summary>
        internal UpdateTranslator UpdateTranslator
        {
            get { return m_updateTranslator; }
        }

        override protected string VisitorName
        {
            get { return s_visitorName; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Propagate changes from C-Space (contained in <paramref name="parent" /> to the S-Space.
        /// </summary>
        /// <remarks>
        /// See Walker class for an explanation of this coding pattern.
        /// </remarks>
        /// <param name="parent">Grouper supporting retrieval of changes for C-Space
        /// extents referenced in the update mapping view.</param>
        /// <param name="table">Table for which updates are being produced.</param>
        /// <param name="umView">Update mapping view to propagate.</param>
        /// <returns>Changes in S-Space.</returns>
        static internal ChangeNode Propagate(UpdateTranslator parent, EntitySet table, DbQueryCommandTree umView)
        {
            // Construct a new instance of a propagator, which implements a visitor interface
            // for expression nodes (nodes in the update mapping view) and returns changes nodes
            // (seeded by C-Space extent changes returned by the grouper).
            DbExpressionVisitor<ChangeNode> propagator = new Propagator(parent, table);

            // Walk the update mapping view using the visitor pattern implemented in this class.
            // The update mapping view describes the S-Space table we're targeting, so the result
            // returned for the root of view corresponds to changes propagated to the S-Space. 
            return umView.Query.Accept(propagator);
        }

        /// <summary>
        /// Utility method constructs a new empty change node.
        /// </summary>
        /// <param name="node">Update mapping view node associated with the change.</param>
        /// <returns>Empty change node with the appropriate type for the view node.</returns>
        private static ChangeNode BuildChangeNode(DbExpression node)
        {
            TypeUsage nodeType = node.ResultType;
            TypeUsage elementType = MetadataHelper.GetElementType(nodeType);
            return new ChangeNode(elementType);
        }

        #region Visitor implementation

        public override ChangeNode Visit(DbCrossJoinExpression node)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Strings.Update_UnsupportedJoinType(node.ExpressionKind));
        }

        /// <summary>
        /// Propagates changes across a join expression node by implementing progation rules w.r.t. inputs
        /// from the left- and right- hand sides of the join. The work is actually performed
        /// by the <see cref="JoinPropagator" />.
        /// </summary>
        /// <param name="node">A join expression node.</param>
        /// <returns>Results propagated to the given join expression node.</returns>
        public override ChangeNode Visit(DbJoinExpression node)
        {
            EntityUtil.CheckArgumentNull(node, "node");

            if (DbExpressionKind.InnerJoin != node.ExpressionKind && DbExpressionKind.LeftOuterJoin != node.ExpressionKind)
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.Update_UnsupportedJoinType(node.ExpressionKind));
            }

            // There are precisely two inputs to the join which we treat as the left and right children.
            DbExpression leftExpr = node.Left.Expression;
            DbExpression rightExpr = node.Right.Expression;

            // Get the results of propagating changes to the left and right inputs to the join.
            ChangeNode left = Visit(leftExpr);
            ChangeNode right = Visit(rightExpr);

            // Construct a new join propagator, passing in the left and right results, the actual
            // join expression, and this parent propagator.
            JoinPropagator evaluator = new JoinPropagator(left, right, node, this);
            
            // Execute propagation.
            ChangeNode result = evaluator.Propagate();

            return result;
        }

        /// <summary>
        /// Given the results returned for the left and right inputs to a union, propagates changes
        /// through the union.
        /// 
        /// Propagation rule (U = union node, L = left input, R = right input, D(x) = deleted rows
        /// in x, I(x) = inserted rows in x)
        /// 
        /// U = L union R
        /// D(U) = D(L) union D(R)
        /// I(U) = I(L) union I(R)
        /// </summary>
        /// <param name="node">Union expression node in the update mapping view.</param>
        /// <returns>Result of propagating changes to this union all node.</returns>
        public override ChangeNode Visit(DbUnionAllExpression node)
        {
            EntityUtil.CheckArgumentNull(node, "node");

            // Initialize an empty change node result for the union all node
            ChangeNode result = BuildChangeNode(node);

            // Retrieve result of propagating changes to the left and right children.
            ChangeNode left = Visit(node.Left);
            ChangeNode right = Visit(node.Right);

            // Implement insertion propagation rule I(U) = I(L) union I(R)
            result.Inserted.AddRange(left.Inserted);
            result.Inserted.AddRange(right.Inserted);

            // Implement deletion progation rule D(U) = D(L) union D(R)
            result.Deleted.AddRange(left.Deleted);
            result.Deleted.AddRange(right.Deleted);
            
            // The choice of side for the placeholder is arbitrary, since CQTs enforce type compatibility 
            // for the left and right hand sides of the union.
            result.Placeholder = left.Placeholder;

            return result;
        }

        /// <summary>
        /// Propagate projection.
        /// 
        /// Propagation rule (P = projection node, S = projection input, D(x) = deleted rows in x,
        /// I(x) = inserted rows in x)
        /// 
        /// P = Proj_f S
        /// D(P) = Proj_f D(S)
        /// I(P) = Proj_f I(S)
        /// </summary>
        /// <param name="node">Projection expression node.</param>
        /// <returns>Result of propagating changes to the projection expression node.</returns>
        public override ChangeNode Visit(DbProjectExpression node)
        {
            EntityUtil.CheckArgumentNull(node, "node");

            // Initialize an empty change node result for the projection node.
            ChangeNode result = BuildChangeNode(node);

            // Retrieve result of propagating changes to the input of the projection.
            ChangeNode input = Visit(node.Input.Expression);

            // Implement propagation rule for insert I(P) = Proj_f I(S)
            foreach(PropagatorResult row in input.Inserted)
            {
                result.Inserted.Add(Project(node, row, result.ElementType));
            }

            // Implement propagation rule for delete D(P) = Proj_f D(S)
            foreach(PropagatorResult row in input.Deleted)
            {
                result.Deleted.Add(Project(node, row, result.ElementType));
            }

            // Generate a placeholder for the projection node by projecting values in the
            // placeholder for the input node.
            result.Placeholder = Project(node, input.Placeholder, result.ElementType);

            return result;
        }

        /// <summary>
        /// Performs projection for a single row. Evaluates each projection argument against the specified 
        /// row, returning a result with the specified type.
        /// </summary>
        /// <param name="node">Projection expression.</param>
        /// <param name="row">Row to project.</param>
        /// <param name="resultType">Type of the projected row.</param>
        /// <returns>Projected row.</returns>
        private PropagatorResult Project(DbProjectExpression node, PropagatorResult row, TypeUsage resultType)
        {
            EntityUtil.CheckArgumentNull(node, "node");

            Debug.Assert(null != node.Projection, "CQT validates DbProjectExpression.Projection property");

            DbNewInstanceExpression projection = node.Projection as DbNewInstanceExpression;

            if (null == projection)
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.Update_UnsupportedProjection(node.Projection.ExpressionKind));
            }

            // Initialize empty structure containing space for every element of the projection.
            PropagatorResult[] projectedValues = new PropagatorResult[projection.Arguments.Count];

            // Extract value from the input row for every projection argument requested.
            for (int ordinal = 0; ordinal < projectedValues.Length; ordinal++)
            {
                projectedValues[ordinal] = Evaluator.Evaluate(projection.Arguments[ordinal], row, this);
            }

            // Return a new row containing projected values.
            PropagatorResult projectedRow = PropagatorResult.CreateStructuralValue(projectedValues, (StructuralType)resultType.EdmType, false);

            return projectedRow;
        }

        /// <summary>
        /// Propagation rule (F = filter node, S = input to filter, I(x) = rows inserted
        /// into x, D(x) = rows deleted from x, Sigma_p = filter predicate)
        /// 
        /// F = Sigma_p S
        /// D(F) = Sigma_p D(S)
        /// I(F) = Sigma_p I(S)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override ChangeNode Visit(DbFilterExpression node)
        {
            EntityUtil.CheckArgumentNull(node, "node");

            // Initialize an empty change node for this filter node.
            ChangeNode result = BuildChangeNode(node);

            // Retrieve result of propagating changes to the input of the filter.
            ChangeNode input = Visit(node.Input.Expression);

            // Implement insert propagation rule I(F) = Sigma_p I(S)
            result.Inserted.AddRange(Evaluator.Filter(node.Predicate, input.Inserted, this));

            // Implement delete propagation rule D(F) = Sigma_p D(S
            result.Deleted.AddRange(Evaluator.Filter(node.Predicate, input.Deleted, this));

            // The placeholder for a filter node is identical to that of the input, which has an 
            // identical shape (type).
            result.Placeholder = input.Placeholder;

            return result;
        }

        /// <summary>
        /// Handles extent expressions (these are the terminal nodes in update mapping views). This handler
        /// retrieves the changes from the grouper.
        /// </summary>
        /// <param name="node">Extent expression node</param>
        /// <returns></returns>
        public override ChangeNode Visit(DbScanExpression node)
        {
            EntityUtil.CheckArgumentNull(node, "node");
            
            // Gets modifications requested for this extent from the grouper.
            EntitySetBase extent = node.Target;
            ChangeNode extentModifications = UpdateTranslator.GetExtentModifications(extent);

            if (null == extentModifications.Placeholder)
            {
                // Bootstrap placeholder (essentially a record for the extent populated with default values).
                extentModifications.Placeholder = ExtentPlaceholderCreator.CreatePlaceholder(extent, UpdateTranslator);
            }

            return extentModifications;
        }
        #endregion
        #endregion
    }
}
