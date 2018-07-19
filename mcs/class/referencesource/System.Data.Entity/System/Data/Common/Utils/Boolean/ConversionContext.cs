//---------------------------------------------------------------------
// <copyright file="ConversionContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace System.Data.Common.Utils.Boolean
{
    /// <summary>
    /// Manages state used to translate BoolExpr to decision diagram vertices and back again.
    /// Specializations exist for generic and DomainConstraint expressions.
    /// </summary>
    internal abstract class ConversionContext<T_Identifier>
    {
        /// <summary>
        /// Gets the solver instance associated with this conversion context. Used to reterieve
        /// canonical Decision Diagram vertices for this context.
        /// </summary>
        internal readonly Solver Solver = new Solver();

        /// <summary>
        /// Given a term in BoolExpr, returns the corresponding decision diagram vertex.
        /// </summary>
        internal abstract Vertex TranslateTermToVertex(TermExpr<T_Identifier> term);

        /// <summary>
        /// Describes a vertex as a series of literal->vertex successors such that the literal
        /// logically implies the given vertex successor.
        /// </summary>
        internal abstract IEnumerable<LiteralVertexPair<T_Identifier>> GetSuccessors(Vertex vertex);
    }

    /// <summary>
    /// VertexLiteral pair, used for ConversionContext.GetSuccessors
    /// </summary>
    internal sealed class LiteralVertexPair<T_Identifier>
    {
        internal readonly Vertex Vertex;
        internal readonly Literal<T_Identifier> Literal;

        internal LiteralVertexPair(Vertex vertex, Literal<T_Identifier> literal)
        {
            this.Vertex = vertex;
            this.Literal = literal;
        }
    }

    /// <summary>
    /// Generic implementation of a ConversionContext
    /// </summary>
    internal sealed class GenericConversionContext<T_Identifier> : ConversionContext<T_Identifier>
    {
        readonly Dictionary<TermExpr<T_Identifier>, int> _variableMap = new Dictionary<TermExpr<T_Identifier>, int>();
        Dictionary<int, TermExpr<T_Identifier>> _inverseVariableMap;

        internal override Vertex TranslateTermToVertex(TermExpr<T_Identifier> term)
        {
            int variable;
            if (!_variableMap.TryGetValue(term, out variable))
            {
                variable = Solver.CreateVariable();
                _variableMap.Add(term, variable);
            }
            return Solver.CreateLeafVertex(variable, Solver.BooleanVariableChildren);
        }

        internal override IEnumerable<LiteralVertexPair<T_Identifier>> GetSuccessors(Vertex vertex)
        {
            LiteralVertexPair<T_Identifier>[] successors = new LiteralVertexPair<T_Identifier>[2];

            Debug.Assert(2 == vertex.Children.Length);
            Vertex then = vertex.Children[0];
            Vertex @else = vertex.Children[1];

            // get corresponding term expression
            InitializeInverseVariableMap();
            TermExpr<T_Identifier> term = _inverseVariableMap[vertex.Variable];

            // add positive successor (then)
            Literal<T_Identifier> literal = new Literal<T_Identifier>(term, true);
            successors[0] = new LiteralVertexPair<T_Identifier>(then, literal);

            // add negative successor (else)
            literal = literal.MakeNegated();
            successors[1] = new LiteralVertexPair<T_Identifier>(@else, literal);
            return successors;
        }

        private void InitializeInverseVariableMap()
        {
            if (null == _inverseVariableMap)
            {
                _inverseVariableMap = _variableMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            }
        }
    }

    /// <summary>
    /// Specialization of ConversionContext for DomainConstraint BoolExpr
    /// </summary>
    internal sealed class DomainConstraintConversionContext<T_Variable, T_Element> : ConversionContext<DomainConstraint<T_Variable, T_Element>>
    {
        /// <summary>
        /// A map from domain variables to decision diagram variables.
        /// </summary>
        readonly Dictionary<DomainVariable<T_Variable, T_Element>, int> _domainVariableToRobddVariableMap =
            new Dictionary<DomainVariable<T_Variable, T_Element>, int>();
        Dictionary<int, DomainVariable<T_Variable, T_Element>> _inverseMap;

        /// <summary>
        /// Translates a domain constraint term to an N-ary DD vertex. 
        /// </summary>
        internal override Vertex TranslateTermToVertex(TermExpr<DomainConstraint<T_Variable, T_Element>> term)
        {
            var range = term.Identifier.Range;
            var domainVariable = term.Identifier.Variable;
            var domain = domainVariable.Domain;

            if (range.All(element => !domain.Contains(element)))
            {
                // trivially false
                return Vertex.Zero;
            }

            if (domain.All(element => range.Contains(element)))
            {
                // trivially true
                return Vertex.One;
            }

            // determine assignments for this constraints (if the range contains a value in the domain, '1', else '0')
            Vertex[] children = domain.Select(element => range.Contains(element) ? Vertex.One : Vertex.Zero).ToArray();

            // see if we know this variable
            int robddVariable;
            if (!_domainVariableToRobddVariableMap.TryGetValue(domainVariable, out robddVariable))
            {
                robddVariable = Solver.CreateVariable();
                _domainVariableToRobddVariableMap[domainVariable] = robddVariable;
            }

            // create a new vertex with the given assignments
            return Solver.CreateLeafVertex(robddVariable, children);
        }

        internal override IEnumerable<LiteralVertexPair<DomainConstraint<T_Variable, T_Element>>> GetSuccessors(Vertex vertex)
        {
            InitializeInverseMap();
            var domainVariable = _inverseMap[vertex.Variable];

            // since vertex children are ordinally aligned with domain, handle domain as array
            var domain = domainVariable.Domain.ToArray();

            // foreach unique successor vertex, build up range
            Dictionary<Vertex, Set<T_Element>> vertexToRange = new Dictionary<Vertex, Set<T_Element>>();

            for (int i = 0; i < vertex.Children.Length; i++)
            {
                Vertex successorVertex = vertex.Children[i];
                Set<T_Element> range;
                if (!vertexToRange.TryGetValue(successorVertex, out range))
                {
                    range = new Set<T_Element>(domainVariable.Domain.Comparer);
                    vertexToRange.Add(successorVertex, range);
                }
                range.Add(domain[i]);
            }

            foreach (var vertexRange in vertexToRange)
            {
                var successorVertex = vertexRange.Key;
                var range = vertexRange.Value;

                // construct a DomainConstraint including the given range
                var constraint = new DomainConstraint<T_Variable, T_Element>(domainVariable, range.MakeReadOnly());
                var literal = new Literal<DomainConstraint<T_Variable, T_Element>>(
                    new TermExpr<DomainConstraint<T_Variable, T_Element>>(constraint), true);

                yield return new LiteralVertexPair<DomainConstraint<T_Variable, T_Element>>(successorVertex, literal);
            }
        }

        private void InitializeInverseMap()
        {
            if (null == _inverseMap)
            {
                _inverseMap = _domainVariableToRobddVariableMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            }
        }
    }
}
