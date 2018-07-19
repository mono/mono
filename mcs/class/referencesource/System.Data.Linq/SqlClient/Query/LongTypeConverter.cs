using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq;

namespace System.Data.Linq.SqlClient {
     
    /// <summary>
    /// Converts expressions of type NText, Text, Image to NVarChar(MAX), VarChar(MAX), VarBinary(MAX)
    /// where necessary. This can only be done on SQL2005, so we add a SqlServerCompatibilityAnnotation
    /// to the changed nodes.
    /// </summary>
    internal class LongTypeConverter {
        Visitor visitor;

        internal LongTypeConverter(SqlFactory sql) {
            this.visitor = new Visitor(sql);
        }

        internal SqlNode AddConversions(SqlNode node, SqlNodeAnnotations annotations) {
            visitor.Annotations = annotations;
            return visitor.Visit(node);
        }

        class Visitor : SqlVisitor {
            SqlFactory sql;
            SqlNodeAnnotations annotations;

            internal SqlNodeAnnotations Annotations {
                set { this.annotations = value; }
            }

            internal Visitor(SqlFactory sql) {
                this.sql = sql;
            }

            private SqlExpression ConvertToMax(SqlExpression expr, ProviderType newType) {
                return sql.UnaryConvert(expr.ClrType, newType, expr, expr.SourceExpression);
            }

            // returns CONVERT(VARCHAR/NVARCHAR/VARBINARY(MAX), expr) if provType is one of Text, NText or Image
            // otherwise just returns expr 
            // changed is true if CONVERT(...(MAX),...) was added
            private SqlExpression ConvertToMax(SqlExpression expr, out bool changed) {
                changed = false;
                if (!expr.SqlType.IsLargeType)
                    return expr;
                ProviderType newType = sql.TypeProvider.GetBestLargeType(expr.SqlType);
                changed = true;
                if (expr.SqlType != newType) {
                    return ConvertToMax(expr, newType);
                }       
                changed = false;
                return expr;
            }

            private void ConvertColumnsToMax(SqlSelect select, out bool changed, out bool containsLongExpressions) {
                SqlRow row = select.Row;
                changed = false;
                containsLongExpressions = false;
                foreach (SqlColumn col in row.Columns) {
                    bool columnChanged;
                    containsLongExpressions = containsLongExpressions || col.SqlType.IsLargeType;
                    col.Expression = ConvertToMax(col.Expression, out columnChanged);
                    changed = changed || columnChanged;     
                }
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                if (select.IsDistinct) {
                    bool changed;
                    bool containsLongExpressions;
                    ConvertColumnsToMax(select, out changed, out containsLongExpressions);
                    if (containsLongExpressions) {
                        this.annotations.Add(select, new SqlServerCompatibilityAnnotation(
                                             Strings.TextNTextAndImageCannotOccurInDistinct(select.SourceExpression), SqlProvider.ProviderMode.Sql2000, SqlProvider.ProviderMode.SqlCE));
                    }

                }
                return base.VisitSelect(select);
            }

            internal override SqlNode VisitUnion(SqlUnion su) {
                bool changedLeft = false;
                bool containsLongExpressionsLeft = false;
                SqlSelect left = su.Left as SqlSelect;
                if (left != null) {
                    ConvertColumnsToMax(left, out changedLeft, out containsLongExpressionsLeft);
                }
                bool changedRight = false;
                bool containsLongExpressionsRight = false;
                SqlSelect right = su.Right as SqlSelect;
                if (right != null) {
                    ConvertColumnsToMax(right, out changedRight, out containsLongExpressionsRight);
                }
                if (!su.All && (containsLongExpressionsLeft || containsLongExpressionsRight)) {
                    // unless the UNION is 'ALL', the server will perform a DISTINCT operation,
                    // which isn't valid for large types (text, ntext, image)
                    this.annotations.Add(su, new SqlServerCompatibilityAnnotation(
                        Strings.TextNTextAndImageCannotOccurInUnion(su.SourceExpression), SqlProvider.ProviderMode.Sql2000, SqlProvider.ProviderMode.SqlCE));
                }
                return base.VisitUnion(su);
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc) {
                if (fc.Name == "LEN") {
                    bool changed;
                    fc.Arguments[0] = ConvertToMax(fc.Arguments[0],out changed);
                    if (fc.Arguments[0].SqlType.IsLargeType) {
                        this.annotations.Add(fc, new SqlServerCompatibilityAnnotation(
                                                   Strings.LenOfTextOrNTextNotSupported(fc.SourceExpression), SqlProvider.ProviderMode.Sql2000));
                    }
                }
                return base.VisitFunctionCall(fc);
            }
        }
    }
}
