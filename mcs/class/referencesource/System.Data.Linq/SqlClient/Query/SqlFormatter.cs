using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Linq;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    internal class SqlFormatter : DbFormatter {
        private Visitor visitor;

        internal SqlFormatter() {
            this.visitor = new Visitor();
        }

        internal override string Format(SqlNode node, bool isDebug) {
            return this.visitor.Format(node, isDebug);
        }

        internal string[] FormatBlock(SqlBlock block, bool isDebug) {
            List<string> results = new List<string>(block.Statements.Count);
            for (int i = 0, n = block.Statements.Count; i < n; i++) {
                SqlStatement stmt = block.Statements[i];
                SqlSelect select = stmt as SqlSelect;
                if (select != null && select.DoNotOutput) {
                    continue;
                } else {
                    results.Add(this.Format(stmt, isDebug));
                }
            }
            return results.ToArray();
        }

        internal override string Format(SqlNode node) {
            return this.visitor.Format(node);
        }

        internal bool ParenthesizeTop {
            get { return this.visitor.parenthesizeTop; }
            set { this.visitor.parenthesizeTop = value; }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal class Visitor : SqlVisitor {
            internal StringBuilder sb;
            internal bool isDebugMode;
            internal List<SqlSource> suppressedAliases = new List<SqlSource>();
            internal Dictionary<SqlNode, string> names = new Dictionary<SqlNode, string>();
            internal Dictionary<SqlColumn, SqlAlias> aliasMap = new Dictionary<SqlColumn, SqlAlias>();
            internal int depth;
            internal bool parenthesizeTop;

            internal Visitor() {
            }

            internal string Format(SqlNode node, bool isDebug) {
                this.sb = new StringBuilder();
                this.isDebugMode = isDebug;
                this.aliasMap.Clear();
                if (isDebug) {
                    new AliasMapper(this.aliasMap).Visit(node);
                }
                this.Visit(node);
                return this.sb.ToString();
            }

            internal string Format(SqlNode node) {
                return this.Format(node, false);
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal virtual void VisitWithParens(SqlNode node, SqlNode outer) {
                if (node == null)
                    return;
                switch (node.NodeType) {
                    case SqlNodeType.ColumnRef:
                    case SqlNodeType.Value:
                    case SqlNodeType.Member:
                    case SqlNodeType.Parameter:
                    case SqlNodeType.FunctionCall:
                    case SqlNodeType.TableValuedFunctionCall:
                    case SqlNodeType.OuterJoinedValue:
                        this.Visit(node);
                        break;
                    case SqlNodeType.Add:
                    case SqlNodeType.Mul:
                    case SqlNodeType.And:
                    case SqlNodeType.Or:
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                    case SqlNodeType.BitAnd:
                    case SqlNodeType.BitOr:
                    case SqlNodeType.BitXor:
                    case SqlNodeType.BitNot:
                        if (outer.NodeType != node.NodeType)
                            goto default;
                        this.Visit(node);
                        break;

                    default:
                        this.sb.Append("(");
                        this.Visit(node);
                        this.sb.Append(")");
                        break;
                }
            }

            internal override SqlExpression VisitNop(SqlNop nop) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("NOP");
                }
                sb.Append("NOP()");
                return nop;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal override SqlExpression VisitUnaryOperator(SqlUnary uo) {
                switch (uo.NodeType) {
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                        this.sb.Append(GetOperator(uo.NodeType));
                        this.sb.Append(" ");
                        this.VisitWithParens(uo.Operand, uo);
                        break;
                    case SqlNodeType.Negate:
                    case SqlNodeType.BitNot:
                        this.sb.Append(GetOperator(uo.NodeType));
                        this.VisitWithParens(uo.Operand, uo);
                        break;
                    case SqlNodeType.Count:
                    case SqlNodeType.LongCount:
                    case SqlNodeType.Max:
                    case SqlNodeType.Min:
                    case SqlNodeType.Sum:
                    case SqlNodeType.Avg:
                    case SqlNodeType.Stddev:
                    case SqlNodeType.ClrLength:{
                            this.sb.Append(GetOperator(uo.NodeType));
                            this.sb.Append("(");
                            if (uo.Operand == null) {
                                this.sb.Append("*");
                            } else {
                                this.Visit(uo.Operand);
                            }
                            this.sb.Append(")");
                            break;
                        }
                    case SqlNodeType.IsNull:
                    case SqlNodeType.IsNotNull: {
                            this.VisitWithParens(uo.Operand, uo);
                            sb.Append(" ");
                            sb.Append(GetOperator(uo.NodeType));
                            break;
                        }
                    case SqlNodeType.Convert: {
                            this.sb.Append("CONVERT(");
                            QueryFormatOptions options = QueryFormatOptions.None; 
                            if(uo.Operand.SqlType.CanSuppressSizeForConversionToString) {
                                options = QueryFormatOptions.SuppressSize;
                            }
                            this.sb.Append(uo.SqlType.ToQueryString(options));
                            this.sb.Append(",");
                            this.Visit(uo.Operand);
                            this.sb.Append(")");
                            break;
                        }
                    case SqlNodeType.ValueOf:
                    case SqlNodeType.OuterJoinedValue:
                        this.Visit(uo.Operand); // no op
                        break;
                    default:
                        throw Error.InvalidFormatNode(uo.NodeType);
                }
                return uo;
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber) {
                sb.Append("ROW_NUMBER() OVER (ORDER BY ");

                for (int i = 0, n = rowNumber.OrderBy.Count; i < n; i++) {
                    SqlOrderExpression exp = rowNumber.OrderBy[i];

                    if (i > 0) sb.Append(", ");

                    this.Visit(exp.Expression);

                    if (exp.OrderType == SqlOrderType.Descending) {
                        sb.Append(" DESC");
                    }
                }

                sb.Append(")");

                return rowNumber;
            }

            internal override SqlExpression VisitLift(SqlLift lift) {
                this.Visit(lift.Expression);
                return lift;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
                switch (bo.NodeType) {
                    case SqlNodeType.Coalesce:
                        sb.Append("COALESCE(");
                        this.Visit(bo.Left);
                        sb.Append(",");
                        this.Visit(bo.Right);
                        sb.Append(")");
                        break;
                    default:
                        this.VisitWithParens(bo.Left, bo);
                        sb.Append(" ");
                        sb.Append(GetOperator(bo.NodeType));
                        sb.Append(" ");
                        this.VisitWithParens(bo.Right, bo);
                        break;
                }
                return bo;
            }

            internal override SqlExpression VisitBetween(SqlBetween between) {
                this.VisitWithParens(between.Expression, between);
                sb.Append(" BETWEEN ");
                this.Visit(between.Start);
                sb.Append(" AND ");
                this.Visit(between.End);
                return between;
            }

            internal override SqlExpression VisitIn(SqlIn sin) {
                this.VisitWithParens(sin.Expression, sin);
                sb.Append(" IN (");
                for (int i = 0, n = sin.Values.Count; i < n; i++) {
                    if (i > 0) {
                        sb.Append(", ");
                    }
                    this.Visit(sin.Values[i]);
                }
                sb.Append(")");
                return sin;
            }

            internal override SqlExpression VisitLike(SqlLike like) {
                this.VisitWithParens(like.Expression, like);
                sb.Append(" LIKE ");
                this.Visit(like.Pattern);
                if (like.Escape != null) {
                    sb.Append(" ESCAPE ");
                    this.Visit(like.Escape);
                }
                return like;
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc) {
                if (fc.Name.Contains(".")) {
                    // Assume UDF -- bracket the name.
                    this.WriteName(fc.Name);
                } else {
                    // No ".", so we assume it's a system function name and leave it alone.
                    sb.Append(fc.Name);
                }
                
                sb.Append("(");
                for (int i = 0, n = fc.Arguments.Count; i < n; i++) {
                    if (i > 0)
                        sb.Append(", ");
                    this.Visit(fc.Arguments[i]);
                }
                sb.Append(")");
                return fc;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc) {
                // both scalar and table valued functions are formatted the same
                return VisitFunctionCall(fc);
            }

            internal override SqlExpression VisitCast(SqlUnary c) {
                sb.Append("CAST(");
                this.Visit(c.Operand);
                sb.Append(" AS ");
                QueryFormatOptions options = QueryFormatOptions.None;
                if (c.Operand.SqlType.CanSuppressSizeForConversionToString) {
                    options = QueryFormatOptions.SuppressSize;
                }
                sb.Append(c.SqlType.ToQueryString(options));
                sb.Append(")");
                return c;
            }

            internal override SqlExpression VisitTreat(SqlUnary t) {
                sb.Append("TREAT(");
                this.Visit(t.Operand);
                sb.Append(" AS ");
                this.FormatType(t.SqlType);
                sb.Append(")");
                return t;
            }

            internal override SqlExpression VisitColumn(SqlColumn c) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("Column");
                }
                sb.Append("COLUMN(");
                if (c.Expression != null) {
                    this.Visit(c.Expression);
                }
                else {
                    string aliasName = null;
                    if (c.Alias != null) {
                        if (c.Alias.Name == null) {
                            if (!this.names.TryGetValue(c.Alias, out aliasName)) {
                                aliasName = "A" + this.names.Count;
                                this.names[c.Alias] = aliasName;
                            }
                        } else {
                            aliasName = c.Alias.Name;
                        }
                    }
                    sb.Append(aliasName);
                    sb.Append(".");
                    sb.Append(c.Name);
                }
                sb.Append(")");
                return c;
            }

            internal override SqlExpression VisitDiscriminatedType(SqlDiscriminatedType dt) {
                if (this.isDebugMode) {
                    sb.Append("DTYPE(");
                }
                base.VisitDiscriminatedType(dt);
                if (this.isDebugMode) {
                    sb.Append(")");
                }
                return dt;
            }

            internal override SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof) {
                if (this.isDebugMode) {
                    sb.Append("DISCO(");
                }
                base.VisitDiscriminatorOf(dof);
                if (this.isDebugMode) {
                    sb.Append(")");
                }
                return dof;
            }

            internal override SqlExpression VisitSimpleExpression(SqlSimpleExpression simple) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("SIMPLE");
                }
                sb.Append("SIMPLE(");
                base.VisitSimpleExpression(simple);
                sb.Append(")");
                return simple;
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("Shared");
                }
                sb.Append("SHARED(");
                this.Visit(shared.Expression);
                sb.Append(")");
                return shared;
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("SharedRef");
                }
                sb.Append("SHAREDREF(");
                this.Visit(sref.SharedExpression.Expression);
                sb.Append(")");
                return sref;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                string aliasName = null;
                SqlColumn c = cref.Column;
                SqlAlias alias = c.Alias;
                if (alias == null) {
                    this.aliasMap.TryGetValue(c, out alias);
                }
                if (alias != null) {
                    if (alias.Name == null) {
                        if (!this.names.TryGetValue(alias, out aliasName)) {
                            aliasName = "A" + this.names.Count;
                            this.names[c.Alias] = aliasName;
                        }
                    } else {
                        aliasName = c.Alias.Name;
                    }
                }
                if (!this.suppressedAliases.Contains(c.Alias) && aliasName != null && aliasName.Length != 0) {
                    this.WriteName(aliasName);
                    sb.Append(".");
                }
                string name = c.Name;
                string inferredName = this.InferName(c.Expression, null);
                if (name == null)
                    name = inferredName;
                if (name == null) {
                    if (!this.names.TryGetValue(c, out name)) {
                        name = "C" + this.names.Count;
                        this.names[c] = name;
                    }
                }
                this.WriteName(name);
                return cref;
            }

            internal virtual void WriteName(string s) {
                sb.Append(SqlIdentifier.QuoteCompoundIdentifier(s));
            }

            internal virtual void WriteVariableName(string s) {
                if (s.StartsWith("@",StringComparison.Ordinal))
                    sb.Append(SqlIdentifier.QuoteCompoundIdentifier(s));
                else
                    sb.Append(SqlIdentifier.QuoteCompoundIdentifier("@" + s));
            }

            internal override SqlExpression VisitParameter(SqlParameter p) {
                sb.Append(p.Name);
                return p;
            }

            internal override SqlExpression VisitValue(SqlValue value) {
                if (value.IsClientSpecified && !this.isDebugMode) {
                    throw Error.InvalidFormatNode("Value");
                }
                else {
                    this.FormatValue(value.Value);
                }
                return value;
            }

            internal override SqlExpression VisitClientParameter(SqlClientParameter cp) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("ClientParameter");
                }
                else {
                    sb.Append("client-parameter(");
                    object value;
                    try {
                        value = cp.Accessor.Compile().DynamicInvoke(new object[] { null });
                    } catch (System.Reflection.TargetInvocationException e) {
                        throw e.InnerException;
                    }

                    sb.Append(value);
                    sb.Append(")");
                }
                return cp;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                int saveDepth = this.depth;
                this.depth++;
                if (this.isDebugMode) {
                    sb.Append("SCALAR");
                }
                sb.Append("(");
                this.NewLine();
                this.Visit(ss.Select);
                this.NewLine();
                sb.Append(")");
                this.depth = saveDepth;
                return ss;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("Element");
                }
                int saveDepth = this.depth;
                this.depth++;
                sb.Append("ELEMENT(");
                this.NewLine();
                this.Visit(elem.Select);
                this.NewLine();
                sb.Append(")");
                this.depth = saveDepth;
                return elem;
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("Multiset");
                }
                int saveDepth = this.depth;
                this.depth++;
                sb.Append("MULTISET(");
                this.NewLine();
                this.Visit(sms.Select);
                this.NewLine();
                sb.Append(")");
                this.depth = saveDepth;
                return sms;
            }

            internal override SqlExpression VisitExists(SqlSubSelect sqlExpr) {
                int saveDepth = this.depth;
                this.depth++;
                sb.Append("EXISTS(");
                this.NewLine();
                this.Visit(sqlExpr.Select);
                this.NewLine();
                sb.Append(")");
                this.depth = saveDepth;
                return sqlExpr;
            }

            internal override SqlTable VisitTable(SqlTable tab) {
                string name = tab.Name;
                this.WriteName(name);
                return tab;
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq) {
                if (suq.Arguments.Count > 0) {
                    // compute all the arg values...
                    StringBuilder savesb = this.sb;
                    this.sb = new StringBuilder();
                    object[] args = new object[suq.Arguments.Count];
                    for (int i = 0, n = args.Length; i < n; i++) {
                        this.Visit(suq.Arguments[i]);
                        args[i] = this.sb.ToString();
                        this.sb.Length = 0;
                    }
                    this.sb = savesb;
                    // append query with args...
                    sb.Append(string.Format(CultureInfo.InvariantCulture, suq.QueryText, args));
                } else {
                    sb.Append(suq.QueryText);
                }
                return suq;
            }

            internal override SqlExpression VisitUserColumn(SqlUserColumn suc) {
                sb.Append(suc.Name);
                return suc;
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc) {
                sb.Append("EXEC @RETURN_VALUE = ");
                this.WriteName(spc.Function.MappedName);
                sb.Append(" ");

                int pc = spc.Function.Parameters.Count;
                System.Diagnostics.Debug.Assert(spc.Arguments.Count >= pc);

                for (int i = 0;  i < pc; i++) {
                    MetaParameter mp = spc.Function.Parameters[i];
                    if (i > 0) sb.Append(", ");
                    this.WriteVariableName(mp.MappedName);
                    sb.Append(" = ");
                    this.Visit(spc.Arguments[i]);
                    if (mp.Parameter.IsOut || mp.Parameter.ParameterType.IsByRef)
                        sb.Append(" OUTPUT");
                }

                if (spc.Arguments.Count > pc) {
                    if (pc > 0) sb.Append(", ");
                    this.WriteVariableName(spc.Function.ReturnParameter.MappedName);
                    sb.Append(" = ");
                    this.Visit(spc.Arguments[pc]);
                    sb.Append(" OUTPUT");
                }

                return spc;
            }

            internal override SqlAlias VisitAlias(SqlAlias alias) {
                bool isSelect = alias.Node is SqlSelect;
                int saveDepth = this.depth;
                string aliasName = null;
                string name = "";
                SqlTable table = alias.Node as SqlTable;
                if (table != null) {
                    name = table.Name;
                }
                if (alias.Name == null) {
                    if (!this.names.TryGetValue(alias, out aliasName)) {
                        aliasName = "A" + this.names.Count;
                        this.names[alias] = aliasName;
                    }
                } else {
                    aliasName = alias.Name;
                }
                if (isSelect) {
                    this.depth++;
                    sb.Append("(");
                    this.NewLine();
                }
                this.Visit(alias.Node);
                if (isSelect) {
                    this.NewLine();
                    sb.Append(")");
                    this.depth = saveDepth;
                }
                if (!this.suppressedAliases.Contains(alias) && aliasName != null && name != aliasName) {
                    sb.Append(" AS ");
                    this.WriteName(aliasName);
                }
                return alias;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref) {
                sb.Append("AREF(");
                this.WriteAliasName(aref.Alias);
                sb.Append(")");
                return aref;
            }

            private void WriteAliasName(SqlAlias alias) {
                string aliasName = null;
                if (alias.Name == null) {
                    if (!this.names.TryGetValue(alias, out aliasName)) {
                        aliasName = "A" + this.names.Count;
                        this.names[alias] = aliasName;
                    }
                }
                else {
                    aliasName = alias.Name;
                }
                this.WriteName(aliasName);
            }

            internal override SqlNode VisitUnion(SqlUnion su) {
                sb.Append("(");
                int saveDepth = this.depth;
                this.depth++;
                this.NewLine();
                this.Visit(su.Left);
                this.NewLine();
                sb.Append("UNION");
                if (su.All) {
                    sb.Append(" ALL");
                }
                this.NewLine();
                this.Visit(su.Right);
                this.NewLine();
                sb.Append(")");
                this.depth = saveDepth;
                return su;
            }

            internal override SqlExpression VisitExprSet(SqlExprSet xs) {
                if (this.isDebugMode) {
                    sb.Append("ES(");
                    for (int i = 0, n = xs.Expressions.Count; i < n; i++) {
                        if (i > 0)
                            sb.Append(", ");
                        this.Visit(xs.Expressions[i]);
                    }
                    sb.Append(")");
                } else {
                    // only show the first one
                    this.Visit(xs.GetFirstExpression());
                }
                return xs;
            }

            internal override SqlRow VisitRow(SqlRow row) {
                for (int i = 0, n = row.Columns.Count; i < n; i++) {
                    SqlColumn c = row.Columns[i];
                    if (i > 0)
                        sb.Append(", ");
                    this.Visit(c.Expression);
                    string name = c.Name;
                    string inferredName = this.InferName(c.Expression, null);
                    if (name == null)
                        name = inferredName;
                    if (name == null) {
                        if (!this.names.TryGetValue(c, out name)) {
                            name = "C" + this.names.Count;
                            this.names[c] = name;
                        }
                    }
                    if (name != inferredName && !String.IsNullOrEmpty(name)) {
                        sb.Append(" AS ");
                        this.WriteName(name);
                    }
                }
                return row;
            }

            internal override SqlExpression VisitNew(SqlNew sox) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("New");
                }
                sb.Append("new ");
                sb.Append(sox.ClrType.Name);
                sb.Append("{ ");
                // Visit Args
                for (int i = 0, n = sox.Args.Count; i < n; i++)
                {
                    SqlExpression argExpr = sox.Args[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(sox.ArgMembers[i].Name);
                    sb.Append(" = ");
                    this.Visit(argExpr);
                }
                // Visit Members
                for (int i = 0, n = sox.Members.Count; i < n; i++) {
                    SqlMemberAssign ma = sox.Members[i];
                    if (i > 0) sb.Append(", ");
                    string ename = this.InferName(ma.Expression, null);
                    if (ename != ma.Member.Name) {
                        sb.Append(ma.Member.Name);
                        sb.Append(" = ");
                    }
                    this.Visit(ma.Expression);
                }
                sb.Append(" }");
                return sox;
            }

            internal override SqlExpression VisitClientArray(SqlClientArray scar) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("ClientArray");
                }
                sb.Append("new []{");
                for (int i = 0, n = scar.Expressions.Count; i < n; i++) {
                    if (i > 0) sb.Append(", ");
                    this.Visit(scar.Expressions[i]);
                }
                sb.Append("}");
                return scar;
            }

            internal override SqlNode VisitMember(SqlMember m) {
                this.Visit(m.Expression);
                sb.Append(".");
                sb.Append(m.Member.Name);
                return m;
            }

            internal virtual void NewLine() {
                if (sb.Length > 0) {
                    sb.AppendLine();
                }
                for (int i = 0; i < this.depth; i++) {
                    sb.Append("    ");
                }
            }

            internal override SqlSelect VisitSelect(SqlSelect ss) {
                if (ss.DoNotOutput) {
                    return ss;
                }
                string from = null;
                if (ss.From != null) {
                    StringBuilder savesb = this.sb;
                    this.sb = new StringBuilder();
                    if (this.IsSimpleCrossJoinList(ss.From)) {
                        this.VisitCrossJoinList(ss.From);
                    } else {
                        this.Visit(ss.From);
                    }
                    from = this.sb.ToString();
                    this.sb = savesb;
                }

                sb.Append("SELECT ");

                if (ss.IsDistinct) {
                    sb.Append("DISTINCT ");
                }

                if (ss.Top != null) {
                    sb.Append("TOP ");
                    if (this.parenthesizeTop) {
                        sb.Append("(");
                    }
                    this.Visit(ss.Top);
                    if (this.parenthesizeTop) {
                        sb.Append(")");
                    }
                    sb.Append(" ");
                    if (ss.IsPercent) {
                        sb.Append(" PERCENT ");
                    }
                }

                if (ss.Row.Columns.Count > 0) {
                    this.VisitRow(ss.Row);
                } else if (this.isDebugMode) {
                    this.Visit(ss.Selection);
                } else {
                    sb.Append("NULL AS [EMPTY]");
                }

                if (from != null) {
                    this.NewLine();
                    sb.Append("FROM ");
                    sb.Append(from);
                }
                if (ss.Where != null) {
                    this.NewLine();
                    sb.Append("WHERE ");
                    this.Visit(ss.Where);
                }
                if (ss.GroupBy.Count > 0) {
                    this.NewLine();
                    sb.Append("GROUP BY ");
                    for (int i = 0, n = ss.GroupBy.Count; i < n; i++) {
                        SqlExpression exp = ss.GroupBy[i];
                        if (i > 0)
                            sb.Append(", ");
                        this.Visit(exp);
                    }
                    if (ss.Having != null) {
                        this.NewLine();
                        sb.Append("HAVING ");
                        this.Visit(ss.Having);
                    }
                }
                if (ss.OrderBy.Count > 0 && ss.OrderingType != SqlOrderingType.Never) {
                    this.NewLine();
                    sb.Append("ORDER BY ");
                    for (int i = 0, n = ss.OrderBy.Count; i < n; i++) {
                        SqlOrderExpression exp = ss.OrderBy[i];
                        if (i > 0)
                            sb.Append(", ");
                        this.Visit(exp.Expression);
                        if (exp.OrderType == SqlOrderType.Descending) {
                            sb.Append(" DESC");
                        }
                    }
                }

                return ss;
            }

            internal virtual bool IsSimpleCrossJoinList(SqlNode node) {
                SqlJoin join = node as SqlJoin;
                if (join != null) {
                    return join.JoinType == SqlJoinType.Cross &&
                        this.IsSimpleCrossJoinList(join.Left) &&
                        this.IsSimpleCrossJoinList(join.Right);
                }
                SqlAlias alias = node as SqlAlias;
                return (alias != null && alias.Node is SqlTable);
            }

            internal virtual void VisitCrossJoinList(SqlNode node) {
                SqlJoin join = node as SqlJoin;
                if (join != null) {
                    this.VisitCrossJoinList(join.Left);
                    sb.Append(", ");
                    this.VisitCrossJoinList(join.Right);
                } else {
                    this.Visit(node);
                }
            }

            internal void VisitJoinSource(SqlSource src) {
                if (src.NodeType == SqlNodeType.Join) {
                    this.depth++;
                    sb.Append("(");
                    this.Visit(src);
                    sb.Append(")");
                    this.depth--;
                } else {
                    this.Visit(src);
                }
            }

            internal override SqlSource VisitJoin(SqlJoin join) {
                this.Visit(join.Left);
                this.NewLine();
                switch (join.JoinType) {
                    case SqlJoinType.CrossApply:
                        sb.Append("CROSS APPLY ");
                        break;
                    case SqlJoinType.Cross:
                        sb.Append("CROSS JOIN ");
                        break;
                    case SqlJoinType.Inner:
                        sb.Append("INNER JOIN ");
                        break;
                    case SqlJoinType.LeftOuter:
                        sb.Append("LEFT OUTER JOIN ");
                        break;
                    case SqlJoinType.OuterApply:
                        sb.Append("OUTER APPLY ");
                        break;
                }
                SqlJoin rightJoin = join.Right as SqlJoin;
                if (rightJoin == null || 
                     (rightJoin.JoinType == SqlJoinType.Cross 
                       && join.JoinType != SqlJoinType.CrossApply 
                       && join.JoinType != SqlJoinType.OuterApply)) {
                    this.Visit(join.Right);
                } else {
                    this.VisitJoinSource(join.Right);
                }
                if (join.Condition != null) {
                    sb.Append(" ON ");
                    this.Visit(join.Condition);
                } else if (this.RequiresOnCondition(join.JoinType)) {
                    sb.Append(" ON 1=1 ");
                }
                return join;
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            internal bool RequiresOnCondition(SqlJoinType joinType) {
                switch (joinType) {
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.Cross:
                    case SqlJoinType.OuterApply:
                        return false;
                    case SqlJoinType.Inner:
                    case SqlJoinType.LeftOuter:
                        return true;
                    default:
                        throw Error.InvalidFormatNode(joinType);
                }
            }

            internal override SqlBlock VisitBlock(SqlBlock block) {
                for (int i = 0, n = block.Statements.Count; i < n; i++) {
                    this.Visit(block.Statements[i]);
                    if (i < n - 1) {
                        SqlSelect select = block.Statements[i+1] as SqlSelect;
                        if (select == null || !select.DoNotOutput) {
                            this.NewLine();
                            this.NewLine();
                        }
                    }
                }
                return block;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("ClientQuery");
                }
                sb.Append("client(");
                for (int i = 0, n = cq.Arguments.Count; i < n; i++) {
                    if (i > 0) sb.Append(", ");
                    this.Visit(cq.Arguments[i]);
                }
                sb.Append("; ");
                this.Visit(cq.Query);
                sb.Append(")");
                return cq;
            }

            internal override SqlExpression VisitJoinedCollection(SqlJoinedCollection jc) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("JoinedCollection");
                }
                sb.Append("big-join(");
                this.Visit(jc.Expression);
                sb.Append(", ");
                this.Visit(jc.Count);
                sb.Append(")");
                return jc;
            }

            internal override SqlStatement VisitDelete(SqlDelete sd) {
                sb.Append("DELETE FROM ");
                this.suppressedAliases.Add(sd.Select.From);
                this.Visit(sd.Select.From);
                if (sd.Select.Where != null) {
                    sb.Append(" WHERE ");
                    this.Visit(sd.Select.Where);
                }
                this.suppressedAliases.Remove(sd.Select.From);
                return sd;
            }

            internal override SqlStatement VisitInsert(SqlInsert si) {

                if (si.OutputKey != null) {
                    sb.Append("DECLARE @output TABLE(");
                    this.WriteName(si.OutputKey.Name);
                    sb.Append(" ");
                    sb.Append(si.OutputKey.SqlType.ToQueryString());
                    sb.Append(")");
                    this.NewLine();
                    if (si.OutputToLocal) {
                        sb.Append("DECLARE @id ");
                        sb.Append(si.OutputKey.SqlType.ToQueryString());
                        this.NewLine();
                    }
                }

                sb.Append("INSERT INTO ");
                this.Visit(si.Table);

                if (si.Row.Columns.Count != 0) {
                    // INSERT INTO table (...columns...) VALUES (...values...)
                    sb.Append("(");
                    for (int i = 0, n = si.Row.Columns.Count; i < n; i++) {
                        if (i > 0) sb.Append(", ");
                        this.WriteName(si.Row.Columns[i].Name);
                    }
                    sb.Append(")");
                }

                if (si.OutputKey != null) {
                    this.NewLine();
                    sb.Append("OUTPUT INSERTED.");
                    this.WriteName(si.OutputKey.MetaMember.MappedName);
                    sb.Append(" INTO @output");
                }

                if (si.Row.Columns.Count == 0) {
                    sb.Append(" DEFAULT VALUES");
                }
                else {
                    // VALUES (...values...)
                    this.NewLine();
                    sb.Append("VALUES (");
                    if (this.isDebugMode && si.Row.Columns.Count == 0) {
                        this.Visit(si.Expression);
                    } else {
                        for (int i = 0, n = si.Row.Columns.Count; i < n; i++) {
                            if (i > 0) sb.Append(", ");
                            this.Visit( si.Row.Columns[i].Expression);
                        }
                    }
                    sb.Append(")");
                }

                if (si.OutputKey != null) {
                    this.NewLine();
                    if (si.OutputToLocal) {
                        sb.Append("SELECT @id = ");
                        sb.Append(si.OutputKey.Name);
                        sb.Append(" FROM @output");
                    }
                    else {
                        sb.Append("SELECT ");
                        this.WriteName(si.OutputKey.Name);
                        sb.Append(" FROM @output");
                    }
                }

                return si;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate su) {
                sb.Append("UPDATE ");
                this.suppressedAliases.Add(su.Select.From);
                this.Visit(su.Select.From);
                this.NewLine();
                sb.Append("SET ");

                for (int i = 0, n = su.Assignments.Count; i < n; i++) {
                    if (i > 0) sb.Append(", ");
                    SqlAssign sa = su.Assignments[i];
                    this.Visit(sa.LValue);
                    sb.Append(" = ");
                    this.Visit(sa.RValue);
                }
                if (su.Select.Where != null) {
                    this.NewLine();
                    sb.Append("WHERE ");
                    this.Visit(su.Select.Where);
                }
                this.suppressedAliases.Remove(su.Select.From);
                return su;
            }

            internal override SqlStatement VisitAssign(SqlAssign sa) {
                sb.Append("SET ");
                this.Visit(sa.LValue);
                sb.Append(" = ");
                this.Visit(sa.RValue);
                return sa;
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c) {
                this.depth++;
                this.NewLine();
                sb.Append("(CASE ");
                this.depth++;
                for (int i = 0, n = c.Whens.Count; i < n; i++) {
                    SqlWhen when = c.Whens[i];
                    this.NewLine();
                    sb.Append("WHEN ");
                    this.Visit(when.Match);
                    sb.Append(" THEN ");
                    this.Visit(when.Value);
                }
                if (c.Else != null) {
                    this.NewLine();
                    sb.Append("ELSE ");
                    this.Visit(c.Else);
                }
                this.depth--;
                this.NewLine();
                sb.Append(" END)");
                this.depth--;
                return c;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c) {
                this.depth++;
                this.NewLine();
                sb.Append("(CASE");
                this.depth++;
                if (c.Expression != null) {
                    sb.Append(" ");
                    this.Visit(c.Expression);
                }
                for (int i = 0, n = c.Whens.Count; i < n; i++) {
                    SqlWhen when = c.Whens[i];
                    if (i == n - 1 && when.Match == null) {
                        this.NewLine();
                        sb.Append("ELSE ");
                        this.Visit(when.Value);
                    } else {
                        this.NewLine();
                        sb.Append("WHEN ");
                        this.Visit(when.Match);
                        sb.Append(" THEN ");
                        this.Visit(when.Value);
                    }
                }
                this.depth--;
                this.NewLine();
                sb.Append(" END)");
                this.depth--;
                return c;
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("ClientCase");
                }
                this.depth++;
                this.NewLine();
                sb.Append("(CASE");
                this.depth++;
                if (c.Expression != null) {
                    sb.Append(" ");
                    this.Visit(c.Expression);
                }
                for (int i = 0, n = c.Whens.Count; i < n; i++) {
                    SqlClientWhen when = c.Whens[i];
                    if (i == n - 1 && when.Match == null) {
                        this.NewLine();
                        sb.Append("ELSE ");
                        this.Visit(when.Value);
                    } else {
                        this.NewLine();
                        sb.Append("WHEN ");
                        this.Visit(when.Match);
                        sb.Append(" THEN ");
                        this.Visit(when.Value);
                    }
                }
                this.depth--;
                this.NewLine();
                sb.Append(" END)");
                this.depth--;
                return c;
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase c) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("TypeCase");
                }
                this.depth++;
                this.NewLine();
                sb.Append("(CASE");
                this.depth++;
                if (c.Discriminator != null) {
                    sb.Append(" ");
                    this.Visit(c.Discriminator);
                }
                for (int i = 0, n = c.Whens.Count; i < n; i++) {
                    SqlTypeCaseWhen when = c.Whens[i];
                    if (i == n - 1 && when.Match == null) {
                        this.NewLine();
                        sb.Append("ELSE ");
                        this.Visit(when.TypeBinding);
                    } else {
                        this.NewLine();
                        sb.Append("WHEN ");
                        this.Visit(when.Match);
                        sb.Append(" THEN ");
                        this.Visit(when.TypeBinding);
                    }
                }
                this.depth--;
                this.NewLine();
                sb.Append(" END)");
                this.depth--;
                return c;
            }

            internal override SqlExpression VisitVariable(SqlVariable v) {
                sb.Append(v.Name);
                return v;
            }

            private string InferName(SqlExpression exp, string def) {
                if (exp == null) return null;
                switch (exp.NodeType) {
                    case SqlNodeType.Member:
                        return ((SqlMember)exp).Member.Name;
                    case SqlNodeType.Column:
                        return ((SqlColumn)exp).Name;
                    case SqlNodeType.ColumnRef:
                        return ((SqlColumnRef)exp).Column.Name;
                    case SqlNodeType.ExprSet:
                        return this.InferName(((SqlExprSet)exp).Expressions[0], def);
                    default:
                        return def;
                }
            }

            private void FormatType(ProviderType type) {
                sb.Append(type.ToQueryString());
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal virtual void FormatValue(object value) {
                if (value == null) {
                    sb.Append("NULL");
                } else {
                    Type t = value.GetType();
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                        t = t.GetGenericArguments()[0];
                    TypeCode tc = Type.GetTypeCode(t);
                    switch (tc) {
                        case TypeCode.Char:
                        case TypeCode.String:
                        case TypeCode.DateTime:
                            sb.Append("'");
                            sb.Append(this.EscapeSingleQuotes(value.ToString()));
                            sb.Append("'");
                            return;
                        case TypeCode.Boolean:
                            sb.Append(this.GetBoolValue((bool)value));
                            return;
                        case TypeCode.Byte:
                        case TypeCode.Decimal:
                        case TypeCode.Double:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.SByte:
                        case TypeCode.Single:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            sb.Append(value);
                            return;
                        case TypeCode.Object: {
                                if (value is Guid) {
                                    sb.Append("'");
                                    sb.Append(value);
                                    sb.Append("'");
                                    return;
                                }
                                Type valueType = value as Type;
                                if (valueType != null) {
                                    if (this.isDebugMode) {
                                        sb.Append("typeof(");
                                        sb.Append(valueType.Name);
                                        sb.Append(")");
                                    } else {
                                        this.FormatValue("");
                                    }
                                    return;
                                }
                                break;
                            }
                    }
                    if (this.isDebugMode) {
                        sb.Append("value(");
                        sb.Append(value.ToString());
                        sb.Append(")");
                    }
                    else {
                        throw Error.ValueHasNoLiteralInSql(value);
                    }
                }
            }

            internal virtual string GetBoolValue(bool value) {
                return value ? "1" : "0";
            }

            internal virtual string EscapeSingleQuotes(string str) {
                if (str.IndexOf('\'') < 0) return str;
                StringBuilder tempStringBuilder = new StringBuilder();
                foreach (char c in str) {
                    if (c == '\'') {
                        tempStringBuilder.Append("''");
                    } else {
                        tempStringBuilder.Append("'");
                    }
                }
                return tempStringBuilder.ToString();
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal virtual string GetOperator(SqlNodeType nt) {
                switch (nt) {
                    case SqlNodeType.Add: return "+";
                    case SqlNodeType.Sub: return "-";
                    case SqlNodeType.Mul: return "*";
                    case SqlNodeType.Div: return "/";
                    case SqlNodeType.Mod: return "%";
                    case SqlNodeType.Concat: return "+";
                    case SqlNodeType.BitAnd: return "&";
                    case SqlNodeType.BitOr: return "|";
                    case SqlNodeType.BitXor: return "^";
                    case SqlNodeType.And: return "AND";
                    case SqlNodeType.Or: return "OR";
                    case SqlNodeType.GE: return ">=";
                    case SqlNodeType.GT: return ">";
                    case SqlNodeType.LE: return "<=";
                    case SqlNodeType.LT: return "<";
                    case SqlNodeType.EQ: return "=";
                    case SqlNodeType.EQ2V: return "=";
                    case SqlNodeType.NE: return "<>";
                    case SqlNodeType.NE2V: return "<>";
                    case SqlNodeType.Not: return "NOT";
                    case SqlNodeType.Not2V: return "NOT";
                    case SqlNodeType.BitNot: return "~";
                    case SqlNodeType.Negate: return "-";
                    case SqlNodeType.IsNull: return "IS NULL";
                    case SqlNodeType.IsNotNull: return "IS NOT NULL";
                    case SqlNodeType.Count: return "COUNT";
                    case SqlNodeType.LongCount: return "COUNT_BIG";
                    case SqlNodeType.Min: return "MIN";
                    case SqlNodeType.Max: return "MAX";
                    case SqlNodeType.Sum: return "SUM";
                    case SqlNodeType.Avg: return "AVG";
                    case SqlNodeType.Stddev: return "STDEV";
                    case SqlNodeType.ClrLength: return "CLRLENGTH";
                    default:
                        throw Error.InvalidFormatNode(nt);
                }
            }

            internal override SqlNode VisitLink(SqlLink link) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("Link");
                }
                if (link.Expansion != null) {
                    sb.Append("LINK(");
                    this.Visit(link.Expansion);
                    sb.Append(")");
                } else {
                    sb.Append("LINK(");
                    for (int i = 0, n = link.KeyExpressions.Count; i < n; i++) {
                        if (i > 0) sb.Append(", ");
                        this.Visit(link.KeyExpressions[i]);
                    }
                    sb.Append(")");
                }
                return link;
            }

            internal override SqlMemberAssign VisitMemberAssign(SqlMemberAssign ma) {
                throw Error.InvalidFormatNode("MemberAssign");
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc) {
                if (!this.isDebugMode) {
                    throw Error.InvalidFormatNode("MethodCall");
                }
                if (mc.Method.IsStatic) {
                    sb.Append(mc.Method.DeclaringType);
                } else {
                    this.Visit(mc.Object);
                }
                sb.Append(".");
                sb.Append(mc.Method.Name);
                sb.Append("(");
                for (int i = 0, n = mc.Arguments.Count; i < n; i++) {
                    if (i > 0) sb.Append(", ");
                    this.Visit(mc.Arguments[i]);
                }
                sb.Append(")");
                return mc;
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov) {
                if (this.isDebugMode) {
                    sb.Append("opt(");
                    this.Visit(sov.HasValue);
                    sb.Append(", ");
                    this.Visit(sov.Value);
                    sb.Append(")");
                    return sov;
                } else {
                    throw Error.InvalidFormatNode("OptionalValue");
                }
            }

            internal override SqlExpression VisitUserRow(SqlUserRow row) {
                if (!isDebugMode) {
                    throw Error.InvalidFormatNode("UserRow");
                }
                return row;
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g) {
                if (!isDebugMode) {
                    throw Error.InvalidFormatNode("Grouping");
                }
                sb.Append("Group(");
                this.Visit(g.Key);
                sb.Append(", ");
                this.Visit(g.Group);
                sb.Append(")");
                return g;
            }
        }

        class AliasMapper : SqlVisitor {
            Dictionary<SqlColumn, SqlAlias> aliasMap;
            SqlAlias currentAlias;

            internal AliasMapper(Dictionary<SqlColumn, SqlAlias> aliasMap) {
                this.aliasMap = aliasMap;
            }

            internal override SqlAlias VisitAlias(SqlAlias a) {
                SqlAlias save = this.currentAlias;
                this.currentAlias = a;
                base.VisitAlias(a);
                this.currentAlias = save;
                return a;
            }

            internal override SqlExpression VisitColumn(SqlColumn col) {
                this.aliasMap[col] = this.currentAlias;
                this.Visit(col.Expression);
                return col;
            }

            internal override SqlRow VisitRow(SqlRow row) {
                foreach(SqlColumn col in row.Columns) {
                    this.VisitColumn(col);
                }
                return row;
            }

            internal override SqlTable VisitTable(SqlTable tab) {
                foreach(SqlColumn col in tab.Columns) {
                    this.VisitColumn(col);
                }
                return tab;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc) {
                foreach(SqlColumn col in fc.Columns) {
                    this.VisitColumn(col);
                }
                return fc;
            }
        }
    }
}
