//---------------------------------------------------------------------
// <copyright file="BoolExpression.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.CommandTrees;
using System.Data.Common.Utils;
using System.Data.Common.Utils.Boolean;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using BoolDomainConstraint = DomainConstraint<BoolLiteral, Constant>;
    using DomainAndExpr = AndExpr<DomainConstraint<BoolLiteral, Constant>>;
    using DomainBoolExpr = BoolExpr<DomainConstraint<BoolLiteral, Constant>>;
    using DomainFalseExpr = FalseExpr<DomainConstraint<BoolLiteral, Constant>>;
    using DomainNotExpr = NotExpr<DomainConstraint<BoolLiteral, Constant>>;
    using DomainOrExpr = OrExpr<DomainConstraint<BoolLiteral, Constant>>;
    using DomainTermExpr = TermExpr<DomainConstraint<BoolLiteral, Constant>>;
    using DomainTrueExpr = TrueExpr<DomainConstraint<BoolLiteral, Constant>>;


    // This class represents an arbitrary boolean expression
    internal partial class BoolExpression : InternalBase
    {

        #region Constructors
        // effects: Create a boolean expression from a literal value
        internal static BoolExpression CreateLiteral(BoolLiteral literal, MemberDomainMap memberDomainMap)
        {
            DomainBoolExpr expr = literal.GetDomainBoolExpression(memberDomainMap);
            return new BoolExpression(expr, memberDomainMap);
        }

        // effects: Creates a new boolean expression using the memberDomainMap of this expression
        internal BoolExpression Create(BoolLiteral literal)
        {
            DomainBoolExpr expr = literal.GetDomainBoolExpression(m_memberDomainMap);
            return new BoolExpression(expr, m_memberDomainMap);
        }

        // effects: Create a boolean expression of the form "NOT expression"
        internal static BoolExpression CreateNot(BoolExpression expression)
        {
            return new BoolExpression(ExprType.Not, new BoolExpression[] { expression });
        }

        // effects: Create a boolean expression of the form "children[0] AND
        // children[1] AND ..."
        internal static BoolExpression CreateAnd(params BoolExpression[] children)
        {
            return new BoolExpression(ExprType.And, children);
        }

        // effects: Create a boolean expression of the form "children[0] OR
        // children[1] OR ..."
        internal static BoolExpression CreateOr(params BoolExpression[] children)
        {
            return new BoolExpression(ExprType.Or, children);
        }

        internal static BoolExpression CreateAndNot(BoolExpression e1, BoolExpression e2)
        {
            return CreateAnd(e1, CreateNot(e2));
        }

        // effects: Creates a new boolean expression using the memberDomainMap of this expression
        internal BoolExpression Create(DomainBoolExpr expression)
        {
            return new BoolExpression(expression, m_memberDomainMap);
        }

        // effects: Creates a boolean expression corresponding to TRUE (if
        // isTrue is true) or FALSE (if isTrue is false)
        private BoolExpression(bool isTrue)
        {
            if (isTrue)
            {
                m_tree = DomainTrueExpr.Value;
            }
            else
            {
                m_tree = DomainFalseExpr.Value;
            }
        }

        // effects: Given the operation type (AND/OR/NOT) and the relevant number of
        // children, returns the corresponding bool expression
        private BoolExpression(ExprType opType, IEnumerable<BoolExpression> children)
        {
            List<BoolExpression> childList = new List<BoolExpression>(children);
            Debug.Assert(childList.Count > 0);
            // If any child is other than true or false, it will have m_memberDomainMap set
            foreach (BoolExpression child in children)
            {
                if (child.m_memberDomainMap != null)
                {
                    m_memberDomainMap = child.m_memberDomainMap;
                    break;
                }
            }

            switch (opType)
            {
                case ExprType.And:
                    m_tree = new DomainAndExpr(ToBoolExprList(childList));
                    break;
                case ExprType.Or:
                    m_tree = new DomainOrExpr(ToBoolExprList(childList));
                    break;
                case ExprType.Not:
                    Debug.Assert(childList.Count == 1);
                    m_tree = new DomainNotExpr(childList[0].m_tree);
                    break;
                default:
                    Debug.Fail("Unknown expression type");
                    break;
            }
        }

        // effects: Creates a boolean expression based on expr
        internal BoolExpression(DomainBoolExpr expr, MemberDomainMap memberDomainMap)
        {
            m_tree = expr;
            m_memberDomainMap = memberDomainMap;
        }
        #endregion

        #region Fields
        private DomainBoolExpr m_tree; // The actual tree that has the expression
        // Domain map for various member paths - can be null
        private readonly MemberDomainMap m_memberDomainMap;
        private Converter<BoolDomainConstraint> m_converter;

        internal static readonly IEqualityComparer<BoolExpression> EqualityComparer = new BoolComparer();
        internal static readonly BoolExpression True = new BoolExpression(true);
        internal static readonly BoolExpression False = new BoolExpression(false);
        #endregion

        #region Properties
        // requires: this is of the form "True", "Literal" or "Literal AND ... AND Literal".
        // effects: Yields the individual atoms in this (for True does not
        // yield anything)
        internal IEnumerable<BoolExpression> Atoms
        {
            get
            {
                // Create the terms visitor and visit it to get atoms (it
                // ensures that there are no ANDs or NOTs in the expression)
                IEnumerable<DomainTermExpr> atoms = TermVisitor.GetTerms(m_tree, false);
                foreach (DomainTermExpr atom in atoms)
                {
                    yield return new BoolExpression(atom, m_memberDomainMap);
                }
            }
        }

        // effects: Yields all the leaves in this
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal IEnumerable<BoolLiteral> Leaves
        {
            get
            {
                // Create the term visitor and visit it to get terms
                IEnumerable<DomainTermExpr> terms = TermVisitor.GetTerms(m_tree, true);
                foreach (DomainTermExpr term in terms)
                {
                    yield return term.Identifier.Variable.Identifier;
                }
            }
        }


        // effects: if this expression is a boolean expression of type BoolLiteral
        // Returns the literal, else returns null
        internal BoolLiteral AsLiteral
        {
            get
            {
                DomainTermExpr literal = m_tree as DomainTermExpr;
                if (literal == null) { return null; }
                BoolLiteral result = GetBoolLiteral(literal);
                return result;
            }
        }

        // effects: Given a term expression, extracts the BoolLiteral from it
        internal static BoolLiteral GetBoolLiteral(DomainTermExpr term)
        {
            DomainConstraint<BoolLiteral, Constant> domainConstraint = term.Identifier;
            DomainVariable<BoolLiteral, Constant> variable = domainConstraint.Variable;
            return variable.Identifier;
        }

        // effects: Returns true iff this corresponds to the boolean literal "true" 
        internal bool IsTrue
        {
            get
            {
                return m_tree.ExprType == ExprType.True;
            }
        }

        // effects: Returns true iff this corresponds to the boolean literal "false" 
        internal bool IsFalse
        {
            get
            {
                return m_tree.ExprType == ExprType.False;
            }
        }

        // effects: Returns true if the expression always evaluates to true
        internal bool IsAlwaysTrue()
        {
            InitializeConverter();
            return m_converter.Vertex.IsOne();
        }

        // effects: Returns true if there is a possible assignment to
        // variables in this such that the expression evaluates to true
        internal bool IsSatisfiable()
        {
            return !IsUnsatisfiable();
        }

        // effects: Returns true if there is no possible assignment to
        // variables in this such that the expression evaluates to true,
        // i.e., the expression will always evaluate to false
        internal bool IsUnsatisfiable()
        {
            InitializeConverter();
            return m_converter.Vertex.IsZero();
        }

        // effects: Returns the internal tree in this
        internal DomainBoolExpr Tree
        {
            get { return m_tree; }
        }

        internal IEnumerable<DomainConstraint<BoolLiteral, Constant>> VariableConstraints
        {
            get { return LeafVisitor<DomainConstraint<BoolLiteral, Constant>>.GetLeaves(m_tree); }
        }

        internal IEnumerable<DomainVariable<BoolLiteral, Constant>> Variables
        {
            get { return VariableConstraints.Select(domainConstraint => domainConstraint.Variable); }
        }

        internal IEnumerable<MemberRestriction> MemberRestrictions
        {
            get
            {
                foreach (DomainVariable<BoolLiteral, Constant> var in Variables)
                {
                    MemberRestriction variableCondition = var.Identifier as MemberRestriction;
                    if (variableCondition != null)
                    {
                        yield return variableCondition;
                    }
                }
            }
        }

        #endregion

        #region Methods

        // effects: Given a sequence of boolean expressions, yields the
        // corresponding trees in it in the same order
        private IEnumerable<DomainBoolExpr> ToBoolExprList(IEnumerable<BoolExpression> nodes)
        {
            foreach (BoolExpression node in nodes)
            {
                yield return node.m_tree;
            }
        }


        /// <summary>
        /// Whether the boolean expression contains only OneOFTypeConst variables.
        /// </summary>
        internal bool RepresentsAllTypeConditions
        {
            get
            {
                return this.MemberRestrictions.All(var => (var is TypeRestriction));
            }
        }


        internal BoolExpression RemapLiterals(Dictionary<BoolLiteral, BoolLiteral> remap)
        {
            var rewriter = new BooleanExpressionTermRewriter<BoolDomainConstraint, BoolDomainConstraint>(
                //                term => remap[BoolExpression.GetBoolLiteral(term)].GetDomainBoolExpression(m_memberDomainMap));
                delegate(DomainTermExpr term)
                {
                    BoolLiteral newLiteral;
                    return remap.TryGetValue(BoolExpression.GetBoolLiteral(term), out newLiteral) ?
                        newLiteral.GetDomainBoolExpression(m_memberDomainMap) : term;
                });
            return new BoolExpression(m_tree.Accept(rewriter), m_memberDomainMap);
        }

        // effects: Given a boolean expression, modifies requiredSlots
        // to indicate which slots are required to generate the expression
        // projectedSlotMap indicates a mapping from member paths to slot
        // numbers (that need to be checked off in requiredSlots)
        internal virtual void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
        {
            RequiredSlotsVisitor.GetRequiredSlots(m_tree, projectedSlotMap, requiredSlots);
        }

        /// <summary>
        /// Given the <paramref name="blockAlias"/> for the block in which the expression resides, converts the expression into eSQL.
        /// </summary>
        internal StringBuilder AsEsql(StringBuilder builder, string blockAlias)
        {
            return AsEsqlVisitor.AsEsql(m_tree, builder, blockAlias);
        }

        /// <summary>
        /// Given the <paramref name="row"/> for the input, converts the expression into CQT.
        /// </summary>
        internal DbExpression AsCqt(DbExpression row)
        {
            return AsCqtVisitor.AsCqt(m_tree, row);
        }
        
        internal StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool writeRoundtrippingMessage)
        {
            if (writeRoundtrippingMessage)
            {
                builder.AppendLine(Strings.Viewgen_ConfigurationErrorMsg(blockAlias));
                builder.Append("  ");
            }
            return AsUserStringVisitor.AsUserString(m_tree, builder, blockAlias);
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            CompactStringVisitor.ToBuilder(m_tree, builder);
        }

        // effects: Given a mapping from old jointree nodes to new ones,
        // creates a boolean expression from "this" in which the references
        // to old join tree nodes are replaced by references to new nodes
        // from remap (boolean expressions other than constants can contain
        // references to jointree nodes, e.g., "var in values" -- var is a
        // reference to a JoinTreeNode
        internal BoolExpression RemapBool(Dictionary<MemberPath, MemberPath> remap)
        {
            DomainBoolExpr expr = RemapBoolVisitor.RemapExtentTreeNodes(m_tree, m_memberDomainMap, remap);
            return new BoolExpression(expr, m_memberDomainMap);
        }

        // effects: Given a list of bools, returns a list of boolean expressions where each
        // boolean in bools has been ANDed with conjunct
        // CHANGE_Microsoft_IMPROVE: replace with lambda pattern
        internal static List<BoolExpression> AddConjunctionToBools(List<BoolExpression> bools,
                                                                   BoolExpression conjunct)
        {
            List<BoolExpression> result = new List<BoolExpression>();
            // Go through the list -- AND each non-null boolean with conjunct
            foreach (BoolExpression b in bools)
            {
                if (null == b)
                { // unused boolean -- leave as it is
                    result.Add(null);
                }
                else
                {
                    result.Add(CreateAnd(b, conjunct));
                }
            }
            return result;
        }

        private void InitializeConverter()
        {
            if (null != m_converter)
            {
                // already done
                return;
            }

            m_converter = new Converter<BoolDomainConstraint>(m_tree,
                IdentifierService<BoolDomainConstraint>.Instance.CreateConversionContext());
        }

        internal BoolExpression MakeCopy()
        {
            BoolExpression copy = Create(m_tree.Accept(CopyVisitorInstance));
            return copy;
        }
        static readonly CopyVisitor CopyVisitorInstance = new CopyVisitor();
        private class CopyVisitor : BasicVisitor<BoolDomainConstraint> { }

        internal void ExpensiveSimplify()
        {
            if (!IsFinal())
            {
                m_tree = m_tree.Simplify();
                return;
            }

            InitializeConverter();
            m_tree = m_tree.ExpensiveSimplify(out m_converter);
            // this call is needed because the possible values on restriction and TrueFalseLiterals
            // may change and need to be synchronized
            FixDomainMap(m_memberDomainMap);
        }

        internal void FixDomainMap(MemberDomainMap domainMap)
        {
            Debug.Assert(domainMap != null, "Member domain map is not set");
            m_tree = FixRangeVisitor.FixRange(m_tree, domainMap);
        }

        private bool IsFinal()
        {
            // First call simplify to get rid of tautologies and true, false
            // etc. and then collapse the OneOfs
            return (m_memberDomainMap != null && IsFinalVisitor.IsFinal(m_tree));
        }

        #endregion

        #region Comparer class
        // This class compares boolean expressions
        private class BoolComparer : IEqualityComparer<BoolExpression>
        {

            #region IEqualityComparer<BoolExpression> Members
            public bool Equals(BoolExpression left, BoolExpression right)
            {
                // Quick check with references
                if (object.ReferenceEquals(left, right))
                {
                    // Gets the Null and Undefined case as well
                    return true;
                }
                // One of them is non-null at least
                if (left == null || right == null)
                {
                    return false;
                }
                // Both are non-null at this point
                return left.m_tree.Equals(right.m_tree);
            }

            public int GetHashCode(BoolExpression expression)
            {
                return expression.m_tree.GetHashCode();
            }
            #endregion
        }
        #endregion
    }
}
