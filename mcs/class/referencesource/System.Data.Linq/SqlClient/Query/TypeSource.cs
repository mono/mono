using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Method used for dealing with dynamic types. The ClrType of SqlNode is the 
    /// statically known type originating in the source expression tree. For methods
    /// like GetType(), we need to know the dynamic type that will be constructed.
    /// </summary>
    internal static class TypeSource {
        private class Visitor : SqlVisitor {
            class UnwrapStack {
                public UnwrapStack(UnwrapStack last, bool unwrap) {
                    Last = last;
                    Unwrap = unwrap;
                }
                public UnwrapStack Last { get; private set; }
                public bool Unwrap { get; private set; }
            }
            UnwrapStack UnwrapSequences;
            internal SqlExpression sourceExpression;
            internal Type sourceType;
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal override SqlNode Visit(SqlNode node) {
                if (node == null)
                    return null;

                sourceExpression = node as SqlExpression;
                if (sourceExpression != null) {
                    Type type = sourceExpression.ClrType;
                    UnwrapStack unwrap = this.UnwrapSequences;
                    while (unwrap != null) {
                        if (unwrap.Unwrap) {
                            type = TypeSystem.GetElementType(type);
                        }
                        unwrap = unwrap.Last;
                    }
                    sourceType = type;
                }
                if (sourceType != null && TypeSystem.GetNonNullableType(sourceType).IsValueType) {
                    return node; // Value types can't also have a dynamic type.
                }
                if (sourceType != null && TypeSystem.HasIEnumerable(sourceType)) {
                    return node; // Sequences can't be polymorphic.
                }

                switch (node.NodeType) {
                    case SqlNodeType.ScalarSubSelect:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.Element:
                    case SqlNodeType.SearchedCase:
                    case SqlNodeType.ClientCase:
                    case SqlNodeType.SimpleCase:
                    case SqlNodeType.Member:
                    case SqlNodeType.DiscriminatedType:
                    case SqlNodeType.New:
                    case SqlNodeType.FunctionCall:
                    case SqlNodeType.MethodCall:
                    case SqlNodeType.Convert: // Object identity does not survive convert. It does survive Cast.
                        // Dig no further.
                        return node;
                    case SqlNodeType.TypeCase:
                        sourceType = ((SqlTypeCase)node).RowType.Type;
                        return node;
                    case SqlNodeType.Link:
                        sourceType = ((SqlLink)node).RowType.Type;
                        return node;
                    case SqlNodeType.Table:
                        sourceType = ((SqlTable)node).RowType.Type;
                        return node;
                    case SqlNodeType.Value:
                        SqlValue val = (SqlValue)node;
                        if (val.Value != null) {
                            // In some cases the ClrType of a Value node may
                            // differ from the actual runtime type of the value.
                            // Therefore, we ensure here that the correct type is set.
                            sourceType = val.Value.GetType();
                        }
                        return node;
                }
                return base.Visit(node);
            }
            internal override SqlSelect VisitSelect(SqlSelect select) {
                /*
                 * We're travelling through <expression> of something like:
                 * 
                 *  SELECT <expression>
                 *  FROM <alias>
                 *
                 * Inside the expression there may be a reference to <alias> that 
                 * represents the dynamic type that we're trying to discover.
                 * 
                 * In this case, the type relationship between AliasRef and Alias is
                 * T to IEnumerable<T>.
                 * 
                 * We need to remember to 'unpivot' the type of IEnumerable<T> to 
                 * get the correct dynamic type.
                 * 
                 * Since SELECTs may be nested, we use a stack of pivots.
                 * 
                 */
                this.UnwrapSequences = new UnwrapStack(this.UnwrapSequences, true);
                VisitExpression(select.Selection);
                this.UnwrapSequences = this.UnwrapSequences.Last;
                return select;
            }
            internal override SqlExpression VisitAliasRef(SqlAliasRef aref) {
                if (this.UnwrapSequences != null && this.UnwrapSequences.Unwrap) {
                    this.UnwrapSequences = new UnwrapStack(this.UnwrapSequences, false);
                    this.VisitAlias(aref.Alias);
                    this.UnwrapSequences = this.UnwrapSequences.Last;
                } else {
                    this.VisitAlias(aref.Alias);
                }
                return aref;
            }
            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                this.VisitColumn(cref.Column); // Travel through column references
                return cref;
            }
        }

        /// <summary>
        /// Get a MetaType that represents the dynamic type of the given node.
        /// </summary>
        internal static MetaType GetSourceMetaType(SqlNode node, MetaModel model) {
            Visitor v = new Visitor();
            v.Visit(node);
            Type type = v.sourceType;
            type = TypeSystem.GetNonNullableType(type); // Emulate CLR's behavior: strip nullability from type.
            return model.GetMetaType(type);
        }

        /// <summary>
        /// Retrieve the expression that will represent the _dynamic_ type of the
        /// given expression. This is either a SqlDiscriminatedType or a SqlValue
        /// of type Type.
        /// </summary>
        internal static SqlExpression GetTypeSource(SqlExpression expr) {
            Visitor v = new Visitor();
            v.Visit(expr);
            return v.sourceExpression;
        }
    }
}
