using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Linq;
using System.Data.Linq.Provider;
using System.Data.Linq.Mapping;

namespace System.Data.Linq.SqlClient {

    // convert special method calls and member accesses into known sql nodes

    internal static class PreBindDotNetConverter {
        internal static SqlNode Convert(SqlNode node, SqlFactory sql, MetaModel model) {
            return new Visitor(sql, model).Visit(node);
        }

        internal static bool CanConvert(SqlNode node) {
            SqlBinary bo = node as SqlBinary;
            if (bo != null && (IsCompareToValue(bo) || IsVbCompareStringEqualsValue(bo))) {
                return true;
            }
            SqlMember sm = node as SqlMember;
            if (sm != null && IsSupportedMember(sm)) {
                return true;
            }
            SqlMethodCall mc = node as SqlMethodCall;
            if (mc != null && (IsSupportedMethod(mc) || IsSupportedVbHelperMethod(mc))) {
                return true;
            }
            return false;
        }

        private static bool IsCompareToValue(SqlBinary bo) {
            if (IsComparison(bo.NodeType)
                && bo.Left.NodeType == SqlNodeType.MethodCall
                && bo.Right.NodeType == SqlNodeType.Value) {
                SqlMethodCall call = (SqlMethodCall)bo.Left;
                return IsCompareToMethod(call) || IsCompareMethod(call);
            }

            return false;
        }

        private static bool IsCompareToMethod(SqlMethodCall call) {
            return !call.Method.IsStatic && call.Method.Name == "CompareTo" && call.Arguments.Count == 1 && call.Method.ReturnType == typeof(int);
        }

        private static bool IsCompareMethod(SqlMethodCall call) {
            return call.Method.IsStatic && call.Method.Name == "Compare" && call.Arguments.Count > 1 && call.Method.ReturnType == typeof(int);
        }

