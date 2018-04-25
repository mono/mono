using System.Linq;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq.Mapping;

    internal class SqlRetyper {
        Visitor visitor;

        internal SqlRetyper(TypeSystemProvider typeProvider, MetaModel model) {
            this.visitor = new Visitor(typeProvider, model);
        }

        internal SqlNode Retype(SqlNode node) {
            return this.visitor.Visit(node);
        }

        class Visitor : SqlVisitor {
            private TypeSystemProvider typeProvider;
            private SqlFactory sql;

            internal Visitor(TypeSystemProvider typeProvider, MetaModel model) {
                this.sql = new SqlFactory(typeProvider, model);
                this.typeProvider = typeProvider;
            }

            internal override SqlExpression VisitColumn(SqlColumn col) {
                return base.VisitColumn(col);
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo) {
                base.VisitUnaryOperator(uo);
                if (uo.NodeType != SqlNodeType.Convert && uo.Operand != null && uo.Operand.SqlType != null) {
                    uo.SetSqlType(this.typeProvider.PredictTypeForUnary(uo.NodeType, uo.Operand.SqlType));
                }
                return uo;
            }

            private static bool CanDbConvert(Type from, Type to) {
                from = System.Data.Linq.SqlClient.TypeSystem.GetNonNullableType(from);
                to = System.Data.Linq.SqlClient.TypeSystem.GetNonNullableType(to);
                if (from == to)
                    return true;
                if (to.IsAssignableFrom(from))
                    return true;
                var tcTo = Type.GetTypeCode(to);
                var tcFrom = Type.GetTypeCode(from);
                switch (tcTo) {
                    case TypeCode.Int16: return tcFrom == TypeCode.Byte || tcFrom == TypeCode.SByte;
                    case TypeCode.Int32: return tcFrom == TypeCode.Byte || tcFrom == TypeCode.SByte || tcFrom == TypeCode.Int16 || tcFrom == TypeCode.UInt16;
                    case TypeCode.Int64: return tcFrom == TypeCode.Byte || tcFrom == TypeCode.SByte || tcFrom == TypeCode.Int16 || tcFrom == TypeCode.UInt16 || tcFrom == TypeCode.Int32 || tcFrom==TypeCode.UInt32;
                    case TypeCode.UInt16: return tcFrom == TypeCode.Byte || tcFrom == TypeCode.SByte;
                    case TypeCode.UInt32: return tcFrom == TypeCode.Byte || tcFrom == TypeCode.SByte || tcFrom == TypeCode.Int16 || tcFrom == TypeCode.UInt16;
                    case TypeCode.UInt64: return tcFrom == TypeCode.Byte || tcFrom == TypeCode.SByte || tcFrom == TypeCode.Int16 || tcFrom == TypeCode.UInt16 || tcFrom == TypeCode.Int32 || tcFrom == TypeCode.UInt32;
                    case TypeCode.Double: return tcFrom == TypeCode.Single;
                    case TypeCode.Decimal: return tcFrom == TypeCode.Single || tcFrom == TypeCode.Double;
                }
                return false;
            }
            internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
                base.VisitBinaryOperator(bo);
                if (bo.NodeType.IsComparisonOperator() 
                    && bo.Left.ClrType!=typeof(bool) && bo.Right.ClrType!=typeof(bool)) {
                    // Strip unnecessary CONVERT calls. 
                    if (bo.Left.NodeType == SqlNodeType.Convert) {
                        var conv = (SqlUnary)bo.Left;
                        if (CanDbConvert(conv.Operand.ClrType, bo.Right.ClrType) 
                            && conv.Operand.SqlType.ComparePrecedenceTo(bo.Right.SqlType) != 1) {
                            return VisitBinaryOperator(new SqlBinary(bo.NodeType, bo.ClrType, bo.SqlType, conv.Operand, bo.Right));
                        }
                    }
                    if (bo.Right.NodeType == SqlNodeType.Convert) {
                        var conv = (SqlUnary)bo.Right;
                        if (CanDbConvert(conv.Operand.ClrType, bo.Left.ClrType)
                            && conv.Operand.SqlType.ComparePrecedenceTo(bo.Left.SqlType) != 1) {
                            return VisitBinaryOperator(new SqlBinary(bo.NodeType, bo.ClrType, bo.SqlType, bo.Left, conv.Operand));
                        }
                    }
                }
                if (bo.Right != null && bo.NodeType != SqlNodeType.Concat) {
                    SqlExpression left = bo.Left;
                    SqlExpression right = bo.Right;
                    this.CoerceBinaryArgs(ref left, ref right);
                    if (bo.Left != left || bo.Right != right) {
                        bo = sql.Binary(bo.NodeType, left, right);
                    }
                    bo.SetSqlType(typeProvider.PredictTypeForBinary(bo.NodeType, left.SqlType, right.SqlType));
                }
                if (bo.NodeType.IsComparisonOperator()) {
                    // When comparing a unicode value against a non-unicode column, 
                    // we want retype the parameter as non-unicode.
                    Func<SqlExpression, SqlExpression, bool> needsRetype = 
                        (expr, val) => (val.NodeType == SqlNodeType.Value || val.NodeType == SqlNodeType.ClientParameter) && 
                                       !(expr.NodeType == SqlNodeType.Value || expr.NodeType == SqlNodeType.ClientParameter) &&
                                       val.SqlType.IsUnicodeType && !expr.SqlType.IsUnicodeType;
                    SqlSimpleTypeExpression valueToRetype = null;
                    if (needsRetype(bo.Left, bo.Right)) {
                        valueToRetype = (SqlSimpleTypeExpression)bo.Right;
                    } else if (needsRetype(bo.Right, bo.Left)) {
                        valueToRetype = (SqlSimpleTypeExpression)bo.Left;
                    }
                    if(valueToRetype != null) {
                        valueToRetype.SetSqlType(valueToRetype.SqlType.GetNonUnicodeEquivalent());
                    }
                }
                return bo;
            }

            internal override SqlExpression VisitIn(SqlIn sin) {
                // Treat the IN as a series of binary comparison expressions (and coerce if necessary).
                // Check to see if any expressions need to change as a result of coercion, where we start
                // with "sin.Expression IN sin.Values" and coerced expressions are "test IN newValues".
                SqlExpression test = sin.Expression;
                bool requiresCoercion = false;
                var newValues = new System.Collections.Generic.List<SqlExpression>(sin.Values.Count);
                ProviderType valueType = null;
                for (int i = 0, n = sin.Values.Count; i < n; i++) {
                    SqlExpression value = sin.Values[i];
                    this.CoerceBinaryArgs(ref test, ref value);
                    if (value != sin.Values[i]) {
                        // Build up 'widest' type by repeatedly applying PredictType
                        valueType = null == valueType
                            ? value.SqlType
                            : this.typeProvider.PredictTypeForBinary(SqlNodeType.EQ, value.SqlType, valueType);
                        requiresCoercion = true;
                    }
                    newValues.Add(value);
                }
                if (test != sin.Expression) {
                    requiresCoercion = true;
                }
                if (requiresCoercion) {
                    ProviderType providerType = this.typeProvider.PredictTypeForBinary(SqlNodeType.EQ, test.SqlType, valueType);
                    sin = new SqlIn(sin.ClrType, providerType, test, newValues, sin.SourceExpression);
                }
                return sin;
            }

            internal override SqlExpression VisitLike(SqlLike like) {
                base.VisitLike(like);
                // When comparing a unicode pattern against a non-unicode expression, 
                // we want retype the pattern as non-unicode.
                if (!like.Expression.SqlType.IsUnicodeType && like.Pattern.SqlType.IsUnicodeType &&
                    (like.Pattern.NodeType == SqlNodeType.Value || like.Pattern.NodeType == SqlNodeType.ClientParameter)) {
                    SqlSimpleTypeExpression pattern = (SqlSimpleTypeExpression)like.Pattern;
                    pattern.SetSqlType(pattern.SqlType.GetNonUnicodeEquivalent());
                }
                return like;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                base.VisitScalarSubSelect(ss);
                ss.SetSqlType(ss.Select.Selection.SqlType);
                return ss;
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c) {
                base.VisitSearchedCase(c);

                // determine the best common type for all the when and else values
                ProviderType type = c.Whens[0].Value.SqlType;
                for (int i = 1; i < c.Whens.Count; i++) {
                    ProviderType whenType = c.Whens[i].Value.SqlType;
                    type = typeProvider.GetBestType(type, whenType);
                }
                if (c.Else != null) {
                    ProviderType elseType = c.Else.SqlType;
                    type = typeProvider.GetBestType(type, elseType);
                }

                // coerce each one          
                foreach (SqlWhen when in c.Whens.Where(w => w.Value.SqlType != type && !w.Value.SqlType.IsRuntimeOnlyType)) {
                    when.Value = sql.UnaryConvert(when.Value.ClrType, type, when.Value, when.Value.SourceExpression);
                }

                if (c.Else != null && c.Else.SqlType != type && !c.Else.SqlType.IsRuntimeOnlyType) {
                    c.Else = sql.UnaryConvert(c.Else.ClrType, type, c.Else, c.Else.SourceExpression);
                }

                return c;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c) {
                base.VisitSimpleCase(c);

                // determine the best common type for all the when values
                ProviderType type = c.Whens[0].Value.SqlType;
                for (int i = 1; i < c.Whens.Count; i++) {
                    ProviderType whenType = c.Whens[i].Value.SqlType;
                    type = typeProvider.GetBestType(type, whenType);
                }

                // coerce each one          
                foreach (SqlWhen when in c.Whens.Where(w => w.Value.SqlType != type && !w.Value.SqlType.IsRuntimeOnlyType)) {
                    when.Value = sql.UnaryConvert(when.Value.ClrType, type, when.Value, when.Value.SourceExpression);
                }

                return c;
            }

            internal override SqlStatement VisitAssign(SqlAssign sa) {
                base.VisitAssign(sa);
                SqlExpression right = sa.RValue;
                this.CoerceToFirst(sa.LValue, ref right);
                sa.RValue = right;
                return sa;
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc) {
                for (int i = 0, n = fc.Arguments.Count; i < n; i++) {
                    fc.Arguments[i] = this.VisitExpression(fc.Arguments[i]);
                }
                if (fc.Arguments.Count > 0) {
                    ProviderType oldType = fc.Arguments[0].SqlType;
                    // if this has a real argument (not e.g. the symbol "DAY" in DATEDIFF(DAY,...))
                    if (oldType != null) {
                        ProviderType newType = this.typeProvider.ReturnTypeOfFunction(fc);
                        if (newType != null) {
                            fc.SetSqlType(newType);
                        }
                    }
                }
                return fc;
            }

            private void CoerceToFirst(SqlExpression arg1, ref SqlExpression arg2) {
                if (arg1.SqlType != null && arg2.SqlType != null) {
                    if (arg2.NodeType == SqlNodeType.Value) {
                        SqlValue val = (SqlValue)arg2;
                        arg2 = sql.Value(
                            arg1.ClrType, arg1.SqlType,
                            DBConvert.ChangeType(val.Value, arg1.ClrType),
                            val.IsClientSpecified, arg2.SourceExpression
                            );
                    } else if (arg2.NodeType == SqlNodeType.ClientParameter && arg2.SqlType != arg1.SqlType) {
                        SqlClientParameter cp = (SqlClientParameter)arg2;
                        cp.SetSqlType(arg1.SqlType);
                    } else {
                        arg2 = sql.UnaryConvert(arg1.ClrType, arg1.SqlType, arg2, arg2.SourceExpression);
                    }
                }
            }

            private void CoerceBinaryArgs(ref SqlExpression arg1, ref SqlExpression arg2)
            {
                if (arg1.SqlType == null || arg2.SqlType == null) return;

                if (arg1.SqlType.IsSameTypeFamily(arg2.SqlType)) {
                    CoerceTypeFamily(arg1, arg2);
                }
                else {
                    // Don't coerce bools because predicates and bits have not been resolved yet.
                    // Leave this for booleanizer.
                    if (arg1.ClrType != typeof(bool) && arg2.ClrType != typeof(bool)) {
                        CoerceTypes(ref arg1, ref arg2);
                    }
                }
            }

            private void CoerceTypeFamily(SqlExpression arg1, SqlExpression arg2)
            {
                if ((arg1.SqlType.HasPrecisionAndScale && arg2.SqlType.HasPrecisionAndScale && arg1.SqlType != arg2.SqlType) ||
                    SqlFactory.IsSqlHighPrecisionDateTimeType(arg1) || SqlFactory.IsSqlHighPrecisionDateTimeType(arg2)) {
                        ProviderType best = typeProvider.GetBestType(arg1.SqlType, arg2.SqlType);
                        SetSqlTypeIfSimpleExpression(arg1, best);
                        SetSqlTypeIfSimpleExpression(arg2, best);
                        return;
                    }

                // The SQL data type DATE is special, in that it has a higher range but lower
                // precedence, so we need to account for that here (DevDiv 175229)
                if (SqlFactory.IsSqlDateType(arg1) && !SqlFactory.IsSqlHighPrecisionDateTimeType(arg2)) {
                    SetSqlTypeIfSimpleExpression(arg2, arg1.SqlType);
                }
                else if (SqlFactory.IsSqlDateType(arg2) && !SqlFactory.IsSqlHighPrecisionDateTimeType(arg1)) {
                    SetSqlTypeIfSimpleExpression(arg1, arg2.SqlType);
                }
            }

            private static void SetSqlTypeIfSimpleExpression(SqlExpression expression, ProviderType sqlType)
            {
                SqlSimpleTypeExpression simpleExpression = expression as SqlSimpleTypeExpression;
                if (simpleExpression != null) {
                    simpleExpression.SetSqlType(sqlType);
                }
            }

            private void CoerceTypes(ref SqlExpression arg1, ref SqlExpression arg2)
            {
                if (arg2.NodeType == SqlNodeType.Value) {
                    arg2 = CoerceValueForExpression((SqlValue)arg2, arg1);
                }
                else if (arg1.NodeType == SqlNodeType.Value) {
                    arg1 = CoerceValueForExpression((SqlValue)arg1, arg2);
                }
                else if (arg2.NodeType == SqlNodeType.ClientParameter && arg2.SqlType != arg1.SqlType) {
                    ((SqlClientParameter)arg2).SetSqlType(arg1.SqlType);
                }
                else if (arg1.NodeType == SqlNodeType.ClientParameter && arg1.SqlType != arg2.SqlType) {
                    ((SqlClientParameter)arg1).SetSqlType(arg2.SqlType);
                }
                else {
                    int coercionPrecedence = arg1.SqlType.ComparePrecedenceTo(arg2.SqlType);
                    if (coercionPrecedence > 0) {
                        arg2 = sql.UnaryConvert(arg1.ClrType, arg1.SqlType, arg2, arg2.SourceExpression);
                    }
                    else if (coercionPrecedence < 0) {
                        arg1 = sql.UnaryConvert(arg2.ClrType, arg2.SqlType, arg1, arg1.SourceExpression);
                    }
                }
            }

            private SqlExpression CoerceValueForExpression(SqlValue value, SqlExpression expression)
            {
                object clrValue = value.Value;
                if (!value.ClrType.IsAssignableFrom(expression.ClrType)) {
                    clrValue = DBConvert.ChangeType(clrValue, expression.ClrType);
                }
                ProviderType newSqlType = typeProvider.ChangeTypeFamilyTo(value.SqlType, expression.SqlType);
                return sql.Value(expression.ClrType, newSqlType, clrValue, value.IsClientSpecified, value.SourceExpression);
            }
        }
    }
}
