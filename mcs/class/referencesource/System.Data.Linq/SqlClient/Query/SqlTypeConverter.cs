using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// The standard SQL text for a type conversion is CONVERT([newtype],value)
    /// Replace it with special SQL functions where necessary (e.g. char -> int uses UNICODE(value) instead).
    /// </summary>
    internal class SqlTypeConverter : SqlVisitor {
        protected SqlFactory sql;

        internal SqlTypeConverter(SqlFactory sql) {
            this.sql = sql;
        }

        bool StringConversionIsSafe(ProviderType oldSqlType, ProviderType newSqlType) {
            // if we are dealing with a conversion from a fixed-size string or char
            if (BothTypesAreStrings(oldSqlType, newSqlType)) {
                    // we assume we can convert to an unknown size
                    // we can do the conversion when both sizes are specified and the destination size is larger
                return !newSqlType.HasSizeOrIsLarge || OldWillFitInNew(oldSqlType, newSqlType);
            }

            // give the benefit of the doubt for conversion from non-string types
            return true;
        }

        bool StringConversionIsNeeded(ProviderType oldSqlType, ProviderType newSqlType) {
            if (BothTypesAreStrings(oldSqlType, newSqlType)) {
                bool stringsFixedSize = oldSqlType.IsFixedSize || newSqlType.IsFixedSize;

                if (!newSqlType.HasSizeOrIsLarge) {
                    // we assume we can convert to an unknown size
                    return true;
                }
                else if (OldWillFitInNew(oldSqlType, newSqlType)) {
                    // we can do the conversion when both sizes are specified and the destination size is larger
                    // but we only need to do it when one is fixed size
                    return stringsFixedSize;
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        private bool OldWillFitInNew(ProviderType oldSqlType, ProviderType newSqlType) {
            bool result = newSqlType.IsLargeType                // we can fit into a large type
                || !newSqlType.HasSizeOrIsLarge                 // if the type is not large, and doesn't have a size specified, assume OK
                || (!oldSqlType.IsLargeType                     // else, if the old type isn't large
                    && oldSqlType.HasSizeOrIsLarge              // and both old ..
                    && newSqlType.HasSizeOrIsLarge              // .. and new sizes are specified
                    && newSqlType.Size >= oldSqlType.Size);     // and if the new size is larger or equal to the old, then OK

            return result;
        }

        private bool BothTypesAreStrings(ProviderType oldSqlType, ProviderType newSqlType) {
            bool result = oldSqlType.IsSameTypeFamily(sql.TypeProvider.From(typeof(string)))
                && newSqlType.IsSameTypeFamily(sql.TypeProvider.From(typeof(string)));

            return result;
        }

        internal override SqlExpression VisitUnaryOperator(SqlUnary uo) {
            uo.Operand = this.VisitExpression(uo.Operand);
            if (uo.NodeType != SqlNodeType.Convert) {
                return uo;
            }
            ProviderType oldSqlType = uo.Operand.SqlType;
            ProviderType newSqlType = uo.SqlType;
            Type oldClrType = TypeSystem.GetNonNullableType(uo.Operand.ClrType);
            Type newClrType = TypeSystem.GetNonNullableType(uo.ClrType);

            if (newClrType == typeof(char)) {
                if (oldClrType == typeof(bool)) {
                    throw Error.ConvertToCharFromBoolNotSupported();
                }

                if (oldSqlType.IsNumeric) {
                    // numeric --> char
                    return sql.FunctionCall(uo.ClrType, "NCHAR", new SqlExpression[] { uo.Operand }, uo.SourceExpression);
                }

                if (StringConversionIsSafe(oldSqlType, newSqlType)) {
                    if (StringConversionIsNeeded(oldSqlType, newSqlType)) {
                        // set the new size to the (potentially smaller) oldSqlType.Size
                        uo.SetSqlType(sql.TypeProvider.From(uo.ClrType, oldSqlType.HasSizeOrIsLarge ? oldSqlType.Size : (int?)null));
                    }
                } else {
                    throw Error.UnsafeStringConversion(oldSqlType.ToQueryString(), newSqlType.ToQueryString());
                }
            } else if (oldClrType == typeof(char) && (oldSqlType.IsChar || oldSqlType.IsString) && newSqlType.IsNumeric) {
                // char --> int 
                return sql.FunctionCall(newClrType, sql.TypeProvider.From(typeof(int)), "UNICODE", new SqlExpression[] { uo.Operand }, uo.SourceExpression);
            } else if (newClrType == typeof(string)) {
                if (oldClrType == typeof(double)) {
                    // use longer format if it was a double in the CLR expression
                    return ConvertDoubleToString(uo.Operand, uo.ClrType);
                } else if (oldClrType == typeof(bool)) {
                    // use 'true' or 'false' if it was a bool in the CLR expression
                    return ConvertBitToString(uo.Operand, uo.ClrType);
                } else if (StringConversionIsSafe(oldSqlType, newSqlType)) {
                    if (StringConversionIsNeeded(oldSqlType, newSqlType)) {
                        // set the new size to the (potentially smaller) oldSqlType.Size
                        uo.SetSqlType(sql.TypeProvider.From(uo.ClrType, oldSqlType.HasSizeOrIsLarge ? oldSqlType.Size : (int?)null));
                    }
                } else {
                    throw Error.UnsafeStringConversion(oldSqlType.ToQueryString(), newSqlType.ToQueryString());
                }
            }
            return uo;
        }

        private SqlExpression ConvertDoubleToString(SqlExpression expr, Type resultClrType) {
            // for double we need the form CONVERT(NVARCHAR(30),...,2) to get the full precision
            // fake up a SqlExpression for NVARCHAR(30)
            SqlExpression nvarchar = sql.FunctionCall(
                typeof(void), "NVARCHAR",
                new SqlExpression[] { sql.ValueFromObject(30, false, expr.SourceExpression) },
                expr.SourceExpression
                );
            return sql.FunctionCall(
                resultClrType, "CONVERT",
                new SqlExpression[] { nvarchar, expr, sql.ValueFromObject(2, false, expr.SourceExpression) },
                expr.SourceExpression
                );
        }

        private SqlExpression ConvertBitToString(SqlExpression expr, Type resultClrType) {
            return new SqlSearchedCase(
                resultClrType,
                new SqlWhen[] { new SqlWhen(expr, sql.ValueFromObject(true.ToString(), false, expr.SourceExpression)) },
                sql.ValueFromObject(false.ToString(), false, expr.SourceExpression),
                expr.SourceExpression
                );
        }
    }
}
