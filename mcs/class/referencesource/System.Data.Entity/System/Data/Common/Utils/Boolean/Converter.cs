//---------------------------------------------------------------------
// <copyright file="Converter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace System.Data.Common.Utils.Boolean
{

    /// <summary>
    /// Handles conversion of expressions to different forms (decision diagram, etc)
    /// </summary>
    internal sealed class Converter<T_Identifier>
    {
        private readonly Vertex _vertex;
        private readonly ConversionContext<T_Identifier> _context;
        private DnfSentence<T_Identifier> _dnf;
        private CnfSentence<T_Identifier> _cnf;

        internal Converter(BoolExpr<T_Identifier> expr, ConversionContext<T_Identifier> context)
        {
            _context = context ?? IdentifierService<T_Identifier>.Instance.CreateConversionContext();
            _vertex = ToDecisionDiagramConverter<T_Identifier>.TranslateToRobdd(expr, _context);
        }

        internal Vertex Vertex
        {
            get { return _vertex; }
        }

        internal DnfSentence<T_Identifier> Dnf
        {
            get
            {
                InitializeNormalForms();
                return _dnf;
            }
        }

        internal CnfSentence<T_Identifier> Cnf
        {
            get
            {
                InitializeNormalForms();
                return _cnf;
            }
        }

        /// <summary>
        /// Converts the decision diagram (Vertex) wrapped by this converter and translates it into DNF
        /// and CNF forms. I'll first explain the strategy with respect to DNF, and then explain how CNF
        /// is achieved in parallel. A DNF sentence representing the expression is simply a disjunction 
        /// of every rooted path through the decision diagram ending in one. For instance, given the 
        /// following decision diagram:
        /// 
        ///                         A
        ///                       0/ \1
        ///                      B     C
        ///                    0/ \1 0/ \1
        ///                 One   Zero   One
        /// 
        /// the following paths evaluate to 'One'
        /// 
        ///                 !A, !B
        ///                 A, C
        ///     
        /// and the corresponding DNF is (!A.!B) + (A.C)
        /// 
        /// It is easy to compute CNF from the DNF of the negation, e.g.:
        /// 
        ///     !((A.B) + (C.D)) iff. (!A+!B) . (!C+!D)
        ///     
        /// To compute the CNF form in parallel, we negate the expression (by swapping One and Zero sinks)
        /// and collect negation of the literals along the path. In the above example, the following paths
        /// evaluate to 'Zero':
        /// 
        ///                 !A, B
        ///                 A, !C
        ///                 
        /// and the CNF (which takes the negation of all literals in the path) is (!A+B) . (A+!C)
        /// </summary>
        private void InitializeNormalForms()
        {
            if (null == _cnf)
            {
                // short-circuit if the root is true/false
                if (_vertex.IsOne())
                {
                    // And() -> True
                    _cnf = new CnfSentence<T_Identifier>(Set<CnfClause<T_Identifier>>.Empty);
                    // Or(And()) -> True
                    var emptyClause = new DnfClause<T_Identifier>(Set<Literal<T_Identifier>>.Empty);
                    var emptyClauseSet = new Set<DnfClause<T_Identifier>>();
                    emptyClauseSet.Add(emptyClause);
                    _dnf = new DnfSentence<T_Identifier>(emptyClauseSet.MakeReadOnly());
                }
                else if (_vertex.IsZero())
                {
                    // And(Or()) -> False
                    var emptyClause = new CnfClause<T_Identifier>(Set<Literal<T_Identifier>>.Empty);
                    var emptyClauseSet = new Set<CnfClause<T_Identifier>>();
                    emptyClauseSet.Add(emptyClause);
                    _cnf = new CnfSentence<T_Identifier>(emptyClauseSet.MakeReadOnly());
                    // Or() -> False
                    _dnf = new DnfSentence<T_Identifier>(Set<DnfClause<T_Identifier>>.Empty);
                }
                else
                {
                    // construct clauses by walking the tree and constructing a clause for each sink
                    Set<DnfClause<T_Identifier>> dnfClauses = new Set<DnfClause<T_Identifier>>();
                    Set<CnfClause<T_Identifier>> cnfClauses = new Set<CnfClause<T_Identifier>>();
                    Set<Literal<T_Identifier>> path = new Set<Literal<T_Identifier>>();

                    FindAllPaths(_vertex, cnfClauses, dnfClauses, path);

                    _cnf = new CnfSentence<T_Identifier>(cnfClauses.MakeReadOnly());
                    _dnf = new DnfSentence<T_Identifier>(dnfClauses.MakeReadOnly());
                }
            }
        }

        private void FindAllPaths(Vertex vertex, Set<CnfClause<T_Identifier>> cnfClauses, Set<DnfClause<T_Identifier>> dnfClauses,
            Set<Literal<T_Identifier>> path)
        {
            if (vertex.IsOne())
            {
                // create DNF clause
                var clause = new DnfClause<T_Identifier>(path);
                dnfClauses.Add(clause);
            }
            else if (vertex.IsZero())
            {
                // create CNF clause
                var clause = new CnfClause<T_Identifier>(new Set<Literal<T_Identifier>>(path.Select(l => l.MakeNegated())));
                cnfClauses.Add(clause);
            }
            else
            {
                // keep on walking...
                foreach (var successor in _context.GetSuccessors(vertex))
                {
                    path.Add(successor.Literal);
                    FindAllPaths(successor.Vertex, cnfClauses, dnfClauses, path);
                    path.Remove(successor.Literal);
                }
            }
        }
    }
}
