//---------------------------------------------------------------------
// <copyright file="Propagator.JoinPropagator.JoinPredicateVisitor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
namespace System.Data.Mapping.Update.Internal
{
    internal partial class Propagator
    {
        private partial class JoinPropagator
        {
            /// <summary>
            /// Extracts equi-join properties from a join condition.
            /// </summary>
            /// <remarks>
            /// Assumptions:
            /// <list>
            /// <item>Only conjunctions of equality predicates are supported</item>
            /// <item>Each equality predicate is of the form (left property == right property). The order
            /// is important.</item>
            /// </list>
            /// </remarks>
            private class JoinConditionVisitor : UpdateExpressionVisitor<object>
            {
                #region Constructors
                /// <summary>
                /// Initializes a join predicate visitor. The visitor will populate the given property
                /// lists with expressions describing the left and right hand side of equi-join
                /// sub-clauses.
                /// </summary>
                private JoinConditionVisitor()
                {
                    m_leftKeySelectors = new List<DbExpression>();
                    m_rightKeySelectors = new List<DbExpression>();
                }
                #endregion

                #region Fields
                private readonly List<DbExpression> m_leftKeySelectors;
                private readonly List<DbExpression> m_rightKeySelectors;
                private static readonly string s_visitorName = typeof(JoinConditionVisitor).FullName;
                #endregion

                #region Properties
                override protected string VisitorName
                {
                    get { return s_visitorName; }
                }
                #endregion

                #region Methods
                #region Static helper methods
                /// <summary>
                /// Determine properties from the left and right inputs to an equi-join participating
                /// in predicate.
                /// </summary>
                /// <remarks>
                /// The property definitions returned are 'aligned'. If the join predicate reads:
                /// <code>
                /// a = b AND c = d AND e = f
                /// </code>
                /// then the output is as follows:
                /// <code>
                /// leftProperties = {a, c, e}
                /// rightProperties = {b, d, f}
                /// </code>
                /// See Walker class for an explanation of this coding pattern.
                /// </remarks>
                static internal void GetKeySelectors(DbExpression joinCondition, out ReadOnlyCollection<DbExpression> leftKeySelectors, out ReadOnlyCollection<DbExpression> rightKeySelectors)
                {
                    EntityUtil.CheckArgumentNull(joinCondition, "joinCondition");

                    // Constructs a new predicate visitor, which implements a visitor for expression nodes
                    // and returns no values. This visitor instead builds up a list of properties as leaves
                    // of the join predicate are visited.
                    JoinConditionVisitor visitor = new JoinConditionVisitor();

                    // Walk the predicate using the predicate visitor.
                    joinCondition.Accept(visitor);

                    // Retrieve properties discovered visiting predicate leaf nodes.
                    leftKeySelectors = visitor.m_leftKeySelectors.AsReadOnly();
                    rightKeySelectors = visitor.m_rightKeySelectors.AsReadOnly();

                    Debug.Assert(leftKeySelectors.Count == rightKeySelectors.Count,
                        "(Update/JoinPropagator) The equi-join must have an equal number of left and right properties");
                }
                #endregion

                #region Visitor implementation
                /// <summary>
                /// Visit and node after its children have visited. There is nothing to do here
                /// because only leaf equality nodes contain properties extracted by this visitor.
                /// </summary>
                /// <param name="node">And expression node</param>
                /// <returns>Results ignored by this visitor implementation.</returns>
                public override object Visit(DbAndExpression node)
                {
                    EntityUtil.CheckArgumentNull(node, "node");

                    Visit(node.Left);
                    Visit(node.Right);

                    return null;
                }

                /// <summary>
                /// Perform work for an equality expression node.
                /// </summary>
                /// <param name="node">Equality expresion node</param>
                /// <returns>Results ignored by this visitor implementation.</returns>
                public override object Visit(DbComparisonExpression node)
                {
                    EntityUtil.CheckArgumentNull(node, "node");

                    if (DbExpressionKind.Equals == node.ExpressionKind)
                    {
                        m_leftKeySelectors.Add(node.Left);
                        m_rightKeySelectors.Add(node.Right);
                        return null;
                    }
                    else
                    {
                        throw ConstructNotSupportedException(node);
                    }
                }
                #endregion
                #endregion
            }
        }
    }
}
