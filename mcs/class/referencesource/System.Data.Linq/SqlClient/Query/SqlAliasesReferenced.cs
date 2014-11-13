using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient
{
    /// <summary>
    /// Find referenced Aliases within a node.
    /// </summary>
    internal static class SqlAliasesReferenced
    {
        /// <summary>
        /// Private visitor which walks the tree and looks for referenced aliases.
        /// </summary>
        private class Visitor : SqlVisitor {
            internal IEnumerable<SqlAlias> aliases;
            internal bool referencesAnyMatchingAliases = false;

            internal override SqlNode Visit(SqlNode node) {
                // Short-circuit when the answer is alreading known
                if (this.referencesAnyMatchingAliases) {
                    return node;
                }
                return base.Visit(node);
            }

            internal SqlAlias VisitAliasConsumed(SqlAlias a) {
                if (a == null)
                    return a;

                bool match = false;
                foreach (SqlAlias alias in aliases)
                    if (alias == a) {
                        match = true;
                        break;
                    }

                if (match) {
                    this.referencesAnyMatchingAliases = true;
                }

                return a;
            }

            internal override SqlExpression VisitColumn(SqlColumn col) {
                VisitAliasConsumed(col.Alias);
                VisitExpression(col.Expression);
                return col;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                VisitAliasConsumed(cref.Column.Alias);
                VisitExpression(cref.Column.Expression);
                return cref;
            }
        }

        /// <summary>
        /// Returns true iff the given node references any aliases the list of 'aliases'.
        /// </summary>
        internal static bool ReferencesAny(SqlNode node, IEnumerable<SqlAlias> aliases) {
            Visitor visitor = new Visitor();
            visitor.aliases = aliases;
            visitor.Visit(node);
            return visitor.referencesAnyMatchingAliases;
        }
    }
}