        private static bool IsComparison(SqlNodeType nodeType) {
            switch (nodeType) {
                case SqlNodeType.EQ:
                case SqlNodeType.NE:
                case SqlNodeType.LT:
                case SqlNodeType.LE:
                case SqlNodeType.GT:
                case SqlNodeType.GE:
                case SqlNodeType.EQ2V:
                case SqlNodeType.NE2V:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsVbCompareStringEqualsValue(SqlBinary bo) {
            return IsComparison(bo.NodeType)
                && bo.Left.NodeType == SqlNodeType.MethodCall
                && bo.Right.NodeType == SqlNodeType.Value
                && IsVbCompareString((SqlMethodCall)bo.Left);
        }

        private static bool IsVbCompareString(SqlMethodCall call) {
            return call.Method.IsStatic &&
                call.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators" &&
                call.Method.Name == "CompareString";
        }

        private static bool IsSupportedVbHelperMethod(SqlMethodCall mc) {
            return IsVbIIF(mc);
        }

        private static bool IsVbIIF(SqlMethodCall mc) {
            return mc.Method.IsStatic &&
                   mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.Interaction" && mc.Method.Name == "IIf";
        }

        private static bool IsSupportedMember(SqlMember m) {
            return IsNullableHasValue(m) || IsNullableHasValue(m);
        }

        private static bool IsNullableValue(SqlMember m) {
            return TypeSystem.IsNullableType(m.Expression.ClrType) && m.Member.Name == "Value";
        }

        private static bool IsNullableHasValue(SqlMember m) {
            return TypeSystem.IsNullableType(m.Expression.ClrType) && m.Member.Name == "HasValue";
        }

        private static bool IsSupportedMethod(SqlMethodCall mc) {
            if (mc.Method.IsStatic) {
                switch (mc.Method.Name) {
                    case "op_Equality":
                    case "op_Inequality":
                    case "op_LessThan":
                    case "op_LessThanOrEqual":
                    case "op_GreaterThan":
                    case "op_GreaterThanOrEqual":
                    case "op_Multiply":
                    case "op_Division":
                    case "op_Subtraction":
                    case "op_Addition":
                    case "op_Modulus":
                    case "op_BitwiseAnd":
                    case "op_BitwiseOr":
                    case "op_ExclusiveOr":
                    case "op_UnaryNegation":
                    case "op_OnesComplement":
                    case "op_False":
                        return true;
                    case "Equals":
                        return mc.Arguments.Count == 2;
                    case "Concat":
                        return mc.Method.DeclaringType == typeof(string);
                }
            }
            else {
                return mc.Method.Name == "Equals" && mc.Arguments.Count == 1 ||
                       mc.Method.Name == "GetType" && mc.Arguments.Count == 0;
            }
            return false;
        }

        private class Visitor : SqlVisitor {
            SqlFactory sql;
            MetaModel model;

            internal Visitor(SqlFactory sql, MetaModel model) {
                this.sql = sql;
                this.model = model;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
                if (IsCompareToValue(bo)) {
                    SqlMethodCall call = (SqlMethodCall)bo.Left;
                    if (IsCompareToMethod(call)) {
                        int iValue = System.Convert.ToInt32(this.Eval(bo.Right), Globalization.CultureInfo.InvariantCulture);
                        bo = this.MakeCompareTo(call.Object, call.Arguments[0], bo.NodeType, iValue) ?? bo;
                    }
                    else if (IsCompareMethod(call)) {
                        int iValue = System.Convert.ToInt32(this.Eval(bo.Right), Globalization.CultureInfo.InvariantCulture);
                        bo = this.MakeCompareTo(call.Arguments[0], call.Arguments[1], bo.NodeType, iValue) ?? bo;
                    }
                }
                else if (IsVbCompareStringEqualsValue(bo)) {
                    SqlMethodCall call = (SqlMethodCall)bo.Left;
                    int iValue = System.Convert.ToInt32(this.Eval(bo.Right), Globalization.CultureInfo.InvariantCulture);
                    //in VB, comparing a string with Nothing means comparing with ""
                    SqlValue strValue = call.Arguments[1] as SqlValue;
                    if (strValue != null && strValue.Value == null) {
                        SqlValue emptyStr = new SqlValue(strValue.ClrType, strValue.SqlType, String.Empty, strValue.IsClientSpecified, strValue.SourceExpression);
                        bo = this.MakeCompareTo(call.Arguments[0], emptyStr, bo.NodeType, iValue) ?? bo;
                    }
                    else {
                        bo = this.MakeCompareTo(call.Arguments[0], call.Arguments[1], bo.NodeType, iValue) ?? bo;
                    }
                }
                return base.VisitBinaryOperator(bo);
            }

            private SqlBinary MakeCompareTo(SqlExpression left, SqlExpression right, SqlNodeType op, int iValue) {
                if (iValue == 0) {
                    return sql.Binary(op, left, right);
                }
                else if (op == SqlNodeType.EQ || op == SqlNodeType.EQ2V) {
                    switch (iValue) {
                        case -1:
                            return sql.Binary(SqlNodeType.LT, left, right);
                        case 1:
                            return sql.Binary(SqlNodeType.GT, left, right);
                    }
                }
                return null;
            }

            private SqlExpression CreateComparison(SqlExpression a, SqlExpression b, Expression source) {
                SqlExpression lower = sql.Binary(SqlNodeType.LT, a, b);
                SqlExpression equal = sql.Binary(SqlNodeType.EQ2V, a, b);
                return sql.SearchedCase(
                    new SqlWhen[] { 
                        new SqlWhen(lower, sql.ValueFromObject(-1, false, source)),
                        new SqlWhen(equal, sql.ValueFromObject(0, false, source)),
                        },
                    sql.ValueFromObject(1, false, source), source
                );
            }

            internal override SqlNode VisitMember(SqlMember m) {
                m.Expression = this.VisitExpression(m.Expression);
                if (IsNullableValue(m)) {
                    return sql.UnaryValueOf(m.Expression, m.SourceExpression);
                }
                else if (IsNullableHasValue(m)) {
                    return sql.Unary(SqlNodeType.IsNotNull, m.Expression, m.SourceExpression);
                }
                return m;
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc) {
                mc.Object = this.VisitExpression(mc.Object);
                for (int i = 0, n = mc.Arguments.Count; i < n; i++) {
                    mc.Arguments[i] = this.VisitExpression(mc.Arguments[i]);
                }
                if (mc.Method.IsStatic) {
                    if (mc.Method.Name == "Equals" && mc.Arguments.Count == 2) {
                        return sql.Binary(SqlNodeType.EQ2V, mc.Arguments[0], mc.Arguments[1], mc.Method);
                    }
                    else if (mc.Method.DeclaringType == typeof(string) && mc.Method.Name == "Concat") {
                        SqlClientArray arr = mc.Arguments[0] as SqlClientArray;
                        List<SqlExpression> exprs = null;
                        if (arr != null) {
                            exprs = arr.Expressions;
                        }
                        else {
                            exprs = mc.Arguments;
                        }
                        if (exprs.Count == 0) {
                            return sql.ValueFromObject("", false, mc.SourceExpression);
                        }
                        else {
                            SqlExpression sum;
                            if (exprs[0].SqlType.IsString || exprs[0].SqlType.IsChar) {
                                sum = exprs[0];
                            }
                            else {
                                sum = sql.ConvertTo(typeof(string), exprs[0]);
                            }
                            for (int i = 1; i < exprs.Count; i++) {
                                if (exprs[i].SqlType.IsString || exprs[i].SqlType.IsChar) {
                                    sum = sql.Concat(sum, exprs[i]);
                                }
                                else {
                                    sum = sql.Concat(sum, sql.ConvertTo(typeof(string), exprs[i]));
                                }
                            }
                            return sum;
                        }
                    }
                    else if (IsVbIIF(mc)) {
                        return TranslateVbIIF(mc);
                    }
                    else {
                        switch (mc.Method.Name) {
                            case "op_Equality":
                                return sql.Binary(SqlNodeType.EQ, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_Inequality":
                                return sql.Binary(SqlNodeType.NE, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_LessThan":
                                return sql.Binary(SqlNodeType.LT, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_LessThanOrEqual":
                                return sql.Binary(SqlNodeType.LE, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_GreaterThan":
                                return sql.Binary(SqlNodeType.GT, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_GreaterThanOrEqual":
                                return sql.Binary(SqlNodeType.GE, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_Multiply":
                                return sql.Binary(SqlNodeType.Mul, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_Division":
                                return sql.Binary(SqlNodeType.Div, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_Subtraction":
                                return sql.Binary(SqlNodeType.Sub, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_Addition":
                                return sql.Binary(SqlNodeType.Add, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_Modulus":
                                return sql.Binary(SqlNodeType.Mod, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_BitwiseAnd":
                                return sql.Binary(SqlNodeType.BitAnd, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_BitwiseOr":
                                return sql.Binary(SqlNodeType.BitOr, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_ExclusiveOr":
                                return sql.Binary(SqlNodeType.BitXor, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            case "op_UnaryNegation":
                                return sql.Unary(SqlNodeType.Negate, mc.Arguments[0], mc.Method, mc.SourceExpression);
                            case "op_OnesComplement":
                                return sql.Unary(SqlNodeType.BitNot, mc.Arguments[0], mc.Method, mc.SourceExpression);
                            case "op_False":
                                return sql.Unary(SqlNodeType.Not, mc.Arguments[0], mc.Method, mc.SourceExpression);
                        }
                    }
                }
                else {
                    if (mc.Method.Name == "Equals" && mc.Arguments.Count == 1) {
                        return sql.Binary(SqlNodeType.EQ, mc.Object, mc.Arguments[0]);
                    }
                    else if (mc.Method.Name == "GetType" && mc.Arguments.Count == 0) {
                        MetaType mt = TypeSource.GetSourceMetaType(mc.Object, this.model);
                        if (mt.HasInheritance) {
                            Type discriminatorType = mt.Discriminator.Type;
                            SqlDiscriminatorOf discriminatorOf = new SqlDiscriminatorOf(mc.Object, discriminatorType, this.sql.TypeProvider.From(discriminatorType), mc.SourceExpression);
                            return this.VisitExpression(sql.DiscriminatedType(discriminatorOf, mt));
                        }
                        return this.VisitExpression(sql.StaticType(mt, mc.SourceExpression));
                    }
                }
                return mc;
            }

            private SqlExpression TranslateVbIIF(SqlMethodCall mc) {
                //Check to see if the types can be implicitly converted from one to another.
                if (mc.Arguments[1].ClrType == mc.Arguments[2].ClrType) {
                    List<SqlWhen> whens = new List<SqlWhen>(1);
                    whens.Add(new SqlWhen(mc.Arguments[0], mc.Arguments[1]));
                    SqlExpression @else = mc.Arguments[2];
                    while (@else.NodeType == SqlNodeType.SearchedCase) {
                        SqlSearchedCase sc = (SqlSearchedCase)@else;
                        whens.AddRange(sc.Whens);
                        @else = sc.Else;
                    }
                    return sql.SearchedCase(whens.ToArray(), @else, mc.SourceExpression);
                }
                else {
                    throw Error.IifReturnTypesMustBeEqual(mc.Arguments[1].ClrType.Name, mc.Arguments[2].ClrType.Name);
                }
            }

            internal override SqlExpression VisitTreat(SqlUnary t) {
                t.Operand = this.VisitExpression(t.Operand);
                Type treatType = t.ClrType;
                Type originalType = model.GetMetaType(t.Operand.ClrType).InheritanceRoot.Type;

                // .NET nullability rules are that typeof(int)==typeof(int?). Let's be consistent with that:
                treatType = TypeSystem.GetNonNullableType(treatType);
                originalType = TypeSystem.GetNonNullableType(originalType);

                if (treatType == originalType) {
                    return t.Operand;
                }
                else if (treatType.IsAssignableFrom(originalType)) {
                    t.Operand.SetClrType(treatType);
                    return t.Operand;
                }
                else if (!treatType.IsAssignableFrom(originalType) && !originalType.IsAssignableFrom(treatType)) {
                    if (!treatType.IsInterface && !originalType.IsInterface) { // You can't tell when there's an interface involved.
                        // We statically know the TREAT will result in NULL.
                        return sql.TypedLiteralNull(treatType, t.SourceExpression);
                    }
                }
                //return base.VisitTreat(t);
                return t;
            }
        }
    }
}
