using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Provider;
using System.Data.Linq.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    internal static class PostBindDotNetConverter {

        internal enum MethodSupport 
        { 
            None,         // Unsupported method
            MethodGroup,  // One or more overloads of the method are supported
            Method        // The particular method form specified is supported (stronger)
        }

        internal static SqlNode Convert(SqlNode node, SqlFactory sql, SqlProvider.ProviderMode providerMode) {
            return new Visitor(sql, providerMode).Visit(node);
        }

        internal static bool CanConvert(SqlNode node) {
            SqlUnary su = node as SqlUnary;
            if (su != null && IsSupportedUnary(su)) {
                return true;
            }
            SqlNew sn = node as SqlNew;
            if (sn != null && IsSupportedNew(sn)) {
                return true;
            }
            SqlMember sm = node as SqlMember;
            if (sm != null && IsSupportedMember(sm)) {
                return true;
            }
            SqlMethodCall mc = node as SqlMethodCall;
            if (mc != null && (GetMethodSupport(mc) == MethodSupport.Method)) {
                return true;
            }
            return false;
        }

        private static bool IsSupportedUnary(SqlUnary uo) {
            return uo.NodeType == SqlNodeType.Convert &&
                   uo.ClrType == typeof(char) || uo.Operand.ClrType == typeof(char);
        }

        private static bool IsSupportedNew(SqlNew snew) {
            if (snew.ClrType == typeof(string)) {
                return IsSupportedStringNew(snew);
            }
            else if (snew.ClrType == typeof(TimeSpan)) {
                return IsSupportedTimeSpanNew(snew);
            }
            else if (snew.ClrType == typeof(DateTime)) {
                return IsSupportedDateTimeNew(snew);
            }
            return false;
        }

        private static bool IsSupportedStringNew(SqlNew snew) {
            return snew.Args.Count == 2 && snew.Args[0].ClrType == typeof(char) && snew.Args[1].ClrType == typeof(int);
        }

        private static bool IsSupportedDateTimeNew(SqlNew sox) {
            if (sox.ClrType == typeof(DateTime)
                && sox.Args.Count >= 3
                && sox.Args[0].ClrType == typeof(int)
                && sox.Args[1].ClrType == typeof(int)
                && sox.Args[2].ClrType == typeof(int)) {
                if (sox.Args.Count == 3) {
                    return true;
                }
                if (sox.Args.Count >= 6 &&
                    sox.Args[3].ClrType == typeof(int) && sox.Args[4].ClrType == typeof(int) && sox.Args[5].ClrType == typeof(int)) {
                    if (sox.Args.Count == 6) {
                        return true;
                    }
                    if ((sox.Args.Count == 7) && (sox.Args[6].ClrType == typeof(int))) {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsSupportedTimeSpanNew(SqlNew sox) {
            if (sox.Args.Count == 1) {
                return true;
            }
            else if (sox.Args.Count == 3) {
                return true;
            }
            else {
                if (sox.Args.Count == 4) {
                    return true;
                }
                else if (sox.Args.Count == 5) {
                    return true;
                }
            }
            return false;
        }

        private static MethodSupport GetMethodSupport(SqlMethodCall mc) {
            // Get support level for each, returning the highest
            MethodSupport best = MethodSupport.None;
            MethodSupport ms = GetSqlMethodsMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetDateTimeMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetDateTimeOffsetMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetTimeSpanMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetConvertMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetDecimalMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetMathMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetStringMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetComparisonMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetNullableMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetCoercionMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetObjectMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }
            ms = GetVbHelperMethodSupport(mc);
            if (ms > best) {
                best = ms;
            }

            return best;
        }

        private static MethodSupport GetCoercionMethodSupport(SqlMethodCall mc) {
            if(mc.Method.IsStatic
                && mc.SqlType.CanBeColumn
                && (mc.Method.Name == "op_Implicit" || mc.Method.Name == "op_Explicit")){
                return MethodSupport.Method;
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetComparisonMethodSupport(SqlMethodCall mc) {
            if (mc.Method.IsStatic && mc.Method.Name == "Compare" && mc.Method.ReturnType == typeof(int)) {
                return MethodSupport.Method;
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetObjectMethodSupport(SqlMethodCall mc) {
            if (!mc.Method.IsStatic) {
                switch (mc.Method.Name) {
                    case "Equals":
                        return MethodSupport.Method;
                    case "ToString":
                        if (mc.Object.SqlType.CanBeColumn) {
                            return MethodSupport.Method;
                        }
                        return MethodSupport.None;
                    case "GetType":
                        if (mc.Arguments.Count == 0) {
                            return MethodSupport.Method;
                        }
                        return MethodSupport.None;
                }
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetNullableMethodSupport(SqlMethodCall mc) {
            if (mc.Method.Name == "GetValueOrDefault" && TypeSystem.IsNullableType(mc.Object.ClrType)) {
                return MethodSupport.Method;
            }
            return MethodSupport.None;
        }

        private static readonly string[] dateParts = { "Year", "Month", "Day", "Hour", "Minute", "Second", 
                                                       "Millisecond", "Microsecond", "Nanosecond" };

        private static MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc) {
            if (mc.Method.IsStatic && mc.Method.DeclaringType == typeof(SqlMethods)) {
                if (mc.Method.Name.StartsWith("DateDiff", StringComparison.Ordinal) && mc.Arguments.Count == 2) {
                    foreach (string datePart in dateParts) {
                        if (mc.Method.Name == "DateDiff" + datePart) {
                            if (mc.Arguments.Count == 2) {
                                return MethodSupport.Method;
                            } else {
                                return MethodSupport.MethodGroup;
                            }
                        }
                    }
                }
                else if (mc.Method.Name == "Like") {
                    if (mc.Arguments.Count == 2) {
                        return MethodSupport.Method;
                    }
                    else if (mc.Arguments.Count == 3) {
                        return MethodSupport.Method;
                    }
                    return MethodSupport.MethodGroup;
                }
                else if (mc.Method.Name == "RawLength") {
                    return MethodSupport.Method;
                }
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetDateTimeMethodSupport(SqlMethodCall mc) {
            if (!mc.Method.IsStatic && mc.Method.DeclaringType == typeof(DateTime)) {
                switch (mc.Method.Name) {
                    case "CompareTo":
                    case "AddTicks":
                    case "AddMonths":
                    case "AddYears":
                    case "AddMilliseconds":
                    case "AddSeconds":
                    case "AddMinutes":
                    case "AddHours":
                    case "AddDays":
                        return MethodSupport.Method;
                    case "Add":
                        if (mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan)) {
                            return MethodSupport.Method;
                        } else {
                            return MethodSupport.MethodGroup;
                        }
                }
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetDateTimeOffsetMethodSupport(SqlMethodCall mc) {
            if (!mc.Method.IsStatic && mc.Method.DeclaringType == typeof(DateTimeOffset)) {
                switch (mc.Method.Name) {
                    case "CompareTo":
                    case "AddTicks":
                    case "AddMonths":
                    case "AddYears":
                    case "AddMilliseconds":
                    case "AddSeconds":
                    case "AddMinutes":
                    case "AddHours":
                    case "AddDays":
                        return MethodSupport.Method;
                    case "Add":
                        if (mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan)) {
                            return MethodSupport.Method;
                        }
                        else {
                            return MethodSupport.MethodGroup;
                        }
                }
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetTimeSpanMethodSupport(SqlMethodCall mc) {
            if (!mc.Method.IsStatic && mc.Method.DeclaringType == typeof(TimeSpan)) {
                switch (mc.Method.Name) {
                    case "Add":
                    case "Subtract":
                    case "CompareTo":
                    case "Duration":
                    case "Negate":
                        return MethodSupport.Method;
                }
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetConvertMethodSupport(SqlMethodCall mc) {
            if (mc.Method.IsStatic && mc.Method.DeclaringType == typeof(Convert) && mc.Arguments.Count == 1) {
                switch (mc.Method.Name) {
                    case "ToBoolean":
                    case "ToDecimal":
                    case "ToByte":
                    case "ToChar":
                    case "ToDouble":
                    case "ToInt16":
                    case "ToInt32":
                    case "ToInt64":
                    case "ToSingle":
                    case "ToString":
                        return MethodSupport.Method;
                    case "ToDateTime":
                        if (mc.Arguments[0].ClrType == typeof(string) || mc.Arguments[0].ClrType == typeof(DateTime)) {
                            return MethodSupport.Method;
                        } else {
                            return MethodSupport.MethodGroup;
                        }
                }
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetDecimalMethodSupport(SqlMethodCall mc) {
            if (mc.Method.IsStatic) {
                if (mc.Arguments.Count == 2) {
                    switch (mc.Method.Name) {
                        case "Multiply":
                        case "Divide":
                        case "Subtract":
                        case "Add":
                        case "Remainder":
                        case "Round":
                            return MethodSupport.Method;
                    }
                }
                else if (mc.Arguments.Count == 1) {
                    switch (mc.Method.Name) {
                        case "Negate":
                        case "Floor":
                        case "Truncate":
                        case "Round":
                            return MethodSupport.Method;
                        default:
                            if (mc.Method.Name.StartsWith("To", StringComparison.Ordinal)) {
                                return MethodSupport.Method;
                            }
                            break;
                    }
                }
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetStringMethodSupport(SqlMethodCall mc) {
            if (mc.Method.DeclaringType == typeof(string)) {
                if (mc.Method.IsStatic) {
                    if (mc.Method.Name == "Concat") {
                        return MethodSupport.Method;
                    }
                }
                else {
                    switch (mc.Method.Name) {
                        case "Contains":
                        case "StartsWith":
                        case "EndsWith":
                            if (mc.Arguments.Count == 1) {
                                return MethodSupport.Method;
                            }
                            return MethodSupport.MethodGroup;
                        case "IndexOf":
                        case "LastIndexOf":
                            if (mc.Arguments.Count == 1
                                || mc.Arguments.Count == 2
                                || mc.Arguments.Count == 3) {
                                return MethodSupport.Method;
                            }
                            return MethodSupport.MethodGroup;
                        case "Insert":
                            if (mc.Arguments.Count == 2) {
                                return MethodSupport.Method;
                            }
                            return MethodSupport.MethodGroup;
                        case "PadLeft":
                        case "PadRight":
                        case "Remove":
                        case "Substring":
                            if(mc.Arguments.Count == 1
                               || mc.Arguments.Count == 2) {
                                return MethodSupport.Method;
                            }
                            return MethodSupport.MethodGroup;
                        case "Replace":
                            return MethodSupport.Method;
                        case "Trim":
                        case "ToLower":
                        case "ToUpper":
                            if (mc.Arguments.Count == 0) {
                                return MethodSupport.Method;
                            }
                            return MethodSupport.MethodGroup;
                        case "get_Chars":
                        case "CompareTo":
                            if (mc.Arguments.Count == 1) {
                                return MethodSupport.Method;
                            }
                            return MethodSupport.MethodGroup;
                    }
                }
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetMathMethodSupport(SqlMethodCall mc) {
            if (mc.Method.IsStatic && mc.Method.DeclaringType == typeof(Math)) {
                switch (mc.Method.Name) {
                    case "Abs":
                    case "Acos":
                    case "Asin":
                    case "Atan":
                    case "Ceiling":
                    case "Cos":
                    case "Cosh":
                    case "Exp":
                    case "Floor":
                    case "Log10":
                        if(mc.Arguments.Count == 1) {
                            return MethodSupport.Method;
                        }
                        return MethodSupport.MethodGroup;
                    case "Log":
                        if (mc.Arguments.Count == 1 || mc.Arguments.Count == 2) {
                            return MethodSupport.Method;
                        };
                        return MethodSupport.MethodGroup;
                    case "Max":
                    case "Min":
                    case "Pow":
                    case "Atan2":
                    case "BigMul":
                        if (mc.Arguments.Count == 2) {
                            return MethodSupport.Method;
                        }
                        return MethodSupport.MethodGroup;
                    case "Round":
                        if (mc.Arguments[mc.Arguments.Count - 1].ClrType == typeof(MidpointRounding)
                            && (mc.Arguments.Count == 2 || mc.Arguments.Count == 3)) {
                            return MethodSupport.Method;
                        }
                        return MethodSupport.MethodGroup;
                    case "Sign":
                    case "Sin":
                    case "Sinh":
                    case "Sqrt":
                    case "Tan":
                    case "Tanh":
                    case "Truncate":
                        if (mc.Arguments.Count == 1) {
                            return MethodSupport.Method;
                        }
                        return MethodSupport.MethodGroup;
                }
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetVbHelperMethodSupport(SqlMethodCall mc) {
            if (IsVbConversionMethod(mc) ||
                IsVbCompareString(mc) ||
                IsVbLike(mc)) {
                return MethodSupport.Method;
            }
            return MethodSupport.None;
        }

        private static bool IsVbCompareString(SqlMethodCall call) {
            return call.Method.IsStatic &&
                   call.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators" &&
                   call.Method.Name == "CompareString";
        }

        private static bool IsVbLike(SqlMethodCall mc) {
            return mc.Method.IsStatic &&
                   (mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.LikeOperator" && mc.Method.Name == "LikeString")
                   || (mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators" && mc.Method.Name == "LikeString");
        }

        private static bool IsVbConversionMethod(SqlMethodCall mc) {
            if (mc.Method.IsStatic &&
                mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Conversions") {
                switch (mc.Method.Name) {
                    case "ToBoolean":
                    case "ToSByte":
                    case "ToByte":
                    case "ToChar":
                    case "ToCharArrayRankOne":
                    case "ToDate":
                    case "ToDecimal":
                    case "ToDouble":
                    case "ToInteger":
                    case "ToUInteger":
                    case "ToLong":
                    case "ToULong":
                    case "ToShort":
                    case "ToUShort":
                    case "ToSingle":
                    case "ToString":
                        return true;
                }
            }
            return false;
        }

        private static bool IsSupportedMember(SqlMember m) {
            return IsSupportedStringMember(m)
                || IsSupportedBinaryMember(m)
                || IsSupportedDateTimeMember(m)
                || IsSupportedDateTimeOffsetMember(m)
                || IsSupportedTimeSpanMember(m);
        }

        private static bool IsSupportedStringMember(SqlMember m) {
            return m.Expression.ClrType == typeof(string) && m.Member.Name == "Length";
        }

        private static bool IsSupportedBinaryMember(SqlMember m) {
            return m.Expression.ClrType == typeof(Binary) && m.Member.Name == "Length";
        }

        private static string GetDatePart(string memberName) {
            switch (memberName) {
                case "Year":
                case "Month":
                case "Day":
                case "DayOfYear":
                case "Hour":
                case "Minute":
                case "Second":
                case "Millisecond":
                    return memberName;
                default:
                    return null;
            }
        }

        private static bool IsSupportedDateTimeMember(SqlMember m) {
            if (m.Expression.ClrType == typeof(DateTime)) {
                string datePart = GetDatePart(m.Member.Name);
                if (datePart != null) {
                    return true;
                }
                switch (m.Member.Name) {
                    case "Date":
                    case "TimeOfDay":
                    case "DayOfWeek":
                        return true;
                }
            }
            return false;
        }

        //
        // Identical to IsSupportedDateTimeMember(), except for support for 'DateTime'
        //
        private static bool IsSupportedDateTimeOffsetMember(SqlMember m) {
            if (m.Expression.ClrType == typeof(DateTimeOffset)) {
                string datePart = GetDatePart(m.Member.Name);
                if (datePart != null) {
                    return true;
                }
                switch (m.Member.Name) {
                    case "Date":
                    case "DateTime":
                    case "TimeOfDay":
                    case "DayOfWeek":
                        return true;
                }
            }
            return false;
        }

        private static bool IsSupportedTimeSpanMember(SqlMember m) {
            if (m.Expression.ClrType == typeof(TimeSpan)) {
                switch (m.Member.Name) {
                    case "Ticks":
                    case "TotalMilliseconds":
                    case "TotalSeconds":
                    case "TotalMinutes":
                    case "TotalHours":
                    case "TotalDays":
                    case "Milliseconds":
                    case "Seconds":
                    case "Minutes":
                    case "Hours":
                    case "Days":
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Skips over client portion of selection expression
        /// </summary>
        private class SqlSelectionSkipper : SqlVisitor {
            SqlVisitor parent;
            internal SqlSelectionSkipper(SqlVisitor parent) {
                this.parent = parent;
            }
            internal override SqlExpression VisitColumn(SqlColumn col) {
                // pass control back to parent
                return parent.VisitColumn(col);
            }
            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                // pass control back to parent
                return this.parent.VisitSubSelect(ss);
            }
            internal override SqlExpression VisitClientQuery(SqlClientQuery cq) {
                // pass control back to parent
                return this.parent.VisitClientQuery(cq);
            }
        }

        private class Visitor : SqlVisitor {
            SqlFactory sql;
            SqlProvider.ProviderMode providerMode;
            SqlSelectionSkipper skipper;

            internal Visitor(SqlFactory sql, SqlProvider.ProviderMode providerMode) {
                this.sql = sql;
                this.providerMode = providerMode;
                this.skipper = new SqlSelectionSkipper(this);
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                select = this.VisitSelectCore(select);
                // don't transate frameworks calls on client side of selection
                select.Selection = this.skipper.VisitExpression(select.Selection);
                return select;
            }

            // transform type conversion if necessary
            internal override SqlExpression VisitUnaryOperator(SqlUnary uo) {
                if (uo.NodeType == SqlNodeType.Convert) {
                    Type newType = uo.ClrType;
                    SqlExpression expr = uo.Operand;
                    if (newType == typeof(char) || expr.ClrType == typeof(char)) {
                        expr = this.VisitExpression(uo.Operand);
                        uo.Operand = expr;
                        return sql.ConvertTo(newType, uo.SqlType, expr);
                    }
                }
                return base.VisitUnaryOperator(uo);
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
                bo = (SqlBinary)base.VisitBinaryOperator(bo);
                Type leftType = TypeSystem.GetNonNullableType(bo.Left.ClrType);
                if (leftType == typeof(DateTime) || leftType == typeof(DateTimeOffset)) {
                    return this.TranslateDateTimeBinary(bo);
                }
                return bo;
            }


            // currently only happens for generated test cases with optimization SimplifyCaseStatements off
            internal override SqlExpression VisitTypeCase(SqlTypeCase tc) {
                tc.Discriminator = base.VisitExpression(tc.Discriminator);
                List<SqlExpression> matches = new List<SqlExpression>();
                List<SqlExpression> values = new List<SqlExpression>();
                bool remainsTypeCase = true;
                foreach (SqlTypeCaseWhen when in tc.Whens) {
                    SqlExpression newMatch = this.VisitExpression(when.Match);
                    SqlExpression newNew = this.VisitExpression(when.TypeBinding);
                    remainsTypeCase = remainsTypeCase && (newNew is SqlNew);
                    matches.Add(newMatch);
                    values.Add(newNew);
                }
                if (remainsTypeCase) {
                    for (int i = 0, n = tc.Whens.Count; i < n; i++) {
                        SqlTypeCaseWhen when = tc.Whens[i];
                        when.Match = matches[i];
                        when.TypeBinding = (SqlNew)values[i];
                    }
                    return tc;
                }
                else {
                    return sql.Case(tc.ClrType, tc.Discriminator, matches, values, tc.SourceExpression);
                }
            }

            // transform constructors if necessary
            internal override SqlExpression VisitNew(SqlNew sox) {
                sox = (SqlNew)base.VisitNew(sox);
                if (sox.ClrType == typeof(string)) {
                    return TranslateNewString(sox);
                }
                else if (sox.ClrType == typeof(TimeSpan)) {
                    return TranslateNewTimeSpan(sox);
                }
                else if (sox.ClrType == typeof(DateTime)) {
                    return TranslateNewDateTime(sox);
                }
                else if (sox.ClrType == typeof(DateTimeOffset)) {
                    return TranslateNewDateTimeOffset(sox);
                }
                return sox;
            }

            private SqlExpression TranslateNewString(SqlNew sox) {
                // string(char c, int i) 
                // --> REPLICATE(@c,@i)
                if (sox.ClrType == typeof(string) && sox.Args.Count == 2
                    && sox.Args[0].ClrType == typeof(char) && sox.Args[1].ClrType == typeof(int)) {
                    return sql.FunctionCall(typeof(string), "REPLICATE", new SqlExpression[] { sox.Args[0], sox.Args[1] }, sox.SourceExpression);
                }
                throw Error.UnsupportedStringConstructorForm();
            }

            private SqlExpression TranslateNewDateTime(SqlNew sox) {
                Expression source = sox.SourceExpression;

                // DateTime(int year, int month, int day) 
                // --> CONVERT(DATETIME, CONVERT(nchar(2),@month) + '/' + CONVERT(nchar(2),@day) + '/' + CONVERT(nchar(4),@year),101)
                if (sox.ClrType == typeof(DateTime) && sox.Args.Count >= 3 &&
                    sox.Args[0].ClrType == typeof(int) && sox.Args[1].ClrType == typeof(int) && sox.Args[2].ClrType == typeof(int)) {
                    SqlExpression char2 = sql.FunctionCall(typeof(void), "NCHAR", new SqlExpression[1] { sql.ValueFromObject(2, false, source) }, source);
                    SqlExpression char4 = sql.FunctionCall(typeof(void), "NCHAR", new SqlExpression[1] { sql.ValueFromObject(4, false, source) }, source);
                    SqlExpression year = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char4, sox.Args[0] }, source);
                    SqlExpression month = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[1] }, source);
                    SqlExpression day = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[2] }, source);
                    SqlExpression datetime = new SqlVariable(typeof(void), null, "DATETIME", source);
                    if (sox.Args.Count == 3) {
                        SqlExpression date = sql.Concat(month, sql.ValueFromObject("/", false, source), day, sql.ValueFromObject("/", false, source), year);
                        return sql.FunctionCall(typeof(DateTime), "CONVERT", new SqlExpression[3] { datetime, date, sql.ValueFromObject(101, false, source) }, source);
                    }
                    if (sox.Args.Count >= 6 &&
                        sox.Args[3].ClrType == typeof(int) && sox.Args[4].ClrType == typeof(int) && sox.Args[5].ClrType == typeof(int)) {
                        // DateTime(year, month, day, hour, minute, second ) 
                        // --> CONVERT(DATETIME, CONVERT(nchar(2),@month) + '-' + CONVERT(nchar(2),@day) + '-' + CONVERT(nchar(4),@year) +
                        //                 ' ' + CONVERT(nchar(2),@hour) + ':' + CONVERT(nchar(2),@minute) + ':' + CONVERT(nchar(2),@second)  ,120)
                        SqlExpression hour = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[3] }, source);
                        SqlExpression minute = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[4] }, source);
                        SqlExpression second = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[5] }, source);
                        SqlExpression date = sql.Concat(year, sql.ValueFromObject("-", false, source), month, sql.ValueFromObject("-", false, source), day);
                        SqlExpression time = sql.Concat(hour, sql.ValueFromObject(":", false, source), minute, sql.ValueFromObject(":", false, source), second);
                        SqlExpression dateAndTime = sql.Concat(date, sql.ValueFromObject(' ', false, source), time);
                        if (sox.Args.Count == 6) {
                            return sql.FunctionCall(typeof(DateTime), "CONVERT", new SqlExpression[3] { datetime, dateAndTime, sql.ValueFromObject(120, false, source) }, source);
                        }
                        if ((sox.Args.Count == 7) && (sox.Args[6].ClrType == typeof(int))) {
                            // DateTime(year, month, day, hour, minute, second, millisecond ) 
                            // add leading zeros to milliseconds by RIGHT(CONVERT(NCHAR(4),1000+@ms),3) 
                            SqlExpression msRaw = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] {char4,
                                       sql.Add(sql.ValueFromObject(1000, false, source),sox.Args[6])}, source);
                            SqlExpression ms;
                            if (this.providerMode == SqlProvider.ProviderMode.SqlCE) {
                                //SqlCE doesn't have "RIGHT", so need to use "SUBSTRING"
                                SqlExpression len = sql.FunctionCall(typeof(int), "LEN", new SqlExpression[1] { msRaw }, source);
                                SqlExpression startIndex = sql.Binary(SqlNodeType.Sub, len, sql.ValueFromObject(2, false, source));
                                ms = sql.FunctionCall(typeof(string), "SUBSTRING", new SqlExpression[3] { msRaw, startIndex, sql.ValueFromObject(3, false, source) }, source);
                            }
                            else {
                                ms = sql.FunctionCall(typeof(string), "RIGHT", new SqlExpression[2] { msRaw, sql.ValueFromObject(3, false, source) }, source);
                            }
                            dateAndTime = sql.Concat(dateAndTime, sql.ValueFromObject('.', false, source), ms);
                            return sql.FunctionCall(typeof(DateTime), "CONVERT", new SqlExpression[3] { datetime, dateAndTime, sql.ValueFromObject(121, false, source) }, source);
                        }
                    }
                }
                throw Error.UnsupportedDateTimeConstructorForm();
            }

            private SqlExpression TranslateNewDateTimeOffset(SqlNew sox) {
                Expression source = sox.SourceExpression;
                if (sox.ClrType == typeof(DateTimeOffset)) {
                    // DateTimeOffset(DateTime dateTime)
                    // --> CONVERT(DATETIMEOFFSET, @dateTime)
                    if (sox.Args.Count == 1 && sox.Args[0].ClrType == typeof(DateTime)) {
                        return sql.FunctionCall(typeof(DateTimeOffset), "TODATETIMEOFFSET", 
                                                new SqlExpression[2] { sox.Args[0], sql.ValueFromObject(0, false, source) }, 
                                                source);
                    }
                    // DateTimeOffset(DateTime dateTime, TimeSpan timeSpan)
                    // --> DATEADD(DATETIMEOFFSET, @dateTimePart)
                    if (sox.Args.Count == 2 && sox.Args[0].ClrType == typeof(DateTime) && sox.Args[1].ClrType == typeof(TimeSpan)) {
                        return sql.FunctionCall(typeof(DateTimeOffset), "TODATETIMEOFFSET",
                                                new SqlExpression[2] 
                                                { 
                                                    sox.Args[0], 
                                                    sql.ConvertToInt(sql.ConvertToBigint(sql.Divide(sql.ConvertTimeToDouble(sox.Args[1]), TimeSpan.TicksPerMinute)))
                                                },
                                                source);
                    }
                    // DateTimeOffset(year, month, day, hour, minute, second, [millisecond,] timeSpan) 
                    //
                    if (sox.Args.Count >= 7 &&
                        sox.Args[0].ClrType == typeof(int) && sox.Args[1].ClrType == typeof(int) && sox.Args[2].ClrType == typeof(int) &&
                        sox.Args[3].ClrType == typeof(int) && sox.Args[4].ClrType == typeof(int) && sox.Args[5].ClrType == typeof(int)) {

                        SqlExpression char2 = sql.FunctionCall(typeof(void), "NCHAR", new SqlExpression[1] { sql.ValueFromObject(2, false, source) }, source);
                        SqlExpression char4 = sql.FunctionCall(typeof(void), "NCHAR", new SqlExpression[1] { sql.ValueFromObject(4, false, source) }, source);
                        SqlExpression char5 = sql.FunctionCall(typeof(void), "NCHAR", new SqlExpression[1] { sql.ValueFromObject(5, false, source) }, source);

                        // add leading zeros to year by RIGHT(CONVERT(NCHAR(5),10000+@ms),4) 
                        SqlExpression yyRaw = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] {char5,
                                       sql.Add(sql.ValueFromObject(10000, false, source),sox.Args[0])}, source);
                        SqlExpression year = sql.FunctionCall(typeof(string), "RIGHT", new SqlExpression[2] { yyRaw, sql.ValueFromObject(4, false, source) }, source);

                        SqlExpression month = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[1] }, source);
                        SqlExpression day = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[2] }, source);

                        SqlExpression hour = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[3] }, source);
                        SqlExpression minute = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[4] }, source);
                        SqlExpression second = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] { char2, sox.Args[5] }, source);
                        SqlExpression date = sql.Concat(year, sql.ValueFromObject("-", false, source), month, sql.ValueFromObject("-", false, source), day);
                        SqlExpression time = sql.Concat(hour, sql.ValueFromObject(":", false, source), minute, sql.ValueFromObject(":", false, source), second);

                        SqlExpression datetimeoffset = new SqlVariable(typeof(void), null, "DATETIMEOFFSET", source);
                        SqlExpression result, dateAndTime;
                        int timeSpanIndex;

                        if (sox.Args.Count == 7 && sox.Args[6].ClrType == typeof(TimeSpan)) {
                            timeSpanIndex = 6;
                            dateAndTime = sql.Concat(date, sql.ValueFromObject(' ', false, source), time);
                            result = sql.FunctionCall(typeof(DateTimeOffset), "CONVERT", new SqlExpression[3] { datetimeoffset, dateAndTime, sql.ValueFromObject(120, false, source) }, source);
                        }
                        else if (sox.Args.Count == 8 && sox.Args[6].ClrType == typeof(int) && sox.Args[7].ClrType == typeof(TimeSpan)) {
                            timeSpanIndex = 7;
                            // add leading zeros to milliseconds by RIGHT(CONVERT(NCHAR(4),1000+@ms),3) 
                            SqlExpression msRaw = sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[2] {char4,
                                       sql.Add(sql.ValueFromObject(1000, false, source),sox.Args[6])}, source);
                            SqlExpression ms = sql.FunctionCall(typeof(string), "RIGHT", new SqlExpression[2] { msRaw, sql.ValueFromObject(3, false, source) }, source);
                            dateAndTime = sql.Concat(date, sql.ValueFromObject(' ', false, source), time, sql.ValueFromObject('.', false, source), ms);
                            result = sql.FunctionCall(typeof(DateTimeOffset), "CONVERT", new SqlExpression[3] { datetimeoffset, dateAndTime, sql.ValueFromObject(121, false, source) }, source);
                        }
                        else {
                            throw Error.UnsupportedDateTimeOffsetConstructorForm();
                        }

                        return sql.FunctionCall(typeof(DateTimeOffset), "TODATETIMEOFFSET",
                                                new SqlExpression[2] 
                                                { 
                                                    result, 
                                                    sql.ConvertToInt(sql.ConvertToBigint(sql.Divide(sql.ConvertTimeToDouble(sox.Args[timeSpanIndex]), TimeSpan.TicksPerMinute)))
                                                },
                                                source);
                    }
                }
                throw Error.UnsupportedDateTimeOffsetConstructorForm();
            }

            private SqlExpression TranslateNewTimeSpan(SqlNew sox) {
                if (sox.Args.Count == 1) {
                    return sql.ConvertTo(typeof(TimeSpan), sox.Args[0]);
                }
                else if (sox.Args.Count == 3) {
                    // TimeSpan(hours, minutes, seconds)
                    SqlExpression hours = sql.ConvertToBigint(sox.Args[0]);
                    SqlExpression minutes = sql.ConvertToBigint(sox.Args[1]);
                    SqlExpression seconds = sql.ConvertToBigint(sox.Args[2]);
                    SqlExpression TicksFromHours = sql.Multiply(hours, TimeSpan.TicksPerHour);
                    SqlExpression TicksFromMinutes = sql.Multiply(minutes, TimeSpan.TicksPerMinute);
                    SqlExpression TicksFromSeconds = sql.Multiply(seconds, TimeSpan.TicksPerSecond);
                    return sql.ConvertTo(typeof(TimeSpan), sql.Add(TicksFromHours, TicksFromMinutes, TicksFromSeconds));
                }
                else {
                    SqlExpression days = sql.ConvertToBigint(sox.Args[0]);
                    SqlExpression hours = sql.ConvertToBigint(sox.Args[1]);
                    SqlExpression minutes = sql.ConvertToBigint(sox.Args[2]);
                    SqlExpression seconds = sql.ConvertToBigint(sox.Args[3]);
                    SqlExpression TicksFromDays = sql.Multiply(days, TimeSpan.TicksPerDay);
                    SqlExpression TicksFromHours = sql.Multiply(hours, TimeSpan.TicksPerHour);
                    SqlExpression TicksFromMinutes = sql.Multiply(minutes, TimeSpan.TicksPerMinute);
                    SqlExpression TicksFromSeconds = sql.Multiply(seconds, TimeSpan.TicksPerSecond);
                    SqlExpression totalTicks = sql.Add(TicksFromDays, TicksFromHours, TicksFromMinutes, TicksFromSeconds);
                    if (sox.Args.Count == 4) {
                        // TimeSpan(days, hours, minutes, seconds)
                        return sql.ConvertTo(typeof(TimeSpan), totalTicks);
                    }
                    else if (sox.Args.Count == 5) {
                        // TimeSpan(days, hours, minutes, seconds, milliseconds)
                        SqlExpression milliseconds = sql.ConvertToBigint(sox.Args[4]);
                        SqlExpression ticksFromMs = sql.Multiply(milliseconds, TimeSpan.TicksPerMillisecond);
                        return sql.ConvertTo(typeof(TimeSpan), sql.Add(totalTicks, ticksFromMs));
                    }
                }
                throw Error.UnsupportedTimeSpanConstructorForm();
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc) {
                Type declType = mc.Method.DeclaringType;
                Expression source = mc.SourceExpression;
                SqlExpression returnValue = null;
                mc.Object = this.VisitExpression(mc.Object);
                for (int i = 0, n = mc.Arguments.Count; i < n; i++) {
                    mc.Arguments[i] = this.VisitExpression(mc.Arguments[i]);
                }
                if (mc.Method.IsStatic) {
                    if (mc.Method.Name == "op_Explicit" || mc.Method.Name == "op_Implicit") {
                        if (mc.SqlType.CanBeColumn && mc.Arguments[0].SqlType.CanBeColumn) {
                            returnValue = sql.ConvertTo(mc.ClrType, mc.Arguments[0]);
                        }
                    }
                    else if (mc.Method.Name == "Compare" && mc.Arguments.Count == 2 && mc.Method.ReturnType == typeof(int)) {
                        returnValue = this.CreateComparison(mc.Arguments[0], mc.Arguments[1], mc.SourceExpression);
                    }
                    else if (declType == typeof(System.Math)) {
                        returnValue = TranslateMathMethod(mc);
                    }
                    else if (declType == typeof(System.String)) {
                        returnValue = TranslateStringStaticMethod(mc);
                    }
                    else if (declType == typeof(System.Convert)) {
                        returnValue = TranslateConvertStaticMethod(mc);
                    }
                    else if (declType == typeof(SqlMethods)) {
                        returnValue = TranslateSqlMethodsMethod(mc);
                    }
                    else if (declType == typeof(decimal)) {
                        returnValue = TranslateDecimalMethod(mc);
                    }
                    else if (IsVbConversionMethod(mc)) {
                        return TranslateVbConversionMethod(mc);
                    }
                    else if (IsVbCompareString(mc)) {
                        return TranslateVbCompareString(mc);
                    }
                    else if (IsVbLike(mc)) {
                        return TranslateVbLikeString(mc);
                    }

                    //Recognized pattern has set return value so return
                    if (returnValue != null) {
                        // Assert here to verify that actual translation stays in [....] with
                        // method support logic
                        Debug.Assert(GetMethodSupport(mc) == MethodSupport.Method);
                        return returnValue;
                    }
                }
                else { // not static   
                    if (mc.Method.Name == "Equals" && mc.Arguments.Count == 1) {
                        return sql.Binary(SqlNodeType.EQ, mc.Object, mc.Arguments[0]);
                    }
                    else if (mc.Method.Name == "GetValueOrDefault" && mc.Method.DeclaringType.IsGenericType
                               && mc.Method.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        return TranslateGetValueOrDefaultMethod(mc);
                    }
                    else if (mc.Method.Name == "ToString" && mc.Arguments.Count == 0) {
                        SqlExpression expr = mc.Object;
                        if (!expr.SqlType.IsRuntimeOnlyType) {
                            return sql.ConvertTo(typeof(string), expr);
                        }
                        throw Error.ToStringOnlySupportedForPrimitiveTypes();
                    }
                    else if (declType == typeof(string)) {
                        return TranslateStringMethod(mc);
                    }
                    else if (declType == typeof(TimeSpan)) {
                        returnValue = TranslateTimeSpanInstanceMethod(mc);
                    }
                    else if (declType == typeof(DateTime)) {
                        returnValue = TranslateDateTimeInstanceMethod(mc);
                    }
                    else if (declType == typeof(DateTimeOffset)) {
                        returnValue = TranslateDateTimeOffsetInstanceMethod(mc);
                    }
                    if (returnValue != null) {
                        // Assert here to verify that actual translation stays in [....] with
                        // method support logic
                        Debug.Assert(GetMethodSupport(mc) == MethodSupport.Method);
                        return returnValue;
                    }
                }
                throw GetMethodSupportException(mc);
            }

            internal static Exception GetMethodSupportException(SqlMethodCall mc) {
                MethodSupport ms = GetMethodSupport(mc);
                if (ms == MethodSupport.MethodGroup) {
                    // If the method is supported in some form, we want to give a
                    // different exception message to the user
                    return Error.MethodFormHasNoSupportConversionToSql(mc.Method.Name, mc.Method);
                } else {
                    // No form of the method is supported
                    return Error.MethodHasNoSupportConversionToSql(mc.Method);
                }
            }

            private SqlExpression TranslateGetValueOrDefaultMethod(SqlMethodCall mc) {
                if (mc.Arguments.Count == 0) {
                    //mc.Object.ClrType must be Nullable<T> and T is a value type
                    System.Type clrType = mc.Object.ClrType.GetGenericArguments()[0];
                    //the default value of clrType is obtained by Activator.CreateInstance(clrType)
                    return sql.Binary(SqlNodeType.Coalesce, mc.Object,
                        sql.ValueFromObject(Activator.CreateInstance(clrType), mc.SourceExpression));
                }
                else {
                    return sql.Binary(SqlNodeType.Coalesce, mc.Object, mc.Arguments[0]);
                }
            }

            private SqlExpression TranslateSqlMethodsMethod(SqlMethodCall mc) {
                Expression source = mc.SourceExpression;
                SqlExpression returnValue = null;
                string name = mc.Method.Name;
                if (name.StartsWith("DateDiff", StringComparison.Ordinal) && mc.Arguments.Count == 2) {
                    foreach (string datePart in dateParts) {
                        if (mc.Method.Name == "DateDiff" + datePart) {
                            SqlExpression start = mc.Arguments[0];
                            SqlExpression end = mc.Arguments[1];
                            SqlExpression unit = new SqlVariable(typeof(void), null, datePart, source);
                            return sql.FunctionCall(typeof(int), "DATEDIFF",
                                                    new SqlExpression[] { unit, start, end }, source);
                        }
                    }
                }
                else if (name == "Like") {
                    if (mc.Arguments.Count == 2) {
                        return sql.Like(mc.Arguments[0], mc.Arguments[1], null, source);
                    }
                    else if (mc.Arguments.Count == 3) {
                        return sql.Like(mc.Arguments[0], mc.Arguments[1], sql.ConvertTo(typeof(string), mc.Arguments[2]), source);
                    }
                }
                else if (name == "RawLength") {
                    SqlExpression length = sql.DATALENGTH(mc.Arguments[0]);
                    return length;
                }

                return returnValue;
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

            private SqlExpression TranslateDateTimeInstanceMethod(SqlMethodCall mc) {
                SqlExpression returnValue = null;
                Expression source = mc.SourceExpression;
                if (mc.Method.Name == "CompareTo") {
                    returnValue = CreateComparison(mc.Object, mc.Arguments[0], source);
                }
                else if ((mc.Method.Name == "Add" && mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan))
                       || (mc.Method.Name == "AddTicks")) {
                       // 
                       SqlExpression sqlTicks = mc.Arguments[0];
                       if (SqlFactory.IsSqlTimeType(mc.Arguments[0]))
                       {
                           SqlExpression ns = this.sql.DATEPART("NANOSECOND", mc.Arguments[0]);
                           SqlExpression ss = this.sql.DATEPART("SECOND", mc.Arguments[0]);
                           SqlExpression mm = this.sql.DATEPART("MINUTE", mc.Arguments[0]);
                           SqlExpression hh = this.sql.DATEPART("HOUR", mc.Arguments[0]);
                           sqlTicks = sql.Add(
                                        sql.Divide(ns, 100), 
                                        sql.Multiply(
                                            sql.Add(
                                                sql.Multiply(sql.ConvertToBigint(hh), 3600000), 
                                                sql.Multiply(sql.ConvertToBigint(mm), 60000), 
                                                sql.Multiply(sql.ConvertToBigint(ss), 1000)
                                            ), 
                                            10000)
                                        );
                       }
                       return this.CreateDateTimeFromDateAndTicks(mc.Object, sqlTicks, source);
                }
                else if (mc.Method.Name == "AddMonths") {
                    // date + m --> DATEADD(month, @m, @date)
                    returnValue = sql.DATEADD("MONTH", mc.Arguments[0], mc.Object);
                }
                else if (mc.Method.Name == "AddYears") {
                    // date + y --> DATEADD(year, @y, @date)
                    returnValue = sql.DATEADD("YEAR", mc.Arguments[0], mc.Object);
                }
                else if (mc.Method.Name == "AddMilliseconds") {
                    // date + ms --> DATEADD(ms, @ms, @date)
                    returnValue = this.CreateDateTimeFromDateAndMs(mc.Object, mc.Arguments[0], source);
                }
                // The following .Net methods take a double parameter, but the SQL function DATEADD only uses the integral part.
                // To make up for this, we compute the number of milliseconds and use DATEADD(ms,...) instead of DATEADD(day,...) etc.
                else if (mc.Method.Name == "AddSeconds") {
                    SqlExpression ms = sql.Multiply(mc.Arguments[0], 1000);
                    returnValue = this.CreateDateTimeFromDateAndMs(mc.Object, ms, source);
                }
                else if (mc.Method.Name == "AddMinutes") {
                    SqlExpression ms = sql.Multiply(mc.Arguments[0], 60000);
                    returnValue = this.CreateDateTimeFromDateAndMs(mc.Object, ms, source);
                }
                else if (mc.Method.Name == "AddHours") {
                    SqlExpression ms = sql.Multiply(mc.Arguments[0], 3600000);
                    returnValue = this.CreateDateTimeFromDateAndMs(mc.Object, ms, source);
                }
                else if (mc.Method.Name == "AddDays") {
                    SqlExpression ms = sql.Multiply(mc.Arguments[0], 86400000);
                    returnValue = this.CreateDateTimeFromDateAndMs(mc.Object, ms, source);
                }
                return returnValue;
            }

            private SqlExpression TranslateDateTimeOffsetInstanceMethod(SqlMethodCall mc) {
                SqlExpression returnValue = null;
                Expression source = mc.SourceExpression;
                if (mc.Method.Name == "CompareTo") {
                    returnValue = CreateComparison(mc.Object, mc.Arguments[0], source);
                }
                else if ((mc.Method.Name == "Add" && mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan))
                       || (mc.Method.Name == "AddTicks")) {
                           SqlExpression ns = sql.DATEPART("NANOSECOND", mc.Arguments[0]);
                           SqlExpression ss = sql.DATEPART("SECOND", mc.Arguments[0]);
                           SqlExpression mi = sql.DATEPART("MINUTE", mc.Arguments[0]);
                           SqlExpression hh = sql.DATEPART("HOUR", mc.Arguments[0]);

                           SqlExpression ticks = sql.Add(
                              sql.Divide(ns, 100),
                              sql.Multiply(
                                          sql.Add(
                                              sql.Multiply(sql.ConvertToBigint(hh), 3600000),
                                              sql.Multiply(sql.ConvertToBigint(mi), 60000),
                                              sql.Multiply(sql.ConvertToBigint(ss), 1000)
                                            ),
                                          10000   // 1 millisecond = 10000 ticks
                                      )
                              );
                   returnValue = this.CreateDateTimeOffsetFromDateAndTicks(mc.Object, ticks, source);
                }
                else if (mc.Method.Name == "AddMonths") {
                    // date + m --> DATEADD(month, @m, @date)
                    returnValue = sql.DATETIMEOFFSETADD("MONTH", mc.Arguments[0], mc.Object);
                }
                else if (mc.Method.Name == "AddYears") {
                    // date + y --> DATEADD(year, @y, @date)
                    returnValue = sql.DATETIMEOFFSETADD("YEAR", mc.Arguments[0], mc.Object);
                }
                else if (mc.Method.Name == "AddMilliseconds") {
                    // date + ms --> DATEADD(ms, @ms, @date)
                    returnValue = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, mc.Arguments[0], source);
                }
                // The following .Net methods take a double parameter, but the SQL function DATEADD only uses the integral part.
                // To make up for this, we compute the number of milliseconds and use DATEADD(ms,...) instead of DATEADD(day,...) etc.
                else if (mc.Method.Name == "AddSeconds") {
                    SqlExpression ms = sql.Multiply(mc.Arguments[0], 1000);
                    returnValue = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, ms, source);
                }
                else if (mc.Method.Name == "AddMinutes") {
                    SqlExpression ms = sql.Multiply(mc.Arguments[0], 60000);
                    returnValue = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, ms, source);
                }
                else if (mc.Method.Name == "AddHours") {
                    SqlExpression ms = sql.Multiply(mc.Arguments[0], 3600000);
                    returnValue = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, ms, source);
                }
                else if (mc.Method.Name == "AddDays") {
                    SqlExpression ms = sql.Multiply(mc.Arguments[0], 86400000);
                    returnValue = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, ms, source);
                }
                return returnValue;
            }

            private SqlExpression TranslateTimeSpanInstanceMethod(SqlMethodCall mc) {
                SqlExpression returnValue = null;
                Expression source = mc.SourceExpression;
                if (mc.Method.Name == "Add") {
                    returnValue = sql.Add(mc.Object, mc.Arguments[0]);
                }
                else if (mc.Method.Name == "Subtract") {
                    returnValue = sql.Subtract(mc.Object, mc.Arguments[0]);
                }
                else if (mc.Method.Name == "CompareTo") {
                    returnValue = CreateComparison(mc.Object, mc.Arguments[0], source);
                }
                else if (mc.Method.Name == "Duration") {
                    if (SqlFactory.IsSqlTimeType(mc.Object))
                        return mc.Object;

                    returnValue = sql.FunctionCall(typeof(TimeSpan), "ABS", new SqlExpression[] { mc.Object }, source);
                }
                else if (mc.Method.Name == "Negate") {
                    returnValue = sql.Unary(SqlNodeType.Negate, mc.Object, source);
                }
                return returnValue;
            }

            private SqlExpression TranslateConvertStaticMethod(SqlMethodCall mc) {
                SqlExpression returnValue = null;
                if (mc.Arguments.Count == 1) {
                    SqlExpression expr = mc.Arguments[0];
                    Type targetType = null;
                    ProviderType providerType = null;
                    switch (mc.Method.Name) {
                        case "ToBoolean":
                            targetType = typeof(bool);
                            break;
                        case "ToDecimal":
                            targetType = typeof(decimal);
                            break;
                        case "ToByte":
                            targetType = typeof(byte);
                            break;
                        case "ToChar": {
                                targetType = typeof(char);
                                if (expr.SqlType.IsChar) {
                                    providerType = sql.TypeProvider.From(targetType, 1);
                                }
                                break;
                            }
                        case "ToDateTime":
                            Type nnType = TypeSystem.GetNonNullableType(expr.ClrType);
                            if (nnType == typeof(string) || nnType == typeof(DateTime)) {
                                targetType = typeof(DateTime);
                            }
                            else {
                                throw Error.ConvertToDateTimeOnlyForDateTimeOrString();
                            }
                            break;
                        // not applicable: "ToDateTimeOffset"
                        case "ToDouble":
                            targetType = typeof(double);
                            break;
                        case "ToInt16":
                            targetType = typeof(Int16);
                            break;
                        case "ToInt32":
                            targetType = typeof(Int32);
                            break;
                        case "ToInt64":
                            targetType = typeof(Int64);
                            break;
                        case "ToSingle":
                            targetType = typeof(float);
                            break;
                        case "ToString":
                            targetType = typeof(string);
                            break;
                        // Unsupported
                        case "ToSByte":
                        case "ToUInt16":
                        case "ToUInt32":
                        case "ToUInt64":
                        default:
                            throw GetMethodSupportException(mc);
                    }
                    // Since boolean literals and Int32 both map to the same provider type, we must
                    // special case boolean types so we don't miss conversions.  Below we catch
                    // conversions from bool->int (Convert.ToInt32(bool)) and ensure the conversion
                    // remains.
                    if (sql.TypeProvider.From(targetType) != expr.SqlType ||
                        (expr.ClrType == typeof(bool) && targetType == typeof(int))) {
                        // do the conversions that would be done for a cast "(<targetType>) expression"
                        returnValue = sql.ConvertTo(targetType, expr);
                    }
                    else if (targetType != null) {
                        if (sql.TypeProvider.From(targetType) != expr.SqlType) {
                            // do the conversions that would be done for a cast "(<targetType>) expression"
                            returnValue = sql.ConvertTo(targetType, expr);
                        }
                        else if (targetType != expr.ClrType &&
                            (TypeSystem.GetNonNullableType(targetType) == TypeSystem.GetNonNullableType(expr.ClrType))) {
                            // types are same except for nullability, so lift the type
                            returnValue = new SqlLift(targetType, expr, expr.SourceExpression);
                        }
                        else {
                            returnValue = expr;
                        }
                    }
                }
                return returnValue;
            }

            private SqlExpression TranslateDateTimeBinary(SqlBinary bo) {
                bool resultNullable = TypeSystem.IsNullableType(bo.ClrType);
                Type rightType = TypeSystem.GetNonNullableType(bo.Right.ClrType);
                switch (bo.NodeType) {
                    case SqlNodeType.Sub:
                        if (rightType == typeof(DateTime)) {
                            // if either of the arguments is nullable, set result type to nullable.
                            Type resultType = bo.ClrType;
                            SqlExpression end = bo.Left;
                            SqlExpression start = bo.Right;
                            SqlExpression day = new SqlVariable(typeof(void), null, "DAY", bo.SourceExpression);
                            SqlExpression ms = new SqlVariable(typeof(void), null, "MILLISECOND", bo.SourceExpression);

                            // DATEDIFF(MILLISECONDS,...) does not work for more then 24 days, since result has to fit int. 
                            // So compute the number of days first, and then find out the number of milliseconds needed in addition to that.
                            SqlExpression intDays = sql.FunctionCall(typeof(int), "DATEDIFF",
                                                        new SqlExpression[] { day, start, end }, bo.SourceExpression);
                            SqlExpression startPlusDays = sql.FunctionCall(
                                                          typeof(DateTime), "DATEADD", new SqlExpression[] { day, intDays, start }, bo.SourceExpression);
                            SqlExpression intMSec = sql.FunctionCall(typeof(int), "DATEDIFF",
                                                        new SqlExpression[] { ms, startPlusDays, end }, bo.SourceExpression);
                            SqlExpression result = sql.Multiply(sql.Add(sql.Multiply(sql.ConvertToBigint(intDays), 86400000), intMSec), 10000); // 1 millisecond = 10000 ticks
                            return sql.ConvertTo(resultType, result);
                        }
                        if (rightType == typeof(DateTimeOffset)) {
                            Debug.Assert(TypeSystem.GetNonNullableType(bo.Left.ClrType) == typeof(DateTimeOffset));
                            // if either of the arguments is nullable, set result type to nullable.
                            Type resultType = bo.ClrType;
                            SqlExpression end = bo.Left;
                            SqlExpression start = bo.Right;
                            SqlExpression day = new SqlVariable(typeof(void), null, "DAY", bo.SourceExpression);
                            SqlExpression ms = new SqlVariable(typeof(void), null, "MILLISECOND", bo.SourceExpression);
                            SqlExpression us = new SqlVariable(typeof(void), null, "MICROSECOND", bo.SourceExpression);
                            SqlExpression ns = new SqlVariable(typeof(void), null, "NANOSECOND", bo.SourceExpression);

                            // compute the number of days first, and then find out the number of milliseconds needed in addition to that.
                            SqlExpression intDays = sql.FunctionCall(typeof(int), "DATEDIFF", new SqlExpression[] { day, start, end }, bo.SourceExpression);
                            SqlExpression startPlusDays = sql.FunctionCall(typeof(DateTimeOffset), "DATEADD", new SqlExpression[] { day, intDays, start }, bo.SourceExpression);
                            SqlExpression intMSec = sql.FunctionCall(typeof(int), "DATEDIFF", new SqlExpression[] { ms, startPlusDays, end }, bo.SourceExpression);
                            SqlExpression startPlusDaysPlusMsec = sql.FunctionCall(typeof(DateTimeOffset), "DATEADD", new SqlExpression[] { ms, intMSec, startPlusDays }, bo.SourceExpression);
                            SqlExpression intUSec = sql.FunctionCall(typeof(int), "DATEDIFF", new SqlExpression[] { us, startPlusDaysPlusMsec, end }, bo.SourceExpression);
                            SqlExpression startPlusDaysPlusMsecPlusUSec = sql.FunctionCall(typeof(DateTimeOffset), "DATEADD", new SqlExpression[] { us, intUSec, startPlusDaysPlusMsec }, bo.SourceExpression);
                            SqlExpression intNSec = sql.FunctionCall(typeof(int), "DATEDIFF", new SqlExpression[] { ns, startPlusDaysPlusMsecPlusUSec, end }, bo.SourceExpression);
                            SqlExpression startPlusDaysPlusMsecPlusUSecPlusNSec = sql.FunctionCall(typeof(DateTimeOffset), "DATEADD", new SqlExpression[] { ns, intNSec, startPlusDaysPlusMsecPlusUSec }, bo.SourceExpression);

                            SqlExpression result = sql.Add(
                                                        sql.Divide(intNSec, 100),
                                                        sql.Multiply(intUSec, 10),
                                                        sql.Multiply(
                                                            sql.Add(
                                                                sql.Multiply(sql.ConvertToBigint(intDays), 86400000), 
                                                                intMSec
                                                            ), 
                                                            10000)
                                                   );

                            return sql.ConvertTo(resultType, result);
                        }
                        else if (rightType == typeof(TimeSpan)) {
                            SqlExpression right = bo.Right;
                            if (SqlFactory.IsSqlTimeType(bo.Right)) {
                                SqlExpression ns = sql.DATEPART("NANOSECOND", right);
                                SqlExpression ss = sql.DATEPART("SECOND", right);
                                SqlExpression mi = sql.DATEPART("MINUTE", right);
                                SqlExpression hh = sql.DATEPART("HOUR", right);

                                right = sql.Add(
                                            sql.Divide(ns, 100),
                                            sql.Multiply(
                                                        sql.Add(
                                                            sql.Multiply(sql.ConvertToBigint(hh), 3600000),
                                                            sql.Multiply(sql.ConvertToBigint(mi), 60000),
                                                            sql.Multiply(sql.ConvertToBigint(ss), 1000)
                                                          ),
                                                        10000   // 1 millisecond = 10000 ticks
                                                    )
                                            );
                            } 
                            
                            return TypeSystem.GetNonNullableType(bo.Left.ClrType) == typeof(DateTimeOffset) ?           
                                                CreateDateTimeOffsetFromDateAndTicks(
                                                    bo.Left,
                                                    sql.Unary(SqlNodeType.Negate, right, bo.SourceExpression),
                                                    bo.SourceExpression, resultNullable
                                                ) :
                                                CreateDateTimeFromDateAndTicks(
                                                    bo.Left,
                                                    sql.Unary(SqlNodeType.Negate, right, bo.SourceExpression),
                                                    bo.SourceExpression, resultNullable
                                                );
                        }
                        break;
                    case SqlNodeType.Add:
                        if (rightType == typeof(TimeSpan)) {
                            if (SqlFactory.IsSqlTimeType(bo.Right)) {
                                return sql.AddTimeSpan(bo.Left, bo.Right, resultNullable);
                            } else if (TypeSystem.GetNonNullableType(bo.Left.ClrType) == typeof(DateTimeOffset)) {
                                return CreateDateTimeOffsetFromDateAndTicks(bo.Left, bo.Right, bo.SourceExpression, resultNullable);
                            }

                            return CreateDateTimeFromDateAndTicks(bo.Left, bo.Right, bo.SourceExpression, resultNullable);
                        }
                        break;
                }
                return bo;
            }

            internal SqlExpression TranslateDecimalMethod(SqlMethodCall mc) {
                Expression source = mc.SourceExpression;
                if (mc.Method.IsStatic) {
                    if (mc.Arguments.Count == 2) {
                        switch (mc.Method.Name) {
                            case "Multiply":
                                return sql.Binary(SqlNodeType.Mul, mc.Arguments[0], mc.Arguments[1]);
                            case "Divide":
                                return sql.Binary(SqlNodeType.Div, mc.Arguments[0], mc.Arguments[1]);
                            case "Subtract":
                                return sql.Binary(SqlNodeType.Sub, mc.Arguments[0], mc.Arguments[1]);
                            case "Add":
                                return sql.Binary(SqlNodeType.Add, mc.Arguments[0], mc.Arguments[1]);
                            case "Remainder":
                                return sql.Binary(SqlNodeType.Mod, mc.Arguments[0], mc.Arguments[1]);
                            case "Round":
                                // ROUND (x, y)
                                return sql.FunctionCall(mc.Method.ReturnType, "ROUND", mc.Arguments, mc.SourceExpression);
                        }
                    }
                    else if (mc.Arguments.Count == 1) {
                        switch (mc.Method.Name) {
                            case "Negate":
                                return sql.Unary(SqlNodeType.Negate, mc.Arguments[0], source);
                            case "Floor":
                            case "Truncate":
                                // Truncate(x) --> ROUND (x, 0, 1)
                                return sql.FunctionCall(mc.Method.ReturnType, "ROUND",
                                    new SqlExpression[] { 
                                        mc.Arguments[0], 
                                        sql.ValueFromObject(0, false, mc.SourceExpression), 
                                        sql.ValueFromObject(1, false, mc.SourceExpression) 
                                    },
                                    mc.SourceExpression);
                            case "Round":
                                // ROUND (x, 0)
                                return sql.FunctionCall(mc.Method.ReturnType, "ROUND",
                                    new SqlExpression[] { 
                                        mc.Arguments[0], 
                                        sql.ValueFromObject(0, false, mc.SourceExpression) 
                                    },
                                    mc.SourceExpression);
                            default:
                                if (mc.Method.Name.StartsWith("To", StringComparison.Ordinal)) {
                                    return this.TranslateConvertStaticMethod(mc);
                                }
                                break;
                        }
                    }
                }
                throw GetMethodSupportException(mc);
            }

            private SqlExpression TranslateStringStaticMethod(SqlMethodCall mc) {
                SqlExpression returnValue = null;
                Expression source = mc.SourceExpression;
                #region String Methods
                if (mc.Method.Name == "Concat") {
                    SqlClientArray arr = mc.Arguments[0] as SqlClientArray;
                    List<SqlExpression> exprs = null;
                    if (arr != null) {
                        exprs = arr.Expressions;
                    }
                    else {
                        exprs = mc.Arguments;
                    }
                    if (exprs.Count == 0) {
                        returnValue = sql.ValueFromObject("", false, source);
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
                        returnValue = sum;
                    }
                }
                else if ((mc.Method.Name == "Equals") && (mc.Arguments.Count == 2)) {
                    returnValue = sql.Binary(SqlNodeType.EQ2V, mc.Arguments[0], mc.Arguments[1]);
                }
                else if ((mc.Method.Name == "Compare") && (mc.Arguments.Count == 2)) {
                    returnValue = CreateComparison(mc.Arguments[0], mc.Arguments[1], source);
                }
                #endregion
                throw GetMethodSupportException(mc);
                //return returnValue;
            }

            //helper functions
            private SqlExpression CreateFunctionCallStatic1(Type type, string functionName, List<SqlExpression> arguments, Expression source) {
                return sql.FunctionCall(type, functionName, new SqlExpression[] { arguments[0] }, source);
            }

            private SqlExpression CreateFunctionCallStatic2(Type type, string functionName, List<SqlExpression> arguments, Expression source) {
                return sql.FunctionCall(type, functionName, new SqlExpression[] { arguments[0], arguments[1] }, source);
            }

            private SqlExpression TranslateStringMethod(SqlMethodCall mc) {
                Expression source = mc.SourceExpression;

                switch (mc.Method.Name) {
                    case "Contains":
                        if (mc.Arguments.Count == 1) {
                            SqlExpression pattern = mc.Arguments[0];
                            SqlExpression escape = null;
                            bool needsEscape = true;

                            if (pattern.NodeType == SqlNodeType.Value) {
                                string unescapedText = (string)((SqlValue)pattern).Value;
                                string patternText = SqlHelpers.GetStringContainsPattern(unescapedText, '~', out needsEscape);
                                pattern = sql.ValueFromObject(patternText, true, pattern.SourceExpression);
                            }
                            else if (pattern.NodeType == SqlNodeType.ClientParameter) {
                                SqlClientParameter cp = (SqlClientParameter)pattern;
                                Func<string, char, string> getStringContainsPatternForced = SqlHelpers.GetStringContainsPatternForced;
                                pattern = new SqlClientParameter(
                                    cp.ClrType, cp.SqlType,
                                    Expression.Lambda(
                                        Expression.Invoke(Expression.Constant(getStringContainsPatternForced), cp.Accessor.Body, Expression.Constant('~')),
                                        cp.Accessor.Parameters[0]
                                        ),
                                    cp.SourceExpression
                                    );
                            }
                            else {
                                throw Error.NonConstantExpressionsNotSupportedFor("String.Contains");
                            }

                            if (needsEscape) {
                                escape = sql.ValueFromObject("~", false, source);
                            }

                            return sql.Like(mc.Object, pattern, escape, source);
                        }
                        break;
                    case "StartsWith":
                        if (mc.Arguments.Count == 1) {
                            SqlExpression pattern = mc.Arguments[0];
                            SqlExpression escape = null;
                            bool needsEscape = true;

                            if (pattern.NodeType == SqlNodeType.Value) {
                                string unescapedText = (string)((SqlValue)pattern).Value;
                                string patternText = SqlHelpers.GetStringStartsWithPattern(unescapedText, '~', out needsEscape);
                                pattern = sql.ValueFromObject(patternText, true, pattern.SourceExpression);
                            }
                            else if (pattern.NodeType == SqlNodeType.ClientParameter) {
                                SqlClientParameter cp = (SqlClientParameter)pattern;
                                Func<string, char, string> getStringStartsWithPatternForced = SqlHelpers.GetStringStartsWithPatternForced;
                                pattern = new SqlClientParameter(
                                    cp.ClrType, cp.SqlType,
                                    Expression.Lambda(
                                        Expression.Invoke(Expression.Constant(getStringStartsWithPatternForced), cp.Accessor.Body, Expression.Constant('~')),
                                             cp.Accessor.Parameters[0]
                                        ),
                                    cp.SourceExpression
                                    );
                            }
                            else {
                                throw Error.NonConstantExpressionsNotSupportedFor("String.StartsWith");
                            }

                            if (needsEscape) {
                                escape = sql.ValueFromObject("~", false, source);
                            }

                            return sql.Like(mc.Object, pattern, escape, source);
                        }
                        break;
                    case "EndsWith":
                        if (mc.Arguments.Count == 1) {
                            SqlExpression pattern = mc.Arguments[0];
                            SqlExpression escape = null;
                            bool needsEscape = true;

                            if (pattern.NodeType == SqlNodeType.Value) {
                                string unescapedText = (string)((SqlValue)pattern).Value;
                                string patternText = SqlHelpers.GetStringEndsWithPattern(unescapedText, '~', out needsEscape);
                                pattern = sql.ValueFromObject(patternText, true, pattern.SourceExpression);
                            }
                            else if (pattern.NodeType == SqlNodeType.ClientParameter) {
                                SqlClientParameter cp = (SqlClientParameter)pattern;
                                Func<string, char, string> getStringEndsWithPatternForced = SqlHelpers.GetStringEndsWithPatternForced;
                                pattern = new SqlClientParameter(
                                    cp.ClrType, cp.SqlType,
                                    Expression.Lambda(
                                        Expression.Invoke(Expression.Constant(getStringEndsWithPatternForced), cp.Accessor.Body, Expression.Constant('~')),
                                        cp.Accessor.Parameters[0]
                                        ),
                                    cp.SourceExpression
                                    );
                            }
                            else {
                                throw Error.NonConstantExpressionsNotSupportedFor("String.EndsWith");
                            }

                            if (needsEscape) {
                                escape = sql.ValueFromObject("~", false, source);
                            }

                            return sql.Like(mc.Object, pattern, escape, source);
                        }
                        break;
                    case "IndexOf":
                        if (mc.Arguments.Count == 1) {
                            if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null) {
                                throw Error.ArgumentNull("value");
                            }
                            // if the search string is empty, return zero
                            SqlExpression lenZeroExpr = sql.Binary(SqlNodeType.EQ, sql.CLRLENGTH(mc.Arguments[0]), sql.ValueFromObject(0, source));
                            SqlWhen when = new SqlWhen(lenZeroExpr, sql.ValueFromObject(0, source));
                            SqlExpression @else = sql.Subtract(sql.FunctionCall(typeof(int), "CHARINDEX",
                                        new SqlExpression[] { 
                                        mc.Arguments[0], 
                                        mc.Object },
                                    source), 1);
                            return sql.SearchedCase(new SqlWhen[] { when }, @else, source);

                        }
                        else if (mc.Arguments.Count == 2) {
                            if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null) {
                                throw Error.ArgumentNull("value");
                            }
                            if (mc.Arguments[1].ClrType == typeof(StringComparison)) {
                                throw Error.IndexOfWithStringComparisonArgNotSupported();
                            }
                            // if the search string is empty and the start index is in bounds,
                            // return the start index
                            SqlExpression lenZeroExpr = sql.Binary(SqlNodeType.EQ, sql.CLRLENGTH(mc.Arguments[0]), sql.ValueFromObject(0, source));
                            lenZeroExpr = sql.AndAccumulate(lenZeroExpr, sql.Binary(SqlNodeType.LE, sql.Add(mc.Arguments[1], 1), sql.CLRLENGTH(mc.Object)));
                            SqlWhen when = new SqlWhen(lenZeroExpr, mc.Arguments[1]);
                            SqlExpression @else = sql.Subtract(sql.FunctionCall(typeof(int), "CHARINDEX",
                                        new SqlExpression[] { 
                                        mc.Arguments[0], 
                                        mc.Object,
                                        sql.Add(mc.Arguments[1], 1)
                                        },
                                    source), 1);
                            return sql.SearchedCase(new SqlWhen[] { when }, @else, source);
                        }
                        else if (mc.Arguments.Count == 3) {
                            if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null) {
                                throw Error.ArgumentNull("value");
                            }
                            if (mc.Arguments[2].ClrType == typeof(StringComparison)) {
                                throw Error.IndexOfWithStringComparisonArgNotSupported();
                            }

                            // s1.IndexOf(s2, start, count) -> CHARINDEX(@s2, SUBSTRING(@s1, 1, @start + @count), @start + 1)
                            // if the search string is empty and the start index is in bounds,
                            // return the start index
                            SqlExpression lenZeroExpr = sql.Binary(SqlNodeType.EQ, sql.CLRLENGTH(mc.Arguments[0]), sql.ValueFromObject(0, source));
                            lenZeroExpr = sql.AndAccumulate(lenZeroExpr, sql.Binary(SqlNodeType.LE, sql.Add(mc.Arguments[1], 1), sql.CLRLENGTH(mc.Object)));
                            SqlWhen when = new SqlWhen(lenZeroExpr, mc.Arguments[1]);
                            SqlExpression substring = sql.FunctionCall(
                                typeof(string), "SUBSTRING",
                                new SqlExpression[] {
                                    mc.Object,
                                    sql.ValueFromObject(1, false, source),
                                    sql.Add(mc.Arguments[1], mc.Arguments[2])
                                    },
                                    source);
                            SqlExpression @else = sql.Subtract(sql.FunctionCall(typeof(int), "CHARINDEX",
                                    new SqlExpression[] { 
                                        mc.Arguments[0], 
                                        substring,
                                        sql.Add(mc.Arguments[1], 1)
                                        },
                                    source), 1);
                            return sql.SearchedCase(new SqlWhen[] { when }, @else, source);
                        }
                        break;
                    case "LastIndexOf":
                        if (mc.Arguments.Count == 1) {
                            // s.LastIndexOf(part) -->
                            // CASE WHEN CHARINDEX(@part, @s) = 0  THEN  -1
                            //      ELSE 1 + CLRLENGTH(@s) - CLRLENGTH(@part) - CHARINDEX(REVERSE(@part),REVERSE(@s))
                            // END
                            SqlExpression exprPart = mc.Arguments[0];
                            if (exprPart is SqlValue && ((SqlValue)exprPart).Value == null) {
                                throw Error.ArgumentNull("value");
                            }
                            SqlExpression exprS = mc.Object;
                            SqlExpression reverseS = sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { exprS }, source);
                            SqlExpression reversePart = sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { exprPart }, source);
                            SqlExpression charIndex = sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { exprPart, exprS }, source);
                            SqlExpression charIndexOfReverse = sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { reversePart, reverseS }, source);
                            SqlExpression notContained = sql.Binary(SqlNodeType.EQ, charIndex, sql.ValueFromObject(0, false, source));
                            SqlExpression len1 = sql.CLRLENGTH(exprS);
                            SqlExpression len2 = sql.CLRLENGTH(exprPart);
                            SqlExpression elseCase = sql.Add(sql.ValueFromObject(1, false, source), sql.Subtract(len1, sql.Add(len2, charIndexOfReverse)));

                            SqlWhen whenNotContained = new SqlWhen(notContained, sql.ValueFromObject(-1, false, source));

                            // if the search string is empty, return zero
                            SqlExpression lenZeroExpr = sql.Binary(SqlNodeType.EQ, sql.CLRLENGTH(mc.Arguments[0]), sql.ValueFromObject(0, source));
                            SqlWhen whenLenZero = new SqlWhen(lenZeroExpr, sql.Subtract(sql.CLRLENGTH(exprS), 1));

                            return sql.SearchedCase(new SqlWhen[] { whenLenZero, whenNotContained },
                                elseCase, source);
                        }
                        else if (mc.Arguments.Count == 2) {
                            // s.LastIndexOf(part,i) -->
                            // set @first = LEFT(@s, @i+1)
                            // CASE WHEN CHARINDEX(@part, @first) = 0  THEN  -1
                            //      ELSE 1 + CLRLENGTH(@first) - CLRLENGTH(@part) - CHARINDEX(REVERSE(@part),REVERSE(@first))
                            // END
                            if (mc.Arguments[1].ClrType == typeof(StringComparison)) {
                                throw Error.LastIndexOfWithStringComparisonArgNotSupported();
                            }
                            SqlExpression s = mc.Object;
                            SqlExpression part = mc.Arguments[0];
                            if (part is SqlValue && ((SqlValue)part).Value == null) {
                                throw Error.ArgumentNull("value");
                            }
                            SqlExpression i = mc.Arguments[1];
                            SqlExpression first = sql.FunctionCall(typeof(string), "LEFT", new SqlExpression[] { s, sql.Add(i, 1) }, source);
                            SqlExpression reverseFirst = sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { first }, source);
                            SqlExpression reversePart = sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { part }, source);
                            SqlExpression charIndex = sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { part, first }, source);
                            SqlExpression charIndexOfReverse = sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { reversePart, reverseFirst }, source);
                            SqlExpression notContained = sql.Binary(SqlNodeType.EQ, charIndex, sql.ValueFromObject(0, false, source));
                            SqlExpression len1 = sql.CLRLENGTH(first);
                            SqlExpression len2 = sql.CLRLENGTH(part);
                            SqlExpression elseCase = sql.Add(sql.ValueFromObject(1, false, source), sql.Subtract(len1, sql.Add(len2, charIndexOfReverse)));

                            SqlWhen whenNotContained = new SqlWhen(notContained, sql.ValueFromObject(-1, false, source));

                            // if the search string is empty and the start index is in bounds,
                            // return the start index
                            SqlExpression lenZeroExpr = sql.Binary(SqlNodeType.EQ, sql.CLRLENGTH(mc.Arguments[0]), sql.ValueFromObject(0, source));
                            lenZeroExpr = sql.AndAccumulate(lenZeroExpr, sql.Binary(SqlNodeType.LE, sql.Add(mc.Arguments[1], 1), sql.CLRLENGTH(s)));
                            SqlWhen whenLenZero = new SqlWhen(lenZeroExpr, mc.Arguments[1]);

                            return sql.SearchedCase(new SqlWhen[] { whenLenZero, whenNotContained },
                                elseCase, source);
                        }
                        else if (mc.Arguments.Count == 3) {
                            // s.LastIndexOf(part, i, count) -->
                            // set @first = LEFT(@s, @i+1)
                            // CASE WHEN (CHARINDEX(@part, @first) = 0)  OR (1 + CLRLENGTH(@first) - CLRLENGTH(@part) - CHARINDEX(REVERSE(@part),REVERSE(@first))) < (@i - @count) THEN  -1
                            //      ELSE 1 + CLRLENGTH(@first) - CLRLENGTH(@part) - CHARINDEX(REVERSE(@part),REVERSE(@first))
                            // END
                            if (mc.Arguments[2].ClrType == typeof(StringComparison)) {
                                throw Error.LastIndexOfWithStringComparisonArgNotSupported();
                            }
                            SqlExpression s = mc.Object;
                            SqlExpression part = mc.Arguments[0];
                            if (part is SqlValue && ((SqlValue)part).Value == null) {
                                throw Error.ArgumentNull("value");
                            }
                            SqlExpression i = mc.Arguments[1];
                            SqlExpression count = mc.Arguments[2];
                            SqlExpression first = sql.FunctionCall(typeof(string), "LEFT", new SqlExpression[] { s, sql.Add(i, 1) }, source);
                            SqlExpression reverseFirst = sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { first }, source);
                            SqlExpression reversePart = sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { part }, source);
                            SqlExpression charIndex = sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { part, first }, source);
                            SqlExpression charIndexOfReverse = sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { reversePart, reverseFirst }, source);
                            SqlExpression len1 = sql.CLRLENGTH(first);
                            SqlExpression len2 = sql.CLRLENGTH(part);
                            SqlExpression elseCase = sql.Add(sql.ValueFromObject(1, false, source), sql.Subtract(len1, sql.Add(len2, charIndexOfReverse)));
                            SqlExpression notContained = sql.Binary(SqlNodeType.EQ, charIndex, sql.ValueFromObject(0, false, source));
                            notContained = sql.OrAccumulate(notContained, sql.Binary(SqlNodeType.LE, elseCase, sql.Subtract(i, count)));

                            SqlWhen whenNotContained = new SqlWhen(notContained, sql.ValueFromObject(-1, false, source));

                            // if the search string is empty and the start index is in bounds,
                            // return the start index
                            SqlExpression lenZeroExpr = sql.Binary(SqlNodeType.EQ, sql.CLRLENGTH(mc.Arguments[0]), sql.ValueFromObject(0, source));
                            lenZeroExpr = sql.AndAccumulate(lenZeroExpr, sql.Binary(SqlNodeType.LE, sql.Add(mc.Arguments[1], 1), sql.CLRLENGTH(s)));
                            SqlWhen whenLenZero = new SqlWhen(lenZeroExpr, mc.Arguments[1]);

                            return sql.SearchedCase(new SqlWhen[] { whenLenZero, whenNotContained },
                                elseCase, source);
                        }
                        break;
                    case "Insert":
                        // Create STUFF(str, insertPos + 1, 0, strToInsert)
                        if (mc.Arguments.Count == 2) {
                            if (mc.Arguments[1] is SqlValue && ((SqlValue)mc.Arguments[1]).Value == null) {
                                throw Error.ArgumentNull("value");
                            }
                            SqlFunctionCall stuffCall = sql.FunctionCall(
                                typeof(string), "STUFF",
                                new SqlExpression[] {
                                    mc.Object,
                                    sql.Add(mc.Arguments[0], 1),
                                    sql.ValueFromObject(0, false, source),
                                    mc.Arguments[1]                                    
                                },
                                source);
                            // We construct SQL to handle the special case of when the length of the string
                            // to modify is equal to the insert position.  This occurs if the string is empty and
                            // the insert pos is 0, or when the string is not empty, and the insert pos indicates
                            // the end of the string.
                            // CASE WHEN (CLRLENGTH(str) = insertPos) THEN str + strToInsert ELSE STUFF(...)                       
                            SqlExpression insertingAtEnd = sql.Binary(SqlNodeType.EQ, sql.CLRLENGTH(mc.Object), mc.Arguments[0]);
                            SqlExpression stringConcat = sql.Concat(mc.Object, mc.Arguments[1]);

                            return sql.SearchedCase(new SqlWhen[] { new SqlWhen(insertingAtEnd, stringConcat) }, stuffCall, source);
                        }
                        break;
                    case "PadLeft":
                        if (mc.Arguments.Count == 1) {
                            // s.PadLeft(i) -->
                            // CASE WHEN CLRLENGTH(@s)>= @i THEN @s
                            //      ELSE SPACE(@i-CLRLENGTH(@s)) + @s 
                            // END
                            SqlExpression exprS = mc.Object;
                            SqlExpression exprI = mc.Arguments[0];
                            SqlExpression len2 = sql.CLRLENGTH(exprS);
                            SqlExpression dontChange = sql.Binary(SqlNodeType.GE, len2, exprI);
                            SqlExpression numSpaces = sql.Subtract(exprI, len2);
                            SqlExpression padding = sql.FunctionCall(typeof(string), "SPACE", new SqlExpression[] { numSpaces }, source);
                            SqlExpression elseCase = sql.Concat(padding, exprS);

                            return sql.SearchedCase(new SqlWhen[] { new SqlWhen(dontChange, exprS) }, elseCase, source);
                        }
                        else if (mc.Arguments.Count == 2) {
                            // s.PadLeft(i,c) -->
                            // CASE WHEN CLRLENGTH(@s) >= @i THEN @s
                            //      ELSE REPLICATE(@c, @i - CLRLENGTH(@s)) + @s 
                            // END
                            SqlExpression exprS = mc.Object;
                            SqlExpression exprI = mc.Arguments[0];
                            SqlExpression exprC = mc.Arguments[1];
                            SqlExpression dontChange = sql.Binary(SqlNodeType.GE, sql.CLRLENGTH(exprS), exprI);
                            SqlExpression len2 = sql.CLRLENGTH(exprS);
                            SqlExpression numSpaces = sql.Subtract(exprI, len2);
                            SqlExpression padding = sql.FunctionCall(typeof(string), "REPLICATE", new SqlExpression[] { exprC, numSpaces }, source);
                            SqlExpression elseCase = sql.Concat(padding, exprS);

                            return sql.SearchedCase(new SqlWhen[] { new SqlWhen(dontChange, exprS) }, elseCase, source);
                        }
                        break;
                    case "PadRight":
                        if (mc.Arguments.Count == 1) {
                            // s.PadRight(i) -->
                            // CASE WHEN CLRLENGTH(@s) >= @i THEN @s
                            //      ELSE @s + SPACE(@i - CLRLENGTH(@s)) 
                            // END
                            SqlExpression exprS = mc.Object;
                            SqlExpression exprI = mc.Arguments[0];
                            SqlExpression dontChange = sql.Binary(SqlNodeType.GE, sql.CLRLENGTH(exprS), exprI);
                            SqlExpression len2 = sql.CLRLENGTH(exprS);
                            SqlExpression numSpaces = sql.Subtract(exprI, len2);
                            SqlExpression padding = sql.FunctionCall(typeof(string), "SPACE", new SqlExpression[] { numSpaces }, source);
                            SqlExpression elseCase = sql.Concat(exprS, padding);

                            return sql.SearchedCase(new SqlWhen[] { new SqlWhen(dontChange, exprS) }, elseCase, source);
                        }
                        else if (mc.Arguments.Count == 2) {
                            // s.PadRight(i,c) -->
                            // CASE WHEN CLRLENGTH(@s) >= @i THEN @s
                            //      ELSE @s + REPLICATE(@c, @i - CLRLENGTH(@s))
                            // END
                            SqlExpression exprS = mc.Object;
                            SqlExpression exprI = mc.Arguments[0];
                            SqlExpression exprC = mc.Arguments[1];
                            SqlExpression dontChange = sql.Binary(SqlNodeType.GE, sql.CLRLENGTH(exprS), exprI);
                            SqlExpression len2 = sql.CLRLENGTH(exprS);
                            SqlExpression numSpaces = sql.Subtract(exprI, len2);
                            SqlExpression padding = sql.FunctionCall(typeof(string), "REPLICATE", new SqlExpression[] { exprC, numSpaces }, source);
                            SqlExpression elseCase = sql.Concat(exprS, padding);

                            return sql.SearchedCase(new SqlWhen[] { new SqlWhen(dontChange, exprS) }, elseCase, source);
                        }
                        break;

                    case "Remove":
                        if (mc.Arguments.Count == 1) {
                            return sql.FunctionCall(
                                typeof(string), "STUFF",
                                new SqlExpression[] {
                                    mc.Object,
                                    sql.Add(mc.Arguments[0], 1),
                                    sql.CLRLENGTH(mc.Object),
                                    sql.ValueFromObject("", false, source)
                                },
                                source);
                        }
                        else if (mc.Arguments.Count == 2) {
                            return sql.FunctionCall(
                                typeof(string), "STUFF",
                                new SqlExpression[] {
                                    mc.Object,
                                    sql.Add(mc.Arguments[0], 1),
                                    mc.Arguments[1],
                                    sql.ValueFromObject("", false, source)
                                },
                                source);
                        }
                        break;
                    case "Replace":
                        if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null) {
                            throw Error.ArgumentNull("old");
                        }
                        if (mc.Arguments[1] is SqlValue && ((SqlValue)mc.Arguments[1]).Value == null) {
                            throw Error.ArgumentNull("new");
                        }
                        return sql.FunctionCall(
                            typeof(string), "REPLACE",
                            new SqlExpression[] {
                                mc.Object,
                                mc.Arguments[0],
                                mc.Arguments[1]
                            },
                            source);
                    case "Substring":
                        if (mc.Arguments.Count == 1) {
                            return sql.FunctionCall(
                                typeof(string), "SUBSTRING",
                                new SqlExpression[] {
                                    mc.Object,
                                    sql.Add(mc.Arguments[0], 1),
                                    sql.CLRLENGTH(mc.Object)
                                    },
                                source);
                        }
                        else if (mc.Arguments.Count == 2) {
                            return sql.FunctionCall(
                                typeof(string), "SUBSTRING",
                                new SqlExpression[] {
                                    mc.Object,
                                    sql.Add(mc.Arguments[0], 1),
                                    mc.Arguments[1]
                                    },
                                source);
                        }
                        break;
                    case "Trim":
                        if (mc.Arguments.Count == 0) {
                            return sql.FunctionCall(
                                typeof(string), "LTRIM",
                                new SqlExpression[] {
                                    sql.FunctionCall(typeof(string), "RTRIM", new SqlExpression[] { mc.Object }, source)
                                    },
                                source);
                        }
                        break;
                    case "ToLower":
                        if (mc.Arguments.Count == 0) {
                            return sql.FunctionCall(typeof(string), "LOWER", new SqlExpression[] { mc.Object }, source);
                        }
                        break;
                    case "ToUpper":
                        if (mc.Arguments.Count == 0) {
                            return sql.FunctionCall(typeof(string), "UPPER", new SqlExpression[] { mc.Object }, source);
                        }
                        break;
                    case "get_Chars":
                        // s[i] --> SUBSTRING(@s, @i+1, 1)
                        if (mc.Arguments.Count == 1) {
                            return sql.FunctionCall(typeof(char), "SUBSTRING", new SqlExpression[]
                                {mc.Object, 
                                 sql.Add( mc.Arguments[0], 1),
                                 sql.ValueFromObject(1, false, source)
                                }, source);
                        }
                        break;
                    case "CompareTo":
                        if (mc.Arguments.Count == 1) {
                            if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null) {
                                throw Error.ArgumentNull("value");
                            }
                            return CreateComparison(mc.Object, mc.Arguments[0], source);
                        }
                        break;
                }
                throw GetMethodSupportException(mc);
            }

            private SqlExpression TranslateMathMethod(SqlMethodCall mc) {
                Expression source = mc.SourceExpression;
                switch (mc.Method.Name) {
                    case "Abs":
                        if (mc.Arguments.Count == 1) {
                            return sql.FunctionCall(mc.Arguments[0].ClrType, "ABS", new SqlExpression[] { mc.Arguments[0] }, source);
                        }
                        break;
                    case "Acos":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "ACOS", mc.Arguments, source);
                        }
                        break;
                    case "Asin":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "ASIN", mc.Arguments, source);
                        }
                        break;
                    case "Atan":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "ATAN", mc.Arguments, source);
                        }
                        break;
                    case "Atan2":
                        if (mc.Arguments.Count == 2) {
                            return this.CreateFunctionCallStatic2(typeof(double), "ATN2", mc.Arguments, source);
                        }
                        break;
                    case "BigMul":
                        if (mc.Arguments.Count == 2) {
                            return sql.Multiply(sql.ConvertToBigint(mc.Arguments[0]), sql.ConvertToBigint(mc.Arguments[1]));
                        }
                        break;
                    case "Ceiling":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(mc.Arguments[0].ClrType, "CEILING", mc.Arguments, source);
                        }
                        break;
                    case "Cos":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "COS", mc.Arguments, source);
                        }
                        break;
                    case "Cosh":
                        if (mc.Arguments.Count == 1) {
                            SqlExpression x = mc.Arguments[0];
                            SqlExpression expX = sql.FunctionCall(typeof(double), "EXP", new SqlExpression[] { x }, source);
                            SqlExpression minusX = sql.Unary(SqlNodeType.Negate, x, source);
                            SqlExpression expMinusX = sql.FunctionCall(typeof(double), "EXP", new SqlExpression[] { minusX }, source);
                            return sql.Divide(sql.Add(expX, expMinusX), 2);
                        }
                        break;
                    // DivRem has out parameter
                    case "Exp":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "EXP", mc.Arguments, source);
                        }
                        break;
                    case "Floor":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(mc.Arguments[0].ClrType, "FLOOR", mc.Arguments, source);
                        }
                        break;
                    // Math.IEEERemainder - difficult to implement correctly since SQL rounds differently
                    case "Log":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "LOG", mc.Arguments, source);
                        }
                        else if (mc.Arguments.Count == 2) {
                            // Math.Log(x,y) --> LOG(@x) / LOG(@y)
                            SqlExpression log1 = sql.FunctionCall(typeof(double), "LOG", new SqlExpression[] { mc.Arguments[0] }, source);
                            SqlExpression log2 = sql.FunctionCall(typeof(double), "LOG", new SqlExpression[] { mc.Arguments[1] }, source);
                            return sql.Divide(log1, log2);
                        }
                        break;
                    case "Log10":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "LOG10", mc.Arguments, source);
                        }
                        break;
                    case "Max":
                        if (mc.Arguments.Count == 2) {
                            // Max(a,b) --> CASE WHEN @a<@b THEN @b ELSE @a
                            SqlExpression a = mc.Arguments[0];
                            SqlExpression b = mc.Arguments[1];
                            SqlExpression aLower = sql.Binary(SqlNodeType.LT, a, b);
                            return new SqlSearchedCase(mc.Method.ReturnType, new SqlWhen[] { new SqlWhen(aLower, b) }, a, source);
                        }
                        break;
                    case "Min":
                        if (mc.Arguments.Count == 2) {
                            // Min(a,b) --> CASE WHEN @a<@b THEN @a ELSE @b
                            SqlExpression a = mc.Arguments[0];
                            SqlExpression b = mc.Arguments[1];
                            SqlExpression aLower = sql.Binary(SqlNodeType.LT, a, b);
                            return sql.SearchedCase(new SqlWhen[] { new SqlWhen(aLower, a) }, b, source);
                        }
                        break;
                    case "Pow":
                        if (mc.Arguments.Count == 2) {
                            return this.CreateFunctionCallStatic2(mc.ClrType, "POWER", mc.Arguments, source);
                        }
                        break;
                    case "Round":
                        int nParams = mc.Arguments.Count;
                        if ((mc.Arguments[nParams - 1].ClrType != typeof(MidpointRounding))) {
                            throw Error.MathRoundNotSupported();
                        }
                        else {
                            SqlExpression x = mc.Arguments[0];
                            SqlExpression i = null;
                            if (nParams == 2) {
                                i = sql.ValueFromObject(0, false, source);
                            }
                            else {
                                i = mc.Arguments[1];
                            }
                            SqlExpression roundingMethod = mc.Arguments[nParams - 1];
                            if (roundingMethod.NodeType != SqlNodeType.Value) {
                                throw Error.NonConstantExpressionsNotSupportedForRounding();
                            }
                            if ((MidpointRounding)this.Eval(roundingMethod) == MidpointRounding.AwayFromZero) {
                                // round(x) --> round(@x,0)
                                return sql.FunctionCall(x.ClrType, "round", new SqlExpression[] { x, i }, source);
                            }
                            else {
                                // CASE WHEN 2*@x = ROUND(2*@x, @i) AND @x <> ROUND(@x, @i)
                                //      THEN 2 * ROUND(@x/2, @i)
                                //      ELSE ROUND(@x, @i)
                                // END
                                Type type = x.ClrType;
                                SqlExpression roundX = sql.FunctionCall(type, "round", new SqlExpression[] { x, i }, source);
                                SqlExpression twiceX = sql.Multiply(x, 2);
                                SqlExpression round2X = sql.FunctionCall(type, "round", new SqlExpression[] { twiceX, i }, source);
                                SqlExpression condition = sql.AndAccumulate(sql.Binary(SqlNodeType.EQ, twiceX, round2X), sql.Binary(SqlNodeType.NE, x, roundX));
                                SqlExpression specialCase = sql.Multiply(sql.FunctionCall(type, "round", new SqlExpression[] { sql.Divide(x, 2), i }, source), 2);
                                return sql.SearchedCase(new SqlWhen[] { new SqlWhen(condition, specialCase) }, roundX, source);
                            }
                        }
                    case "Sign":
                        if (mc.Arguments.Count == 1) {
                            return sql.FunctionCall(typeof(int), "SIGN", new SqlExpression[] { mc.Arguments[0] }, source);
                        }
                        break;
                    case "Sin":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "SIN", mc.Arguments, source);
                        }
                        break;
                    case "Sinh":
                        if (mc.Arguments.Count == 1) {
                            SqlExpression x = mc.Arguments[0];
                            SqlExpression exp = sql.FunctionCall(typeof(double), "EXP", new SqlExpression[] { x }, source);
                            SqlExpression minusX = sql.Unary(SqlNodeType.Negate, x, source);
                            SqlExpression expMinus = sql.FunctionCall(typeof(double), "EXP", new SqlExpression[] { minusX }, source);
                            return sql.Divide(sql.Subtract(exp, expMinus), 2);
                        }
                        break;
                    case "Sqrt":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "SQRT", mc.Arguments, source);
                        }
                        break;
                    case "Tan":
                        if (mc.Arguments.Count == 1) {
                            return this.CreateFunctionCallStatic1(typeof(double), "TAN", mc.Arguments, source);
                        }
                        break;
                    case "Tanh":
                        if (mc.Arguments.Count == 1) {
                            SqlExpression x = mc.Arguments[0];
                            SqlExpression expX = sql.FunctionCall(typeof(double), "EXP", new SqlExpression[] { x }, source);
                            SqlExpression minusX = sql.Unary(SqlNodeType.Negate, x, source);
                            SqlExpression expMinusX = sql.FunctionCall(typeof(double), "EXP", new SqlExpression[] { minusX }, source);
                            return sql.Divide(sql.Subtract(expX, expMinusX), sql.Add(expX, expMinusX));
                        }
                        break;
                    case "Truncate":
                        if (mc.Arguments.Count == 1) {
                            // Truncate(x) --> ROUND (x, 0, 1)
                            SqlExpression x = mc.Arguments[0];
                            return sql.FunctionCall(mc.Method.ReturnType, "ROUND", new SqlExpression[] { x, sql.ValueFromObject(0, false, source), sql.ValueFromObject(1, false, source) }, source);
                        }
                        break;
                }
                throw GetMethodSupportException(mc);
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "[....]: These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal override SqlNode VisitMember(SqlMember m) {
                SqlExpression exp = this.VisitExpression(m.Expression);
                MemberInfo member = m.Member;
                Expression source = m.SourceExpression;

                Type baseClrTypeOfExpr = TypeSystem.GetNonNullableType(exp.ClrType);
                if (baseClrTypeOfExpr == typeof(string) && member.Name == "Length") {
                    // This gives a different result than .Net would if the string ends in spaces.
                    // We decided not to fix this up (e.g. LEN(@s+'#') - 1) since it would incur a performance hit and 
                    // people may actually expect that it translates to the SQL LEN function.
                    return sql.LEN(exp);
                }
                else if (baseClrTypeOfExpr == typeof(Binary) && member.Name == "Length") {
                    return sql.DATALENGTH(exp);
                }
                else if (baseClrTypeOfExpr == typeof(DateTime) || baseClrTypeOfExpr == typeof(DateTimeOffset)) {
                    string datePart = GetDatePart(member.Name);
                    if (datePart != null) {
                        return sql.DATEPART(datePart, exp);
                    }
                    else if (member.Name == "Date") {
                        if (this.providerMode == SqlProvider.ProviderMode.Sql2008) {
                            SqlExpression date = new SqlVariable(typeof(void), null, "DATE", source);
                            return sql.FunctionCall(typeof(DateTime), "CONVERT", new SqlExpression[2] { date, exp }, source);
                        }
                        // date --> dateadd(hh, -(datepart(hh, @date)), 
                        //          dateadd(mi, -(datepart(mi, @date)), 
                        //          dateadd(ss, -(datepart(ss, @date)), 
                        //          dateadd(ms, -(datepart(ms, @date)), 
                        //          @date))))

                        SqlExpression ms = sql.DATEPART("MILLISECOND", exp);
                        SqlExpression ss = sql.DATEPART("SECOND", exp);
                        SqlExpression mi = sql.DATEPART("MINUTE", exp);
                        SqlExpression hh = sql.DATEPART("HOUR", exp);

                        SqlExpression result = exp;

                        result = sql.DATEADD("MILLISECOND", sql.Unary(SqlNodeType.Negate, ms), result);
                        result = sql.DATEADD("SECOND", sql.Unary(SqlNodeType.Negate, ss), result);
                        result = sql.DATEADD("MINUTE", sql.Unary(SqlNodeType.Negate, mi), result);
                        result = sql.DATEADD("HOUR", sql.Unary(SqlNodeType.Negate, hh), result);

                        return result;
                    }
                    else if (member.Name == "DateTime") {
                        Debug.Assert(baseClrTypeOfExpr == typeof(DateTimeOffset), "'DateTime' property supported only for instances of DateTimeOffset.");
                        SqlExpression datetime = new SqlVariable(typeof(void), null, "DATETIME", source);
                        return sql.FunctionCall(typeof(DateTime), "CONVERT", new SqlExpression[2] { datetime, exp }, source);
                    }
                    else if (member.Name == "TimeOfDay") {
                        SqlExpression hours = sql.DATEPART("HOUR", exp);
                        SqlExpression minutes = sql.DATEPART("MINUTE", exp);
                        SqlExpression seconds = sql.DATEPART("SECOND", exp);
                        SqlExpression milliseconds = sql.DATEPART("MILLISECOND", exp);

                        SqlExpression ticksFromHour = sql.Multiply(sql.ConvertToBigint(hours), TimeSpan.TicksPerHour);
                        SqlExpression ticksFromMinutes = sql.Multiply(sql.ConvertToBigint(minutes), TimeSpan.TicksPerMinute);
                        SqlExpression ticksFromSeconds = sql.Multiply(sql.ConvertToBigint(seconds), TimeSpan.TicksPerSecond);
                        SqlExpression ticksFromMs = sql.Multiply(sql.ConvertToBigint(milliseconds), TimeSpan.TicksPerMillisecond);
                        return sql.ConvertTo(typeof(TimeSpan), sql.Add(ticksFromHour, ticksFromMinutes, ticksFromSeconds, ticksFromMs));
                    }
                    else if (member.Name == "DayOfWeek") {
                        //(DATEPART(dw,@date) + @@Datefirst + 6) % 7 to make it independent from SQL settings
                        SqlExpression sqlDay = sql.DATEPART("dw", exp);

                        // 
                        // .DayOfWeek returns a System.DayOfWeek, so ConvertTo that enum.
                        return sql.ConvertTo(typeof(DayOfWeek),
                                sql.Mod(
                                  sql.Add(sqlDay,
                                     sql.Add(new SqlVariable(typeof(int), sql.Default(typeof(int)), "@@DATEFIRST", source), 6)
                                  )
                                , 7));
                    }
                }
                else if (baseClrTypeOfExpr == typeof(System.TimeSpan)) {
                    switch (member.Name) {
                        case "Ticks":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.Divide(
                                            sql.ConvertToBigint(
                                                sql.Add(
                                                    this.sql.Multiply(sql.ConvertToBigint(sql.DATEPART("HOUR", exp)), 3600000000000),
                                                    this.sql.Multiply(sql.ConvertToBigint(sql.DATEPART("MINUTE", exp)), 60000000000),
                                                    this.sql.Multiply(sql.ConvertToBigint(sql.DATEPART("SECOND", exp)), 1000000000),
                                                    sql.DATEPART("NANOSECOND", exp))
                                                ),
                                            100
                                    );
                            }
                            return sql.ConvertToBigint(exp);
                        case "TotalMilliseconds":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.Add(
                                            this.sql.Multiply(sql.DATEPART("HOUR", exp), 3600000),
                                            this.sql.Multiply(sql.DATEPART("MINUTE", exp), 60000),
                                            this.sql.Multiply(sql.DATEPART("SECOND", exp), 1000),
                                            this.sql.Divide(sql.ConvertToDouble(sql.ConvertToBigint(sql.DATEPART("NANOSECOND", exp))), 1000000)
                                        );
                            }
                            return sql.Divide(sql.ConvertToDouble(exp), TimeSpan.TicksPerMillisecond);
                        case "TotalSeconds":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.Add(
                                            this.sql.Multiply(sql.DATEPART("HOUR", exp), 3600),
                                            this.sql.Multiply(sql.DATEPART("MINUTE", exp), 60),
                                            this.sql.DATEPART("SECOND", exp),
                                            this.sql.Divide(sql.ConvertToDouble(sql.ConvertToBigint(sql.DATEPART("NANOSECOND", exp))), 1000000000)
                                        );
                            }
                            return sql.Divide(sql.ConvertToDouble(exp), TimeSpan.TicksPerSecond);
                        case "TotalMinutes":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.Add(
                                            this.sql.Multiply(sql.DATEPART("HOUR", exp), 60),
                                            this.sql.DATEPART("MINUTE", exp),
                                            this.sql.Divide(sql.ConvertToDouble(sql.DATEPART("SECOND", exp)), 60),
                                            this.sql.Divide(sql.ConvertToDouble(sql.ConvertToBigint(sql.DATEPART("NANOSECOND", exp))), 60000000000)
                                        );
                            }
                            return sql.Divide(sql.ConvertToDouble(exp), TimeSpan.TicksPerMinute);
                        case "TotalHours":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.Add(
                                            this.sql.DATEPART("HOUR", exp),
                                            this.sql.Divide(sql.ConvertToDouble(sql.DATEPART("MINUTE", exp)), 60),
                                            this.sql.Divide(sql.ConvertToDouble(sql.DATEPART("SECOND", exp)), 3600),
                                            this.sql.Divide(sql.ConvertToDouble(sql.ConvertToBigint(sql.DATEPART("NANOSECOND", exp))), 3600000000000)
                                        );
                            }
                            return sql.Divide(sql.ConvertToDouble(exp), TimeSpan.TicksPerHour);
                        case "TotalDays":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.Divide(
                                            this.sql.Add(
                                                this.sql.DATEPART("HOUR", exp),
                                                this.sql.Divide(sql.ConvertToDouble(sql.DATEPART("MINUTE", exp)), 60),
                                                this.sql.Divide(sql.ConvertToDouble(sql.DATEPART("SECOND", exp)), 3600),
                                                this.sql.Divide(sql.ConvertToDouble(sql.ConvertToBigint(sql.DATEPART("NANOSECOND", exp))), 3600000000000)),
                                            24
                                        );
                            }
                            return sql.Divide(sql.ConvertToDouble(exp), TimeSpan.TicksPerDay);
                        case "Milliseconds":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.DATEPART("MILLISECOND", exp);
                            }
                            return sql.ConvertToInt(sql.Mod(sql.ConvertToBigint(sql.Divide(exp, TimeSpan.TicksPerMillisecond)), 1000));
                        case "Seconds":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.DATEPART("SECOND", exp);
                            }
                            return sql.ConvertToInt(sql.Mod(sql.ConvertToBigint(sql.Divide(exp, TimeSpan.TicksPerSecond)), 60));
                        case "Minutes":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.DATEPART("MINUTE", exp);
                            }
                            return sql.ConvertToInt(sql.Mod(sql.ConvertToBigint(sql.Divide(exp, TimeSpan.TicksPerMinute)), 60));
                        case "Hours":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.DATEPART("HOUR", exp);
                            }
                            return sql.ConvertToInt(sql.Mod(sql.ConvertToBigint(sql.Divide(exp, TimeSpan.TicksPerHour)), 24));
                        case "Days":
                            if (SqlFactory.IsSqlTimeType(exp)) {
                                return this.sql.ValueFromObject(0, false, exp.SourceExpression);
                            }
                            return sql.ConvertToInt(sql.Divide(exp, TimeSpan.TicksPerDay));
                        default:
                            throw Error.MemberCannotBeTranslated(member.DeclaringType, member.Name);
                    }
                }
                throw Error.MemberCannotBeTranslated(member.DeclaringType, member.Name);
            }

            // date + timespan --> DATEADD( day, @timespan/864000000000, DATEADD(ms,(@timespan/1000) % 86400000 , @date))
            private SqlExpression CreateDateTimeFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source) {
                return CreateDateTimeFromDateAndTicks(sqlDate, sqlTicks, source, false);
            }

            private SqlExpression CreateDateTimeFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source, bool asNullable) {
                SqlExpression daysAdded = sql.DATEADD("day", sql.Divide(sqlTicks, TimeSpan.TicksPerDay), sqlDate, source, asNullable);
                return sql.DATEADD("ms", sql.Mod(sql.Divide(sqlTicks, TimeSpan.TicksPerMillisecond), 86400000), daysAdded, source, asNullable);
            }

            // date + ms --> DATEADD( day, @ms/86400000, DATEADD(ms, @ms % 86400000 , @date))
            private SqlExpression CreateDateTimeFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source) {
                return CreateDateTimeFromDateAndMs(sqlDate, ms, source, false);
            }

            private SqlExpression CreateDateTimeFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source, bool asNullable) {
                SqlExpression msBigint = sql.ConvertToBigint(ms);
                SqlExpression daysAdded = sql.DATEADD("day", sql.Divide(msBigint, 86400000), sqlDate, source, asNullable);
                return sql.DATEADD("ms", sql.Mod(msBigint, 86400000), daysAdded, source, asNullable);
            }

            // date + timespan --> DATEADD( day, @timespan/864000000000, DATEADD(ms,(@timespan/1000) % 86400000 , @date))
            private SqlExpression CreateDateTimeOffsetFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source) {
                return CreateDateTimeOffsetFromDateAndTicks(sqlDate, sqlTicks, source, false);
            }

            private SqlExpression CreateDateTimeOffsetFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source, bool asNullable) {
                SqlExpression daysAdded = sql.DATETIMEOFFSETADD("day", sql.Divide(sqlTicks, TimeSpan.TicksPerDay), sqlDate, source, asNullable);
                return sql.DATETIMEOFFSETADD("ms", sql.Mod(sql.Divide(sqlTicks, TimeSpan.TicksPerMillisecond), 86400000), daysAdded, source, asNullable);
            }

            // date + ms --> DATEADD( day, @ms/86400000, DATEADD(ms, @ms % 86400000 , @date))
            private SqlExpression CreateDateTimeOffsetFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source) {
                return CreateDateTimeOffsetFromDateAndMs(sqlDate, ms, source, false);
            }

            private SqlExpression CreateDateTimeOffsetFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source, bool asNullable) {
                SqlExpression msBigint = sql.ConvertToBigint(ms);
                SqlExpression daysAdded = sql.DATETIMEOFFSETADD("day", sql.Divide(msBigint, 86400000), sqlDate, source, asNullable);
                return sql.DATETIMEOFFSETADD("ms", sql.Mod(msBigint, 86400000), daysAdded, source, asNullable);
            }

            private SqlExpression TranslateVbConversionMethod(SqlMethodCall mc) {
                Expression source = mc.SourceExpression;
                if (mc.Arguments.Count == 1) {
                    SqlExpression expr = mc.Arguments[0];
                    Type targetType = null;
                    switch (mc.Method.Name) {
                        case "ToBoolean":
                            targetType = typeof(bool);
                            break;
                        case "ToSByte":
                            targetType = typeof(sbyte);
                            break;
                        case "ToByte":
                            targetType = typeof(byte);
                            break;
                        case "ToChar":
                            targetType = typeof(char);
                            break;
                        case "ToCharArrayRankOne":
                            targetType = typeof(char[]);
                            break;
                        case "ToDate":
                            targetType = typeof(DateTime);
                            break;
                        case "ToDecimal":
                            targetType = typeof(decimal);
                            break;
                        case "ToDouble":
                            targetType = typeof(double);
                            break;
                        case "ToInteger":
                            targetType = typeof(Int32);
                            break;
                        case "ToUInteger":
                            targetType = typeof(UInt32);
                            break;
                        case "ToLong":
                            targetType = typeof(Int64);
                            break;
                        case "ToULong":
                            targetType = typeof(UInt64);
                            break;
                        case "ToShort":
                            targetType = typeof(Int16);
                            break;
                        case "ToUShort":
                            targetType = typeof(UInt16);
                            break;
                        case "ToSingle":
                            targetType = typeof(float);
                            break;
                        case "ToString":
                            targetType = typeof(string);
                            break;
                    }
                    if (targetType != null) {
                        if ((targetType == typeof(int) || targetType == typeof(Single)) && expr.ClrType == typeof(bool)) {
                            List<SqlExpression> matchesList = new List<SqlExpression>();
                            List<SqlExpression> valuesList = new List<SqlExpression>();

                            matchesList.Add(sql.ValueFromObject(true, false, source));
                            valuesList.Add(sql.ValueFromObject(-1, false, source));
                            matchesList.Add(sql.ValueFromObject(false, false, source));
                            valuesList.Add(sql.ValueFromObject(0, false, source));

                            return sql.Case(targetType, expr, matchesList, valuesList, source);
                        }
                        else if (mc.ClrType != mc.Arguments[0].ClrType) {
                            // do the conversions that would be done for a cast "(<targetType>) expression"
                            return sql.ConvertTo(targetType, expr);
                        }
                        else {
                            return mc.Arguments[0];
                        }
                    }
                }
                throw GetMethodSupportException(mc);
            }

            private SqlExpression TranslateVbCompareString(SqlMethodCall mc) {
                if (mc.Arguments.Count >= 2) {
                    return CreateComparison(mc.Arguments[0], mc.Arguments[1], mc.SourceExpression);
                }
                throw GetMethodSupportException(mc);
            }

            private SqlExpression TranslateVbLikeString(SqlMethodCall mc) {
                // these should be true per the method signature
                Debug.Assert(mc.Arguments.Count == 3);
                Debug.Assert(mc.Arguments[0].ClrType == typeof(string));
                Debug.Assert(mc.Arguments[1].ClrType == typeof(string));
                bool needsEscape = true;

                Expression source = mc.SourceExpression;
                SqlExpression pattern = mc.Arguments[1];
                if (pattern.NodeType == SqlNodeType.Value) {
                    string unescapedText = (string)((SqlValue)pattern).Value;
                    string patternText = SqlHelpers.TranslateVBLikePattern(unescapedText, '~');
                    pattern = sql.ValueFromObject(patternText, typeof(string), true, source);
                    needsEscape = unescapedText != patternText;
                }
                else if (pattern.NodeType == SqlNodeType.ClientParameter) {
                    SqlClientParameter cp = (SqlClientParameter)pattern;
                    pattern = new SqlClientParameter(
                        cp.ClrType, cp.SqlType,
                        Expression.Lambda(
                            Expression.Call(typeof(SqlHelpers), "TranslateVBLikePattern", Type.EmptyTypes, cp.Accessor.Body, Expression.Constant('~')),
                            cp.Accessor.Parameters[0]
                            ),
                        cp.SourceExpression
                        );
                }
                else {
                    throw Error.NonConstantExpressionsNotSupportedFor("LIKE");
                }
                SqlExpression escape = needsEscape ? sql.ValueFromObject("~", false, mc.SourceExpression) : null;
                return sql.Like(mc.Arguments[0], pattern, escape, source);
            }
        }
    }
}
