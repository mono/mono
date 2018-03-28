using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Linq;
using System.Data.Linq.Provider;
using System.Linq;

namespace System.Data.Linq.SqlClient {

    internal class SqlTopReducer {

        internal static SqlNode Reduce(SqlNode node, SqlNodeAnnotations annotations, SqlFactory sql) {
            return new Visitor(annotations, sql).Visit(node);
        }

        class Visitor : SqlVisitor {
            SqlNodeAnnotations annotations;
            SqlFactory sql;

            internal Visitor(SqlNodeAnnotations annotations, SqlFactory sql) {
                this.annotations = annotations;
                this.sql = sql;
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                base.VisitSelect(select);
                if (select.Top != null) {
                    if (select.Top.NodeType == SqlNodeType.Value) {
                        SqlValue val = (SqlValue)select.Top;
                        // convert to literal value for SQL2K compatibility
                        if (val.IsClientSpecified) {
                            select.Top = sql.Value(val.ClrType, val.SqlType, val.Value, false, val.SourceExpression);
                        }
                    }
                    else {
                        // cannot be converted to literal value. note that this select is not SQL2K compatible
                        this.annotations.Add(select.Top, new SqlServerCompatibilityAnnotation(Strings.SourceExpressionAnnotation(select.Top.SourceExpression), SqlProvider.ProviderMode.Sql2000));
                    }
                }
                return select;
            }
        }
    }
}
