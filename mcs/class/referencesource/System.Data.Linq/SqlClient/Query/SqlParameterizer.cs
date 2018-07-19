using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    internal enum SqlParameterType {
        Value,
        UserArgument,
        PreviousResult
    }

    internal class SqlParameterInfo {
        SqlParameter parameter;
        object value;
        Delegate accessor;
        internal SqlParameterInfo(SqlParameter parameter, Delegate accessor) {
            this.parameter = parameter;
            this.accessor = accessor;
        }
        internal SqlParameterInfo(SqlParameter parameter, object value) {
            this.parameter = parameter;
            this.value = value;
        }
        internal SqlParameterInfo(SqlParameter parameter) {
            this.parameter = parameter;
        }
        internal SqlParameterType Type {
            get {
                if (this.accessor != null) {
                    return SqlParameterType.UserArgument;
                }
                else if (this.parameter.Name == "@ROWCOUNT") {
                    return SqlParameterType.PreviousResult;
                }
                else {
                    return SqlParameterType.Value;
                }
            }
        }
        internal SqlParameter Parameter {
            get { return this.parameter; }
        }
        internal Delegate Accessor {
            get { return this.accessor; }
        }
        internal object Value {
            get { return this.value; }
        }
    }

    internal class SqlParameterizer {
        TypeSystemProvider typeProvider;
        SqlNodeAnnotations annotations;
        int index;

        internal SqlParameterizer(TypeSystemProvider typeProvider, SqlNodeAnnotations annotations) {
            this.typeProvider = typeProvider;
            this.annotations = annotations;
        }

        internal ReadOnlyCollection<SqlParameterInfo> Parameterize(SqlNode node) {
            return this.ParameterizeInternal(node).AsReadOnly();
        }

        private List<SqlParameterInfo> ParameterizeInternal(SqlNode node) {
            Visitor v = new Visitor(this);
            v.Visit(node);
            return new List<SqlParameterInfo>(v.currentParams);
        }

        internal ReadOnlyCollection<ReadOnlyCollection<SqlParameterInfo>> ParameterizeBlock(SqlBlock block) {
            SqlParameterInfo rowStatus =
                new SqlParameterInfo(
                    new SqlParameter(typeof(int), typeProvider.From(typeof(int)), "@ROWCOUNT", block.SourceExpression)
                    );
            List<ReadOnlyCollection<SqlParameterInfo>> list = new List<ReadOnlyCollection<SqlParameterInfo>>();
            for (int i = 0, n = block.Statements.Count; i < n; i++) {
                SqlNode statement = block.Statements[i];
                List<SqlParameterInfo> parameters = this.ParameterizeInternal(statement);
                if (i > 0) {
                    parameters.Add(rowStatus);
                }
                list.Add(parameters.AsReadOnly());
            }
            return list.AsReadOnly();
        }

        internal virtual string CreateParameterName() {
            return "@p" + this.index++;
        }

        class Visitor : SqlVisitor {
            private SqlParameterizer parameterizer;
            internal Dictionary<object, SqlParameterInfo> map;
            internal List<SqlParameterInfo> currentParams;
            private bool topLevel;
            private ProviderType timeProviderType;  // for special case handling of DateTime parameters

            internal Visitor(SqlParameterizer parameterizer) {
                this.parameterizer = parameterizer;
                this.topLevel = true;
                this.map = new Dictionary<object, SqlParameterInfo>();
                this.currentParams = new List<SqlParameterInfo>();
            }

            private SqlParameter InsertLookup(SqlValue cp) {
                SqlParameterInfo pi = null;
                if (!this.map.TryGetValue(cp, out pi)) {
                    SqlParameter p;
                    if (this.timeProviderType == null) {
                        p = new SqlParameter(cp.ClrType, cp.SqlType, this.parameterizer.CreateParameterName(), cp.SourceExpression);
                        pi = new SqlParameterInfo(p, cp.Value);
                    }
                    else {
                        p = new SqlParameter(cp.ClrType, this.timeProviderType, this.parameterizer.CreateParameterName(), cp.SourceExpression);
                        pi = new SqlParameterInfo(p, ((DateTime)cp.Value).TimeOfDay);
                    }
                    this.map.Add(cp, pi);
                    this.currentParams.Add(pi);
                }
                return pi.Parameter;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                //
                // Special case to allow DateTime CLR type to be passed as a paramater where
                // a SQL type TIME is expected. We do this only for the equality/inequality
                // comparisons.
                //
                switch (bo.NodeType) {
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V: {
                        SqlDbType leftSqlDbType = ((SqlTypeSystem.SqlType)(bo.Left.SqlType)).SqlDbType;
                        SqlDbType rightSqlDbType = ((SqlTypeSystem.SqlType)(bo.Right.SqlType)).SqlDbType;
                        if (leftSqlDbType == rightSqlDbType)
                            break;

                        bool isLeftColRef = bo.Left is SqlColumnRef;
                        bool isRightColRef = bo.Right is SqlColumnRef;
                        if (isLeftColRef == isRightColRef)
                            break;

                        if (isLeftColRef && leftSqlDbType == SqlDbType.Time && bo.Right.ClrType == typeof(DateTime))
                            this.timeProviderType = bo.Left.SqlType;
                        else if (isRightColRef && rightSqlDbType == SqlDbType.Time && bo.Left.ClrType == typeof(DateTime))
                            this.timeProviderType = bo.Left.SqlType;
                        break;
                    }
                }
                base.VisitBinaryOperator(bo);
                return bo;
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                bool saveTop = this.topLevel;
                this.topLevel = false;
                select = this.VisitSelectCore(select);
                this.topLevel = saveTop;
                select.Selection = this.VisitExpression(select.Selection);
                return select;
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq) {
                bool saveTop = this.topLevel;
                this.topLevel = false;
                for (int i = 0, n = suq.Arguments.Count; i < n; i++) {
                    suq.Arguments[i] = this.VisitParameter(suq.Arguments[i]);
                }
                this.topLevel = saveTop;
                suq.Projection = this.VisitExpression(suq.Projection);
                return suq;
            }

            [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification="Unknown reason.")]
            internal SqlExpression VisitParameter(SqlExpression expr) {
                SqlExpression result = this.VisitExpression(expr);
                switch (result.NodeType) {
                    case SqlNodeType.Parameter:
                        return (SqlParameter)result;
                    case SqlNodeType.Value:
                        // force even literal values to become parameters
                        return this.InsertLookup((SqlValue)result);
                    default:
                        System.Diagnostics.Debug.Assert(false);
                        return result;
                }
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc) {
                this.VisitUserQuery(spc);

                for (int i = 0, n = spc.Function.Parameters.Count; i < n; i++) {
                    MetaParameter mp = spc.Function.Parameters[i];
                    SqlParameter arg = spc.Arguments[i] as SqlParameter;
                    if (arg != null) {
                        arg.Direction = this.GetParameterDirection(mp);
                        if (arg.Direction == ParameterDirection.InputOutput ||
                            arg.Direction == ParameterDirection.Output) {
                            // Text, NText and Image parameters cannot be used as output parameters
                            // so we retype them if necessary.
                            RetypeOutParameter(arg);
                        }
                    }
                }

                // add default return value 
                SqlParameter p = new SqlParameter(typeof(int?), this.parameterizer.typeProvider.From(typeof(int)), "@RETURN_VALUE", spc.SourceExpression);
                p.Direction = System.Data.ParameterDirection.Output;
                this.currentParams.Add(new SqlParameterInfo(p));

                return spc;
            }

            private bool RetypeOutParameter(SqlParameter node) {
                if (!node.SqlType.IsLargeType) {
                    return false;
                }
                ProviderType newType = this.parameterizer.typeProvider.GetBestLargeType(node.SqlType);
                if (node.SqlType != newType) {
                    node.SetSqlType(newType);
                    return true;
                }
                // Since we are dealing with a long out parameter that hasn't been
                // retyped, we need to annotate
                this.parameterizer.annotations.Add(
                    node, 
                    new SqlServerCompatibilityAnnotation(
                        SqlClient.Strings.MaxSizeNotSupported(node.SourceExpression), SqlProvider.ProviderMode.Sql2000));
                return false;
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private System.Data.ParameterDirection GetParameterDirection(MetaParameter p) {
                if (p.Parameter.IsRetval) {
                    return System.Data.ParameterDirection.ReturnValue;
                }
                else if (p.Parameter.IsOut) {
                    return System.Data.ParameterDirection.Output;
                }
                else if (p.Parameter.ParameterType.IsByRef) {
                    return System.Data.ParameterDirection.InputOutput;
                }
                else {
                    return System.Data.ParameterDirection.Input;
                }
            }

            internal override SqlStatement VisitInsert(SqlInsert sin) {
                bool saveTop = this.topLevel;
                this.topLevel = false;
                base.VisitInsert(sin);
                this.topLevel = saveTop;
                return sin;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate sup) {
                bool saveTop = this.topLevel;
                this.topLevel = false;
                base.VisitUpdate(sup);
                this.topLevel = saveTop;
                return sup;
            }

            internal override SqlStatement VisitDelete(SqlDelete sd) {
                bool saveTop = this.topLevel;
                this.topLevel = false;
                base.VisitDelete(sd);
                this.topLevel = saveTop;
                return sd;
            }

            internal override SqlExpression VisitValue(SqlValue value) {
                if (this.topLevel || !value.IsClientSpecified || !value.SqlType.CanBeParameter) {
                    return value;
                }
                else {
                    return this.InsertLookup(value);
                }
            }

            internal override SqlExpression VisitClientParameter(SqlClientParameter cp) {
                if (cp.SqlType.CanBeParameter) {
                    SqlParameter p = new SqlParameter(cp.ClrType, cp.SqlType, this.parameterizer.CreateParameterName(), cp.SourceExpression);
                    this.currentParams.Add(new SqlParameterInfo(p, cp.Accessor.Compile()));
                    return p;
                }
                else {
                    return cp;
                }
            }
        }
    }
}
