using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Data.Linq;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics;

    /// <summary>
    /// Factory class produces SqlNodes. Smarts about type system mappings should go
    /// here and not in the individual SqlNodes.
    /// </summary>
    internal class SqlFactory {
        private TypeSystemProvider typeProvider;
        private MetaModel model;

        internal TypeSystemProvider TypeProvider {
            get { return typeProvider; }
        }

        internal SqlFactory(TypeSystemProvider typeProvider, MetaModel model) {
            this.typeProvider = typeProvider;
            this.model = model;
        }

        #region Expression Operators

        internal SqlExpression ConvertTo(Type clrType, ProviderType sqlType, SqlExpression expr) {
            return UnaryConvert(clrType, sqlType, expr, expr.SourceExpression);
        }

        internal SqlExpression ConvertTo(Type clrType, SqlExpression expr) {
            //
            // In SQL Server 2008, the new TIME data type cannot be converted to BIGINT, or FLOAT,
            // or a bunch of other SQL types.
            //
            if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
                clrType = clrType.GetGenericArguments()[0];

            bool isClrTimeSpanType = clrType == typeof(TimeSpan);

            if (IsSqlTimeType(expr))
            {
                if (isClrTimeSpanType) {
                    // no conversion necessary
                    return expr;
                } else {
                    expr = ConvertToDateTime(expr);
                }
            }

            return UnaryConvert(clrType, typeProvider.From(clrType), expr, expr.SourceExpression);
        }

        internal SqlExpression ConvertToBigint(SqlExpression expr) {
            return ConvertTo(typeof(long), expr);
        }

        internal SqlExpression ConvertToInt(SqlExpression expr) {
            return ConvertTo(typeof(int), expr);
        }

        internal SqlExpression ConvertToDouble(SqlExpression expr) {
            return ConvertTo(typeof(double), expr);
        }

        // If the argument expression has SqlDbType Time, inject a conversion to Double, else return
        // the expression unchanged.
        //
        internal SqlExpression ConvertTimeToDouble(SqlExpression exp) {
            return IsSqlTimeType(exp) ? ConvertToDouble(exp) : exp;
        }

        internal SqlExpression ConvertToBool(SqlExpression expr) {
            return ConvertTo(typeof(bool), expr);
        }

        internal SqlExpression ConvertToDateTime(SqlExpression expr) {
            return UnaryConvert(typeof(DateTime), typeProvider.From(typeof(DateTime)), expr, expr.SourceExpression);
        }

        internal SqlExpression AndAccumulate(SqlExpression left, SqlExpression right) {
            if (left == null) {
                return right;
            }
            else if (right == null) {
                return left;
            }
            else {
                return Binary(SqlNodeType.And, left, right);
            }
        }

        internal SqlExpression OrAccumulate(SqlExpression left, SqlExpression right) {
            if (left == null) {
                return right;
            }
            else if (right == null) {
                return left;
            }
            else {
                return Binary(SqlNodeType.Or, left, right);
            }
        }

        internal SqlExpression Concat(params SqlExpression[] expressions) {
            SqlExpression result = expressions[expressions.Length - 1];
            for (int i = expressions.Length - 2; i >= 0; i--) {
                result = Binary(SqlNodeType.Concat, expressions[i], result);
            }
            return result;
        }

        internal SqlExpression Add(params SqlExpression[] expressions) {
            SqlExpression sum = expressions[expressions.Length - 1];
            for (int i = expressions.Length - 2; i >= 0; i--) {
                sum = Binary(SqlNodeType.Add, expressions[i], sum);
            }
            return sum;
        }

        internal SqlExpression Subtract(SqlExpression first, SqlExpression second) {
            return Binary(SqlNodeType.Sub, first, second);
        }

        internal SqlExpression Multiply(params SqlExpression[] expressions) {
            SqlExpression result = expressions[expressions.Length - 1];
            for (int i = expressions.Length - 2; i >= 0; i--) {
                result = Binary(SqlNodeType.Mul, expressions[i], result);
            }
            return result;
        }

        internal SqlExpression Divide(SqlExpression first, SqlExpression second) {
            return Binary(SqlNodeType.Div, first, second);
        }

        internal SqlExpression Add(SqlExpression expr, int second) {
            return Binary(SqlNodeType.Add, expr, ValueFromObject(second, false, expr.SourceExpression));
        }

        internal SqlExpression Subtract(SqlExpression expr, int second) {
            return Binary(SqlNodeType.Sub, expr, ValueFromObject(second, false, expr.SourceExpression));
        }

        internal SqlExpression Multiply(SqlExpression expr, long second) {
            return Binary(SqlNodeType.Mul, expr, ValueFromObject(second, false, expr.SourceExpression));
        }

        internal SqlExpression Divide(SqlExpression expr, long second) {
            return Binary(SqlNodeType.Div, expr, ValueFromObject(second, false, expr.SourceExpression));
        }

        internal SqlExpression Mod(SqlExpression expr, long second) {
            return Binary(SqlNodeType.Mod, expr, ValueFromObject(second, false, expr.SourceExpression));
        }

        /// <summary>
        /// Non-internal string length.  This should only be used when translating an explicit call by the
        /// user to String.Length.
        /// </summary>
        internal SqlExpression LEN(SqlExpression expr) {
            return FunctionCall(typeof(int), "LEN", new SqlExpression[] { expr }, expr.SourceExpression);
        }

        /// <summary>
        /// This represents the SQL DATALENGTH function, which is the raw number of bytes in the argument.  In the
        /// case of string types it will count trailing spaces, but doesn't understand unicode.
        /// </summary>
        internal SqlExpression DATALENGTH(SqlExpression expr) {
            return FunctionCall(typeof(int), "DATALENGTH", new SqlExpression[] { expr }, expr.SourceExpression);
        }

        /// <summary>
        /// A unary function that uses DATALENGTH, dividing by two if the string is unicode.  This is the internal
        /// form of String.Length that should always be used.
        /// </summary>
        internal SqlExpression CLRLENGTH(SqlExpression expr) {
            return Unary(SqlNodeType.ClrLength, expr);
        }

        internal SqlExpression DATEPART(string partName, SqlExpression expr) {
            return FunctionCall(
                typeof(int),
                "DATEPART",
                new SqlExpression[] { 
                    new SqlVariable(typeof(void), null, partName, expr.SourceExpression), 
                    expr 
                },
                expr.SourceExpression
                );
        }

        internal SqlExpression DATEADD(string partName, SqlExpression value, SqlExpression expr) {
            return DATEADD(partName, value, expr, expr.SourceExpression, false);
        }

        internal SqlExpression DATEADD(string partName, SqlExpression value, SqlExpression expr, Expression sourceExpression, bool asNullable) {
            Type returnType = asNullable ? typeof(DateTime?) : typeof(DateTime);

            return FunctionCall(
                returnType,
                "DATEADD",
                new SqlExpression[] {
                    new SqlVariable(typeof(void), null, partName, sourceExpression),
                    value,
                    expr },
                sourceExpression
                );
        }

        internal SqlExpression DATETIMEOFFSETADD(string partName, SqlExpression value, SqlExpression expr) {
            return DATETIMEOFFSETADD(partName, value, expr, expr.SourceExpression, false);
        }

        internal SqlExpression DATETIMEOFFSETADD(string partName, SqlExpression value, SqlExpression expr, Expression sourceExpression, bool asNullable) {
            Type returnType = asNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);

            return FunctionCall(
                returnType,
                "DATEADD",
                new SqlExpression[] {
                    new SqlVariable(typeof(void), null, partName, sourceExpression),
                    value,
                    expr },
                sourceExpression
                );
        }

        #endregion

        internal SqlExpression AddTimeSpan(SqlExpression dateTime, SqlExpression timeSpan) {
            return AddTimeSpan(dateTime, timeSpan, false);
        }

        internal SqlExpression AddTimeSpan(SqlExpression dateTime, SqlExpression timeSpan, bool asNullable) {
            Debug.Assert(IsSqlHighPrecisionDateTimeType(timeSpan));

            SqlExpression ns = DATEPART("NANOSECOND", timeSpan);
            SqlExpression ms = DATEPART("MILLISECOND", timeSpan);
            SqlExpression ss = DATEPART("SECOND", timeSpan);
            SqlExpression mi = DATEPART("MINUTE", timeSpan);
            SqlExpression hh = DATEPART("HOUR", timeSpan);

            SqlExpression result = dateTime;
            if (IsSqlHighPrecisionDateTimeType(dateTime)) {
                result = DATEADD("NANOSECOND", ns, result, dateTime.SourceExpression, asNullable);
            } else {
                result = DATEADD("MILLISECOND", ms, result, dateTime.SourceExpression, asNullable);
            }
            result = DATEADD("SECOND", ss, result, dateTime.SourceExpression, asNullable);
            result = DATEADD("MINUTE", mi, result, dateTime.SourceExpression, asNullable);
            result = DATEADD("HOUR", hh, result, dateTime.SourceExpression, asNullable);

            if (IsSqlDateTimeOffsetType(dateTime))
                return ConvertTo(typeof(DateTimeOffset), result);

            return result;
        }

        internal static bool IsSqlDateTimeType(SqlExpression exp) {
            SqlDbType sqlDbType = ((SqlTypeSystem.SqlType)(exp.SqlType)).SqlDbType;
            return (sqlDbType == SqlDbType.DateTime || sqlDbType == SqlDbType.SmallDateTime);
        }

        internal static bool IsSqlDateType(SqlExpression exp) {
            return (((SqlTypeSystem.SqlType)(exp.SqlType)).SqlDbType == SqlDbType.Date);
        }

        internal static bool IsSqlTimeType(SqlExpression exp) {
            return (((SqlTypeSystem.SqlType)(exp.SqlType)).SqlDbType == SqlDbType.Time);
        }

        internal static bool IsSqlDateTimeOffsetType(SqlExpression exp) {
            return (((SqlTypeSystem.SqlType)(exp.SqlType)).SqlDbType == SqlDbType.DateTimeOffset);
        }

        internal static bool IsSqlHighPrecisionDateTimeType(SqlExpression exp) {
            SqlDbType sqlDbType = ((SqlTypeSystem.SqlType)(exp.SqlType)).SqlDbType;
            return (sqlDbType == SqlDbType.Time || sqlDbType == SqlDbType.DateTime2 || sqlDbType == SqlDbType.DateTimeOffset);
        }

        internal SqlExpression Value(Type clrType, ProviderType sqlType, object value, bool isClientSpecified, Expression sourceExpression) {
            if (typeof(Type).IsAssignableFrom(clrType) && value != null) {
                MetaType typeOf = this.model.GetMetaType((Type)value);
                return StaticType(typeOf, sourceExpression);
            }
            return new SqlValue(clrType, sqlType, value, isClientSpecified, sourceExpression);
        }

        /// <summary>
        /// Return a node representing typeof(typeOf)
        /// </summary>
        internal SqlExpression StaticType(MetaType typeOf, Expression sourceExpression) {
            if (typeOf==null)
                throw Error.ArgumentNull("typeOf");
            if(typeOf.InheritanceCode==null) {
                // If no inheritance is involved, then there's no discriminator to 
                // make a discriminated type. In this case, just make a literal type.
                return new SqlValue(typeof(Type), this.typeProvider.From(typeof(Type)), typeOf.Type, false, sourceExpression);
            }
            Type type = typeOf.InheritanceCode.GetType();
            SqlValue match = new SqlValue(type, this.typeProvider.From(type), typeOf.InheritanceCode, true, sourceExpression);
            return this.DiscriminatedType(match, typeOf);
        }

        internal SqlExpression DiscriminatedType(SqlExpression discriminator, MetaType targetType) {
            return new SqlDiscriminatedType(typeProvider.From(typeof(Type)), discriminator, targetType, discriminator.SourceExpression);
        }

        internal SqlTable Table(MetaTable table, MetaType rowType, Expression sourceExpression) {
            return new SqlTable(table, rowType, this.typeProvider.GetApplicationType((int)ConverterSpecialTypes.Row), sourceExpression);
        }

        internal SqlUnary Unary(SqlNodeType nodeType, SqlExpression expression) {
            return Unary(nodeType, expression, expression.SourceExpression);
        }

        internal SqlRowNumber RowNumber(List<SqlOrderExpression> orderBy, Expression sourceExpression) {
            return new SqlRowNumber(typeof(long), typeProvider.From(typeof(long)), orderBy, sourceExpression);
        }

        internal SqlUnary Unary(SqlNodeType nodeType, SqlExpression expression, Expression sourceExpression) {
            return Unary(nodeType, expression, null, sourceExpression);
        }

        internal SqlUnary Unary(SqlNodeType nodeType, SqlExpression expression, MethodInfo method, Expression sourceExpression) {
            Type clrType = null;
            ProviderType sqlType = null;

            if (nodeType == SqlNodeType.Count) {
                clrType = typeof(int);
                sqlType = typeProvider.From(typeof(int));
            }
            else if (nodeType == SqlNodeType.LongCount) {
                clrType = typeof(long);
                sqlType = typeProvider.From(typeof(long));
            }
            else if (nodeType == SqlNodeType.ClrLength) {
                clrType = typeof(int);
                sqlType = typeProvider.From(typeof(int));
            }
            else {
                if (nodeType.IsPredicateUnaryOperator()) {
                    // DevDiv 201730 - Do not ignore nullability of bool type
                    clrType = expression.ClrType.Equals(typeof(bool?)) ? typeof(bool?) : typeof(bool);
                }
                else {
                    clrType = expression.ClrType;
                }
                sqlType = typeProvider.PredictTypeForUnary(nodeType, expression.SqlType);
            }

            return new SqlUnary(nodeType, clrType, sqlType, expression, method, sourceExpression);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        internal SqlUnary UnaryConvert(Type targetClrType, ProviderType targetSqlType, SqlExpression expression, Expression sourceExpression) {
            System.Diagnostics.Debug.Assert(!targetSqlType.IsRuntimeOnlyType, "Attempted coversion to a runtime type: from = " + expression.SqlType.ToQueryString() + "; to = " + targetSqlType.ToQueryString() + "; source = " + sourceExpression.ToString());
            return new SqlUnary(SqlNodeType.Convert, targetClrType, targetSqlType, expression, null, sourceExpression);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        internal SqlUnary UnaryValueOf(SqlExpression expression, Expression sourceExpression) {
            Type valueType = TypeSystem.GetNonNullableType(expression.ClrType);
            return new SqlUnary(SqlNodeType.ValueOf, valueType, expression.SqlType, expression, null, sourceExpression);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right) {
            return Binary(nodeType, left, right, null, null);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right, MethodInfo method) {
            return Binary(nodeType, left, right, method, null);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right, Type clrType) {
            return Binary(nodeType, left, right, null, clrType);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right, MethodInfo method, Type clrType) {
            ProviderType sqlType = null;
            if (nodeType.IsPredicateBinaryOperator()) {
                if (clrType == null) {
                    clrType = typeof(bool);
                }
                sqlType = typeProvider.From(clrType);
            }
            else {
                ProviderType resultType = this.typeProvider.PredictTypeForBinary(nodeType, left.SqlType, right.SqlType);
                if (resultType == right.SqlType) {
                    if (clrType == null) {
                        clrType = right.ClrType;
                    }
                    sqlType = right.SqlType;
                }
                else if (resultType == left.SqlType) {
                    if (clrType == null) {
                        clrType = left.ClrType;
                    }
                    sqlType = left.SqlType;
                }
                else {
                    sqlType = resultType;
                    if (clrType == null) {
                        clrType = resultType.GetClosestRuntimeType();
                    }
                }
            }
            return new SqlBinary(nodeType, clrType, sqlType, left, right, method);
        }

        internal SqlBetween Between(SqlExpression expr, SqlExpression start, SqlExpression end, Expression source) {
            return new SqlBetween(typeof(bool), typeProvider.From(typeof(bool)), expr, start, end, source);
        }

        internal SqlIn In(SqlExpression expr, IEnumerable<SqlExpression> values, Expression source) {
            return new SqlIn(typeof(bool), typeProvider.From(typeof(bool)), expr, values, source);
        }

        internal SqlLike Like(SqlExpression expr, SqlExpression pattern, SqlExpression escape, Expression source) {
            SqlLike like = new SqlLike(typeof(bool), typeProvider.From(typeof(bool)), expr, pattern, escape, source);
            return like;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        internal SqlSearchedCase SearchedCase(SqlWhen[] whens, SqlExpression @else, Expression sourceExpression) {
            return new SqlSearchedCase(whens[0].Value.ClrType, whens, @else, sourceExpression);
        }

        /// <summary>
        /// Construct either a SqlClientCase or a SqlSimpleCase depending on whether the individual cases 
        /// are client-aided or not.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        internal SqlExpression Case(Type clrType, SqlExpression discriminator, List<SqlExpression> matches, List<SqlExpression> values, Expression sourceExpression) {
            if (values.Count == 0) {
                throw Error.EmptyCaseNotSupported();
            }
            bool anyClient = false;
            foreach (SqlExpression value in values) {
                anyClient |= value.IsClientAidedExpression();
            }
            if (anyClient) {
                List<SqlClientWhen> whens = new List<SqlClientWhen>();
                for (int i = 0, c = matches.Count; i < c; ++i) {
                    whens.Add(new SqlClientWhen(matches[i], values[i]));
                }
                return new SqlClientCase(clrType, discriminator, whens, sourceExpression);
            }
            else {
                List<SqlWhen> whens = new List<SqlWhen>();
                for (int i = 0, c = matches.Count; i < c; ++i) {
                    whens.Add(new SqlWhen(matches[i], values[i]));
                }
                return new SqlSimpleCase(clrType, discriminator, whens, sourceExpression);
            }
        }

        internal SqlExpression Parameter(object value, Expression source) {
            System.Diagnostics.Debug.Assert(value != null);
            Type type = value.GetType();
            return Value(type, this.typeProvider.From(value), value, true, source);
        }

        internal SqlExpression ValueFromObject(object value, Expression sourceExpression) {
            return ValueFromObject(value, false, sourceExpression);
        }

        internal SqlExpression ValueFromObject(object value, bool isClientSpecified, Expression sourceExpression) {
            if (value == null) {
                System.Diagnostics.Debug.Assert(false);
                throw Error.ArgumentNull("value");
            }
            Type clrType = value.GetType();
            return ValueFromObject(value, clrType, isClientSpecified, sourceExpression);
        }

        // Override allowing the CLR type of the value to be specified explicitly.
        internal SqlExpression ValueFromObject(object value, Type clrType, bool isClientSpecified, Expression sourceExpression) {
            if (clrType == null) {
                throw Error.ArgumentNull("clrType");
            }
            ProviderType sqlType = (value == null) ? this.typeProvider.From(clrType) : this.typeProvider.From(value);
            return Value(clrType, sqlType, value, isClientSpecified, sourceExpression);
        }

        public SqlExpression TypedLiteralNull(Type type, Expression sourceExpression) {
            return ValueFromObject(null, type, false, sourceExpression);
        }

        internal SqlMember Member(SqlExpression expr, MetaDataMember member) {
            return new SqlMember(member.Type, this.Default(member), expr, member.Member);
        }

        internal SqlMember Member(SqlExpression expr, MemberInfo member) {
            Type clrType = TypeSystem.GetMemberType(member);
            MetaType metaType = this.model.GetMetaType(member.DeclaringType);
            MetaDataMember metaDataMember = metaType.GetDataMember(member);
            if (metaType != null && metaDataMember != null) {
                return new SqlMember(clrType, this.Default(metaDataMember), expr, member);
            } else {
                return new SqlMember(clrType, this.Default(clrType), expr, member);
            }
        }

        internal SqlExpression TypeCase(Type clrType, MetaType rowType, SqlExpression discriminator, IEnumerable<SqlTypeCaseWhen> whens, Expression sourceExpression) {
            return new SqlTypeCase(clrType, typeProvider.From(clrType), rowType, discriminator, whens, sourceExpression);
        }

        internal SqlNew New(MetaType type, ConstructorInfo cons, IEnumerable<SqlExpression> args, IEnumerable<MemberInfo> argMembers, IEnumerable<SqlMemberAssign> bindings, Expression sourceExpression) {
            SqlNew tb = new SqlNew(type, typeProvider.From(type.Type), cons, args, argMembers, bindings, sourceExpression);
            return tb;
        }

        internal SqlMethodCall MethodCall(MethodInfo method, SqlExpression obj, SqlExpression[] args, Expression sourceExpression) {
            return new SqlMethodCall(method.ReturnType, this.Default(method.ReturnType), method, obj, args, sourceExpression);
        }

        internal SqlMethodCall MethodCall(Type returnType, MethodInfo method, SqlExpression obj, SqlExpression[] args, Expression sourceExpression) {
            return new SqlMethodCall(returnType, this.Default(returnType), method, obj, args, sourceExpression);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        internal SqlExprSet ExprSet(SqlExpression[] exprs, Expression sourceExpression) {
            return new SqlExprSet(exprs[0].ClrType, exprs, sourceExpression);
        }

        internal SqlSubSelect SubSelect(SqlNodeType nt, SqlSelect select) {
            return this.SubSelect(nt, select, null);
        }
        internal SqlSubSelect SubSelect(SqlNodeType nt, SqlSelect select, Type clrType) {
            ProviderType sqlType = null;
            switch (nt) {
                case SqlNodeType.ScalarSubSelect:
                case SqlNodeType.Element:
                    clrType = select.Selection.ClrType;
                    sqlType = select.Selection.SqlType;
                    break;
                case SqlNodeType.Multiset:
                    if (clrType == null) {
                        clrType = typeof(List<>).MakeGenericType(select.Selection.ClrType);
                    }
                    sqlType = typeProvider.GetApplicationType((int)ConverterSpecialTypes.Table);
                    break;
                case SqlNodeType.Exists:
                    clrType = typeof(bool);
                    sqlType = typeProvider.From(typeof(bool));
                    break;
            }
            return new SqlSubSelect(nt, clrType, sqlType, select);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        internal SqlDoNotVisitExpression DoNotVisitExpression(SqlExpression expr) {
            return new SqlDoNotVisitExpression(expr);
        }

        internal SqlFunctionCall FunctionCall(Type clrType, string name, IEnumerable<SqlExpression> args, Expression source) {
            return new SqlFunctionCall(clrType, Default(clrType), name, args, source);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        internal SqlFunctionCall FunctionCall(Type clrType, ProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source) {
            return new SqlFunctionCall(clrType, sqlType, name, args, source);
        }

        internal SqlTableValuedFunctionCall TableValuedFunctionCall(MetaType rowType, Type clrType, string name, IEnumerable<SqlExpression> args, Expression source) {
            return new SqlTableValuedFunctionCall(rowType, clrType, Default(clrType), name, args, source);
        }

        internal ProviderType Default(Type clrType) {
            return typeProvider.From(clrType);
        }

        internal ProviderType Default(MetaDataMember member) {
            if(member == null) 
                throw Error.ArgumentNull("member");

            if (member.DbType != null) {
                return this.typeProvider.Parse(member.DbType);
            }
            else {
                return this.typeProvider.From(member.Type);
            }
        }

        internal SqlJoin MakeJoin(SqlJoinType joinType, SqlSource location, SqlAlias alias, SqlExpression condition, Expression source) {
            // if the new item is on the right side of some outer join then fixup the projection to reflect that it can possibly be null
            if (joinType == SqlJoinType.LeftOuter) {
                SqlSelect sel = alias.Node as SqlSelect;
                if (sel != null && sel.Selection != null && sel.Selection.NodeType != SqlNodeType.OptionalValue) {
                    // replace selection w/ optional + outer-joined-value
                    sel.Selection = new SqlOptionalValue(
                                        new SqlColumn(
                                            "test",
                                            this.Unary(SqlNodeType.OuterJoinedValue,
                                                this.Value(typeof(int?), this.typeProvider.From(typeof(int)), 1, false, source))
                                            ),
                                        sel.Selection
                                        );
                }
            }
            return new SqlJoin(joinType, location, alias, condition, source);
        }
    }
}
