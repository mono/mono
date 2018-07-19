using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Linq;
using System.Data.Linq.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    // duplicates an expression up until a column or column ref is encountered
    // goes 'deep' through alias ref's
    // assumes that columnizing has been done already
    internal class SqlExpander {
        SqlFactory factory;

        internal SqlExpander(SqlFactory factory) {
            this.factory = factory;
        }

        internal SqlExpression Expand(SqlExpression exp) {
            return (new Visitor(this.factory)).VisitExpression(exp);
        }

        class Visitor : SqlDuplicator.DuplicatingVisitor {
            SqlFactory factory;
            Expression sourceExpression;


            internal Visitor(SqlFactory factory)
                : base(true) {
                this.factory = factory;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                return cref;
            }

            internal override SqlExpression VisitColumn(SqlColumn col) {
                return new SqlColumnRef(col);
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared) {
                return this.VisitExpression(shared.Expression);
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref) {
                return this.VisitExpression(sref.SharedExpression.Expression);
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref) {
                SqlNode node = aref.Alias.Node;
                if (node is SqlTable || node is SqlTableValuedFunctionCall) {
                    return aref;
                }
                SqlUnion union = node as SqlUnion;
                if (union != null) {
                    return this.ExpandUnion(union);
                }
                SqlSelect ss = node as SqlSelect;
                if (ss != null) {
                    return this.VisitExpression(ss.Selection);
                }
                SqlExpression exp = node as SqlExpression;
                if (exp != null)
                    return this.VisitExpression(exp);
                throw Error.CouldNotHandleAliasRef(node.NodeType);
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                return (SqlExpression)new SqlDuplicator().Duplicate(ss);
            }

            internal override SqlNode VisitLink(SqlLink link) {
                SqlExpression expansion = this.VisitExpression(link.Expansion);
                SqlExpression[] exprs = new SqlExpression[link.KeyExpressions.Count];
                for (int i = 0, n = exprs.Length; i < n; i++) {
                    exprs[i] = this.VisitExpression(link.KeyExpressions[i]);
                }
                return new SqlLink(link.Id, link.RowType, link.ClrType, link.SqlType, link.Expression, link.Member, exprs, expansion, link.SourceExpression);
            }

            private SqlExpression ExpandUnion(SqlUnion union) {
                List<SqlExpression> exprs = new List<SqlExpression>(2);
                this.GatherUnionExpressions(union, exprs);
                this.sourceExpression = union.SourceExpression;
                SqlExpression result = this.ExpandTogether(exprs);
                return result;
            }

            private void GatherUnionExpressions(SqlNode node, List<SqlExpression> exprs) {
                SqlUnion union = node as SqlUnion;
                if (union != null) {
                    this.GatherUnionExpressions(union.Left, exprs);
                    this.GatherUnionExpressions(union.Right, exprs);
                }
                else {
                    SqlSelect sel = node as SqlSelect;
                    if (sel != null) {
                        SqlAliasRef aref = sel.Selection as SqlAliasRef;
                        if (aref != null) {
                            this.GatherUnionExpressions(aref.Alias.Node, exprs);
                        }
                        else {
                            exprs.Add(sel.Selection);
                        }
                    }
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            private SqlExpression ExpandTogether(List<SqlExpression> exprs) {
                switch (exprs[0].NodeType) {
                    case SqlNodeType.MethodCall: {
                            SqlMethodCall[] mcs = new SqlMethodCall[exprs.Count];
                            for (int i = 0; i < mcs.Length; ++i) {
                                mcs[i] = (SqlMethodCall)exprs[i];
                            }

                            List<SqlExpression> expandedArgs = new List<SqlExpression>();

                            for (int i = 0; i < mcs[0].Arguments.Count; ++i) {
                                List<SqlExpression> args = new List<SqlExpression>();
                                for (int j = 0; j < mcs.Length; ++j) {
                                    args.Add(mcs[j].Arguments[i]);
                                }
                                SqlExpression expanded = this.ExpandTogether(args);
                                expandedArgs.Add(expanded);
                            }
                            return factory.MethodCall(mcs[0].Method, mcs[0].Object, expandedArgs.ToArray(), mcs[0].SourceExpression);
                        }
                    case SqlNodeType.ClientCase: {
                            // Are they all the same?
                            SqlClientCase[] scs = new SqlClientCase[exprs.Count];
                            scs[0] = (SqlClientCase)exprs[0];
                            for (int i = 1; i < scs.Length; ++i) {
                                scs[i] = (SqlClientCase)exprs[i];
                            }

                            // Expand expressions together.
                            List<SqlExpression> expressions = new List<SqlExpression>();
                            for (int i = 0; i < scs.Length; ++i) {
                                expressions.Add(scs[i].Expression);
                            }
                            SqlExpression expression = this.ExpandTogether(expressions);

                            // Expand individual expressions together.
                            List<SqlClientWhen> whens = new List<SqlClientWhen>();
                            for (int i = 0; i < scs[0].Whens.Count; ++i) {
                                List<SqlExpression> scos = new List<SqlExpression>();
                                for (int j = 0; j < scs.Length; ++j) {
                                    SqlClientWhen when = scs[j].Whens[i];
                                    scos.Add(when.Value);
                                }
                                whens.Add(new SqlClientWhen(scs[0].Whens[i].Match, this.ExpandTogether(scos)));
                            }

                            return new SqlClientCase(scs[0].ClrType, expression, whens, scs[0].SourceExpression);
                        }
                    case SqlNodeType.TypeCase: {
                            // Are they all the same?
                            SqlTypeCase[] tcs = new SqlTypeCase[exprs.Count];
                            tcs[0] = (SqlTypeCase)exprs[0];
                            for (int i = 1; i < tcs.Length; ++i) {
                                tcs[i] = (SqlTypeCase)exprs[i];
                            }

                            // Expand discriminators together.
                            List<SqlExpression> discriminators = new List<SqlExpression>();
                            for (int i = 0; i < tcs.Length; ++i) {
                                discriminators.Add(tcs[i].Discriminator);
                            }
                            SqlExpression discriminator = this.ExpandTogether(discriminators);
                            // Write expanded discriminators back in.
                            for (int i = 0; i < tcs.Length; ++i) {
                                tcs[i].Discriminator = discriminators[i];
                            }
                            // Expand individual type bindings together.
                            List<SqlTypeCaseWhen> whens = new List<SqlTypeCaseWhen>();
                            for (int i = 0; i < tcs[0].Whens.Count; ++i) {
                                List<SqlExpression> scos = new List<SqlExpression>();
                                for (int j = 0; j < tcs.Length; ++j) {
                                    SqlTypeCaseWhen when = tcs[j].Whens[i];
                                    scos.Add(when.TypeBinding);
                                }
                                SqlExpression expanded = this.ExpandTogether(scos);
                                whens.Add(new SqlTypeCaseWhen(tcs[0].Whens[i].Match, expanded));
                            }

                            return factory.TypeCase(tcs[0].ClrType, tcs[0].RowType, discriminator, whens, tcs[0].SourceExpression);
                        }
                    case SqlNodeType.New: {
                            // first verify all are similar client objects...
                            SqlNew[] cobs = new SqlNew[exprs.Count];
                            cobs[0] = (SqlNew)exprs[0];
                            for (int i = 1, n = exprs.Count; i < n; i++) {
                                if (exprs[i] == null || exprs[i].NodeType != SqlNodeType.New)
                                    throw Error.UnionIncompatibleConstruction();
                                cobs[i] = (SqlNew)exprs[1];
                                if (cobs[i].Members.Count != cobs[0].Members.Count)
                                    throw Error.UnionDifferentMembers();
                                for (int m = 0, mn = cobs[0].Members.Count; m < mn; m++) {
                                    if (cobs[i].Members[m].Member != cobs[0].Members[m].Member) {
                                        throw Error.UnionDifferentMemberOrder();
                                    }
                                }
                            }
                            SqlMemberAssign[] bindings = new SqlMemberAssign[cobs[0].Members.Count];
                            for (int m = 0, mn = bindings.Length; m < mn; m++) {
                                List<SqlExpression> mexprs = new List<SqlExpression>();
                                for (int i = 0, n = exprs.Count; i < n; i++) {
                                    mexprs.Add(cobs[i].Members[m].Expression);
                                }
                                bindings[m] = new SqlMemberAssign(cobs[0].Members[m].Member, this.ExpandTogether(mexprs));
                                for (int i = 0, n = exprs.Count; i < n; i++) {
                                    cobs[i].Members[m].Expression = mexprs[i];
                                }
                            }
                            SqlExpression[] arguments = new SqlExpression[cobs[0].Args.Count];
                            for (int m = 0, mn = arguments.Length; m < mn; ++m) {
                                List<SqlExpression> mexprs = new List<SqlExpression>();
                                for (int i = 0, n = exprs.Count; i < n; i++) {
                                    mexprs.Add(cobs[i].Args[m]);
                                }
                                arguments[m] = ExpandTogether(mexprs);
                            }
                            return factory.New(cobs[0].MetaType, cobs[0].Constructor, arguments, cobs[0].ArgMembers, bindings, exprs[0].SourceExpression);
                        }
                    case SqlNodeType.Link: {
                            SqlLink[] links = new SqlLink[exprs.Count];
                            links[0] = (SqlLink)exprs[0];
                            for (int i = 1, n = exprs.Count; i < n; i++) {
                                if (exprs[i] == null || exprs[i].NodeType != SqlNodeType.Link)
                                    throw Error.UnionIncompatibleConstruction();
                                links[i] = (SqlLink)exprs[i];
                                if (links[i].KeyExpressions.Count != links[0].KeyExpressions.Count ||
                                    links[i].Member != links[0].Member ||
                                    (links[i].Expansion != null) != (links[0].Expansion != null))
                                    throw Error.UnionIncompatibleConstruction();
                            }
                            SqlExpression[] kexprs = new SqlExpression[links[0].KeyExpressions.Count];
                            List<SqlExpression> lexprs = new List<SqlExpression>();
                            for (int k = 0, nk = links[0].KeyExpressions.Count; k < nk; k++) {
                                lexprs.Clear();
                                for (int i = 0, n = exprs.Count; i < n; i++) {
                                    lexprs.Add(links[i].KeyExpressions[k]);
                                }
                                kexprs[k] = this.ExpandTogether(lexprs);
                                for (int i = 0, n = exprs.Count; i < n; i++) {
                                    links[i].KeyExpressions[k] = lexprs[i];
                                }
                            }
                            SqlExpression expansion = null;
                            if (links[0].Expansion != null) {
                                lexprs.Clear();
                                for (int i = 0, n = exprs.Count; i < n; i++) {
                                    lexprs.Add(links[i].Expansion);
                                }
                                expansion = this.ExpandTogether(lexprs);
                                for (int i = 0, n = exprs.Count; i < n; i++) {
                                    links[i].Expansion = lexprs[i];
                                }
                            }
                            return new SqlLink(links[0].Id, links[0].RowType, links[0].ClrType, links[0].SqlType, links[0].Expression, links[0].Member, kexprs, expansion, links[0].SourceExpression);
                        }
                    case SqlNodeType.Value: {
                            /*
                            * ExprSet of all literals of the same value reduce to just a single literal.
                            */
                            SqlValue val0 = (SqlValue)exprs[0];
                            for (int i = 1; i < exprs.Count; ++i) {
                                SqlValue val = (SqlValue)exprs[i];
                                if (!object.Equals(val.Value, val0.Value))
                                    return this.ExpandIntoExprSet(exprs);
                            }
                            return val0;
                        }
                    case SqlNodeType.OptionalValue: {
                            if (exprs[0].SqlType.CanBeColumn) {
                                goto default;
                            }
                            List<SqlExpression> hvals = new List<SqlExpression>(exprs.Count);
                            List<SqlExpression> vals = new List<SqlExpression>(exprs.Count);
                            for (int i = 0, n = exprs.Count; i < n; i++) {
                                if (exprs[i] == null || exprs[i].NodeType != SqlNodeType.OptionalValue) {
                                    throw Error.UnionIncompatibleConstruction();
                                }
                                SqlOptionalValue sov = (SqlOptionalValue)exprs[i];
                                hvals.Add(sov.HasValue);
                                vals.Add(sov.Value);
                            }
                            return new SqlOptionalValue(this.ExpandTogether(hvals), this.ExpandTogether(vals));
                        }
                    case SqlNodeType.OuterJoinedValue: {
                            if (exprs[0].SqlType.CanBeColumn) {
                                goto default;
                            }
                            List<SqlExpression> values = new List<SqlExpression>(exprs.Count);
                            for (int i = 0, n = exprs.Count; i < n; i++) {
                                if (exprs[i] == null || exprs[i].NodeType != SqlNodeType.OuterJoinedValue) {
                                    throw Error.UnionIncompatibleConstruction();
                                }
                                SqlUnary su = (SqlUnary)exprs[i];
                                values.Add(su.Operand);
                            }
                            return factory.Unary(SqlNodeType.OuterJoinedValue, this.ExpandTogether(values));
                        }
                    case SqlNodeType.DiscriminatedType: {
                            SqlDiscriminatedType sdt0 = (SqlDiscriminatedType)exprs[0];
                            List<SqlExpression> foos = new List<SqlExpression>(exprs.Count);
                            foos.Add(sdt0.Discriminator);
                            for (int i = 1, n = exprs.Count; i < n; i++) {
                                SqlDiscriminatedType sdtN = (SqlDiscriminatedType)exprs[i];
                                if (sdtN.TargetType != sdt0.TargetType) {
                                    throw Error.UnionIncompatibleConstruction();
                                }
                                foos.Add(sdtN.Discriminator);
                            }
                            return factory.DiscriminatedType(this.ExpandTogether(foos), ((SqlDiscriminatedType)exprs[0]).TargetType);
                        }
                    case SqlNodeType.ClientQuery:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.Element:
                    case SqlNodeType.Grouping:
                        throw Error.UnionWithHierarchy();
                    default:
                        return this.ExpandIntoExprSet(exprs);
                }
            }

            /// <summary>
            /// Expand a set of expressions into a single expr set.
            /// This is typically a fallback when there is no other way to unify a set of expressions.
            /// </summary>
            private SqlExpression ExpandIntoExprSet(List<SqlExpression> exprs) {
                SqlExpression[] rexprs = new SqlExpression[exprs.Count];
                for (int i = 0, n = exprs.Count; i < n; i++) {
                    rexprs[i] = this.VisitExpression(exprs[i]);
                }
                return this.factory.ExprSet(rexprs, this.sourceExpression);
            }
        }
    }
}
