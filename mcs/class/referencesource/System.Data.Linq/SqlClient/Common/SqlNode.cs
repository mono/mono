using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Data;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;
    using System.Diagnostics.CodeAnalysis;

    internal enum SqlNodeType {
        Add,
        Alias,
        AliasRef,
        And,
        Assign,
        Avg,
        Between,
        BitAnd,
        BitNot,
        BitOr,
        BitXor,
        Block,
        ClientArray,
        ClientCase,
        ClientParameter,
        ClientQuery,
        ClrLength,
        Coalesce,
        Column,
        ColumnRef,
        Concat,
        Convert,
        Count,
        Delete,
        DiscriminatedType,
        DiscriminatorOf,
        Div,
        DoNotVisit,
        Element,
        ExprSet,
        EQ,
        EQ2V,
        Exists,
        FunctionCall,
        In,
        IncludeScope,
        IsNotNull,
        IsNull,
        LE,
        Lift,
        Link,
        Like,
        LongCount,
        LT,
        GE,
        Grouping,
        GT,
        Insert,
        Join,
        JoinedCollection,
        Max,
        MethodCall,
        Member,
        MemberAssign,
        Min,
        Mod,
        Mul,
        Multiset,
        NE,
        NE2V,
        Negate,
        New,
        Not,
        Not2V,
        Nop,
        Or,
        OptionalValue,
        OuterJoinedValue,
        Parameter,
        Property,
        Row,
        RowNumber,
        ScalarSubSelect,
        SearchedCase,
        Select,
        SharedExpression,
        SharedExpressionRef,
        SimpleCase,
        SimpleExpression,
        Stddev,
        StoredProcedureCall,
        Sub,
        Sum,
        Table,
        TableValuedFunctionCall,
        Treat,
        TypeCase,
        Union,
        Update,
        UserColumn,
        UserQuery,
        UserRow,
        Variable,
        Value,
        ValueOf
    }

    [System.Diagnostics.DebuggerDisplay("text = {Text}, \r\nsource = {SourceExpression}")]
    internal abstract class SqlNode {
        private SqlNodeType nodeType;
        private Expression sourceExpression;

        internal SqlNode(SqlNodeType nodeType, Expression sourceExpression) {
            this.nodeType = nodeType;
            this.sourceExpression = sourceExpression;
        }

        internal Expression SourceExpression {
            get { return this.sourceExpression; }
        }

        internal void ClearSourceExpression() {
            this.sourceExpression = null;
        }

        internal SqlNodeType NodeType {
            get { return this.nodeType; }
        }

#if DEBUG
        private static DbFormatter formatter;
        internal static DbFormatter Formatter {
            get { return formatter; }
            set { formatter = value; }
        }

        internal string Text {
            get {
                if (Formatter == null)
                    return "SqlNode.Formatter is not assigned";
                return SqlNode.Formatter.Format(this, true);
            }
        }
#endif
    }

    internal abstract class SqlExpression : SqlNode {
        private Type clrType;
        internal SqlExpression(SqlNodeType nodeType, Type clrType, Expression sourceExpression)
            : base(nodeType, sourceExpression) {
            this.clrType = clrType;
        }

        internal Type ClrType {
            get { return this.clrType; }
        }

        // note: changing the CLR type of a node is potentially dangerous
        internal void SetClrType(Type type) {
            this.clrType = type;
        }

        internal abstract ProviderType SqlType { get; }

        /// <summary>
        /// Drill down looking for a constant root expression, returning true if found.
        /// </summary>           
        internal bool IsConstantColumn {
            get {
                if (this.NodeType == SqlNodeType.Column) {
                    SqlColumn col = (SqlColumn)this;
                    if (col.Expression != null) {
                        return col.Expression.IsConstantColumn;
                    }
                }
                else if (this.NodeType == SqlNodeType.ColumnRef) {
                    return ((SqlColumnRef)this).Column.IsConstantColumn;
                }
                else if (this.NodeType == SqlNodeType.OptionalValue) {
                    return ((SqlOptionalValue)this).Value.IsConstantColumn;
                }
                else if (this.NodeType == SqlNodeType.Value ||
                        this.NodeType == SqlNodeType.Parameter) {
                    return true;
                }
                return false;
            }
        }
    }

    /// <summary>
    /// A SqlExpression with a simple implementation of ClrType and SqlType.
    /// </summary>
    internal abstract class SqlSimpleTypeExpression : SqlExpression {
        private ProviderType sqlType;

        internal SqlSimpleTypeExpression(SqlNodeType nodeType, Type clrType, ProviderType sqlType, Expression sourceExpression)
            : base(nodeType, clrType, sourceExpression) {
            this.sqlType = sqlType;
        }

        internal override ProviderType SqlType {
            get { return this.sqlType; }
        }

        internal void SetSqlType(ProviderType type) {
            this.sqlType = type;
        }
    }

    internal class SqlDiscriminatorOf : SqlSimpleTypeExpression {
        SqlExpression obj;
        internal SqlDiscriminatorOf(SqlExpression obj, Type clrType, ProviderType sqlType, Expression sourceExpression)
            : base(SqlNodeType.DiscriminatorOf, clrType, sqlType, sourceExpression) {
            this.obj = obj;
        }
        internal SqlExpression Object {
            get { return this.obj; }
            set { this.obj = value; }
        }
    }


    /// <summary>
    /// Represents a dynamic CLR type that is chosen based on a discriminator expression.
    /// </summary>
    internal class SqlDiscriminatedType : SqlExpression {
        private ProviderType sqlType;
        private SqlExpression discriminator;
        private MetaType targetType;
        internal SqlDiscriminatedType(ProviderType sqlType, SqlExpression discriminator, MetaType targetType, Expression sourceExpression)
            : base(SqlNodeType.DiscriminatedType,
                   typeof(Type),
                   sourceExpression) {
            if (discriminator == null)
                throw Error.ArgumentNull("discriminator");
            this.discriminator = discriminator;
            this.targetType = targetType;
            this.sqlType = sqlType;
        }
        internal override ProviderType SqlType {
            get { return this.sqlType; }
        }
        internal SqlExpression Discriminator {
            get { return this.discriminator; }
            set { this.discriminator = value; }
        }

        internal MetaType TargetType {
            get { return this.targetType; }
        }
    }

    internal abstract class SqlStatement : SqlNode {
        internal SqlStatement(SqlNodeType nodeType, Expression sourceExpression)
            : base(nodeType, sourceExpression) {
        }
    }

    internal abstract class SqlSource : SqlNode {
        internal SqlSource(SqlNodeType nt, Expression sourceExpression)
            : base(nt, sourceExpression) {
        }
    }

    internal class SqlSelect : SqlStatement {
        private SqlExpression top;
        private bool isPercent;
        private bool isDistinct;
        private SqlExpression selection;
        private SqlRow row;
        private SqlSource from;
        private SqlExpression where;
        private List<SqlExpression> groupBy;
        private SqlExpression having;
        private List<SqlOrderExpression> orderBy;
        private SqlOrderingType orderingType;
        private bool squelch;

        internal SqlSelect(SqlExpression selection, SqlSource from, Expression sourceExpression)
            : base(SqlNodeType.Select, sourceExpression) {
            this.Row = new SqlRow(sourceExpression);
            this.Selection = selection;
            this.From = from;
            this.groupBy = new List<SqlExpression>();
            this.orderBy = new List<SqlOrderExpression>();
            this.orderingType = SqlOrderingType.Default;
        }

        internal SqlExpression Top {
            get { return this.top; }
            set { this.top = value; }
        }

        internal bool IsPercent {
            get { return this.isPercent; }
            set { this.isPercent = value; }
        }

        internal bool IsDistinct {
            get { return this.isDistinct; }
            set { this.isDistinct = value; }
        }

        internal SqlExpression Selection {
            get { return this.selection; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.selection = value;
            }
        }

        internal SqlRow Row {
            get { return this.row; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.row = value;
            }
        }

        internal SqlSource From {
            get { return this.from; }
            set { this.from = value; }
        }

        internal SqlExpression Where {
            get { return this.where; }
            set {
                if (value != null && TypeSystem.GetNonNullableType(value.ClrType) != typeof(bool)) {
                    throw Error.ArgumentWrongType("value", "bool", value.ClrType);
                }
                this.where = value;
            }
        }

        internal List<SqlExpression> GroupBy {
            get { return this.groupBy; }
        }

        internal SqlExpression Having {
            get { return this.having; }
            set {
                if (value != null && TypeSystem.GetNonNullableType(value.ClrType) != typeof(bool)) {
                    throw Error.ArgumentWrongType("value", "bool", value.ClrType);
                }
                this.having = value;
            }
        }

        internal List<SqlOrderExpression> OrderBy {
            get { return this.orderBy; }
        }

        internal SqlOrderingType OrderingType {
            get { return this.orderingType; }
            set { this.orderingType = value; }
        }

        internal bool DoNotOutput {
            get { return this.squelch; }
            set { this.squelch = value; }
        }
    }

    internal enum SqlOrderingType {
        Default,
        Never,
        Blocked,
        Always
    }

    internal class SqlTable : SqlNode {
        private MetaTable table;
        private MetaType rowType;
        private ProviderType sqlRowType;
        private List<SqlColumn> columns;

        internal SqlTable(MetaTable table, MetaType rowType, ProviderType sqlRowType, Expression sourceExpression)
            : base(SqlNodeType.Table, sourceExpression) {
            this.table = table;
            this.rowType = rowType;
            this.sqlRowType = sqlRowType;
            this.columns = new List<SqlColumn>();
        }

        internal MetaTable MetaTable {
            get { return this.table; }
        }

        internal string Name {
            get { return this.table.TableName; }
        }

        internal List<SqlColumn> Columns {
            get { return this.columns; }
        }

        internal MetaType RowType {
            get { return this.rowType; }
        }

        internal ProviderType SqlRowType {
            get { return this.sqlRowType; }
        }

        internal SqlColumn Find(string columnName) {
            foreach (SqlColumn c in this.Columns) {
                if (c.Name == columnName)
                    return c;
            }
            return null;
        }

    }

    internal class SqlUserQuery : SqlNode {
        private string queryText;
        private SqlExpression projection;
        private List<SqlExpression> args;
        private List<SqlUserColumn> columns;

        internal SqlUserQuery(SqlNodeType nt, SqlExpression projection, IEnumerable<SqlExpression> args, Expression source)
            : base(nt, source) {
            this.Projection = projection;
            this.args = (args != null) ? new List<SqlExpression>(args) : new List<SqlExpression>();
            this.columns = new List<SqlUserColumn>();
        }

        internal SqlUserQuery(string queryText, SqlExpression projection, IEnumerable<SqlExpression> args, Expression source)
            : base(SqlNodeType.UserQuery, source) {
            this.queryText = queryText;
            this.Projection = projection;
            this.args = (args != null) ? new List<SqlExpression>(args) : new List<SqlExpression>();
            this.columns = new List<SqlUserColumn>();
        }

        internal string QueryText {
            get { return this.queryText; }
        }

        internal SqlExpression Projection {
            get { return this.projection; }
            set {
                if (this.projection != null && this.projection.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType("value", this.projection.ClrType, value.ClrType);
                this.projection = value;
            }
        }

        internal List<SqlExpression> Arguments {
            get { return this.args; }
        }

        internal List<SqlUserColumn> Columns {
            get { return this.columns; }
        }

        internal SqlUserColumn Find(string name) {
            foreach (SqlUserColumn c in this.Columns) {
                if (c.Name == name)
                    return c;
            }
            return null;
        }
    }

    internal class SqlStoredProcedureCall : SqlUserQuery {
        private MetaFunction function;

        internal SqlStoredProcedureCall(MetaFunction function, SqlExpression projection, IEnumerable<SqlExpression> args, Expression source)
            : base(SqlNodeType.StoredProcedureCall, projection, args, source) {
            if (function == null)
                throw Error.ArgumentNull("function");
            this.function = function;
        }

        internal MetaFunction Function {
            get { return this.function; }
        }
    }

    internal class SqlUserRow : SqlSimpleTypeExpression {
        private SqlUserQuery query;
        private MetaType rowType;

        internal SqlUserRow(MetaType rowType, ProviderType sqlType, SqlUserQuery query, Expression source)
            : base(SqlNodeType.UserRow, rowType.Type, sqlType, source) {
            this.Query = query;
            this.rowType = rowType;
        }

        internal MetaType RowType {
            get { return this.rowType; }
        }

        internal SqlUserQuery Query {
            get { return this.query; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.Projection != null && value.Projection.ClrType != this.ClrType)
                    throw Error.ArgumentWrongType("value", this.ClrType, value.Projection.ClrType);
                this.query = value;
            }
        }
    }

    internal class SqlUserColumn : SqlSimpleTypeExpression {
        private SqlUserQuery query;
        private string name;
        private bool isRequired;

        internal SqlUserColumn(Type clrType, ProviderType sqlType, SqlUserQuery query, string name, bool isRequired, Expression source)
            : base(SqlNodeType.UserColumn, clrType, sqlType, source) {
            this.Query = query;
            this.name = name;
            this.isRequired = isRequired;
        }

        internal SqlUserQuery Query {
            get { return this.query; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.query != null && this.query != value)
                    throw Error.ArgumentWrongValue("value");
                this.query = value;
            }
        }

        internal string Name {
            get { return this.name; }
        }

        internal bool IsRequired {
            get { return this.isRequired; }
        }
    }

    internal class SqlAlias : SqlSource {
        private string name;
        private SqlNode node;

        internal SqlAlias(SqlNode node)
            : base(SqlNodeType.Alias, node.SourceExpression) {
            this.Node = node;
        }

        internal string Name {
            get { return this.name; }
            set { this.name = value; }
        }

        internal SqlNode Node {
            get { return this.node; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (!(value is SqlExpression || value is SqlSelect || value is SqlTable || value is SqlUnion))
                    throw Error.UnexpectedNode(value.NodeType);
                this.node = value;
            }
        }
    }

    internal class SqlAliasRef : SqlExpression {
        private SqlAlias alias;

        internal SqlAliasRef(SqlAlias alias)
            : base(SqlNodeType.AliasRef, GetClrType(alias.Node), alias.SourceExpression) {
            if (alias == null)
                throw Error.ArgumentNull("alias");
            this.alias = alias;
        }

        internal SqlAlias Alias {
            get { return this.alias; }
        }

        internal override ProviderType SqlType {
            get { return GetSqlType(this.alias.Node); }
        }

        private static Type GetClrType(SqlNode node) {
            SqlTableValuedFunctionCall tvf = node as SqlTableValuedFunctionCall;
            if (tvf != null)
                return tvf.RowType.Type;
            SqlExpression exp = node as SqlExpression;
            if (exp != null) {
                if (TypeSystem.IsSequenceType(exp.ClrType))
                    return TypeSystem.GetElementType(exp.ClrType);
                return exp.ClrType;
            }
            SqlSelect sel = node as SqlSelect;
            if (sel != null)
                return sel.Selection.ClrType;
            SqlTable tab = node as SqlTable;
            if (tab != null)
                return tab.RowType.Type;
            SqlUnion su = node as SqlUnion;
            if (su != null)
                return su.GetClrType();
            throw Error.UnexpectedNode(node.NodeType);
        }

        private static ProviderType GetSqlType(SqlNode node) {
            SqlExpression exp = node as SqlExpression;
            if (exp != null)
                return exp.SqlType;
            SqlSelect sel = node as SqlSelect;
            if (sel != null)
                return sel.Selection.SqlType;
            SqlTable tab = node as SqlTable;
            if (tab != null)
                return tab.SqlRowType;
            SqlUnion su = node as SqlUnion;
            if (su != null)
                return su.GetSqlType();
            throw Error.UnexpectedNode(node.NodeType);
        }
    }

    internal class SqlJoin : SqlSource {
        private SqlJoinType joinType;
        private SqlSource left;
        private SqlSource right;
        private SqlExpression condition;

        internal SqlJoin(SqlJoinType type, SqlSource left, SqlSource right, SqlExpression cond, Expression sourceExpression)
            : base(SqlNodeType.Join, sourceExpression) {
            this.JoinType = type;
            this.Left = left;
            this.Right = right;
            this.Condition = cond;
        }

        internal SqlJoinType JoinType {
            get { return this.joinType; }
            set { this.joinType = value; }
        }

        internal SqlSource Left {
            get { return this.left; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.left = value;
            }
        }

        internal SqlSource Right {
            get { return this.right; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.right = value;
            }
        }

        internal SqlExpression Condition {
            get { return this.condition; }
            set { this.condition = value; }
        }
    }

    internal enum SqlJoinType {
        Cross,
        Inner,
        LeftOuter,
        CrossApply,
        OuterApply
    }

    internal class SqlUnion : SqlNode {
        private SqlNode left;
        private SqlNode right;
        private bool all;

        internal SqlUnion(SqlNode left, SqlNode right, bool all)
            : base(SqlNodeType.Union, right.SourceExpression) {
            this.Left = left;
            this.Right = right;
            this.All = all;
        }

        internal SqlNode Left {
            get { return this.left; }
            set {
                Validate(value);
                this.left = value;
            }
        }

        internal SqlNode Right {
            get { return this.right; }
            set {
                Validate(value);
                this.right = value;
            }
        }

        internal bool All {
            get { return this.all; }
            set { this.all = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        private void Validate(SqlNode node) {
            if (node == null)
                throw Error.ArgumentNull("node");
            if (!(node is SqlExpression || node is SqlSelect || node is SqlUnion))
                throw Error.UnexpectedNode(node.NodeType);
        }

        internal Type GetClrType() {
            SqlExpression exp = this.Left as SqlExpression;
            if (exp != null)
                return exp.ClrType;
            SqlSelect sel = this.Left as SqlSelect;
            if (sel != null)
                return sel.Selection.ClrType;
            throw Error.CouldNotGetClrType();
        }

        internal ProviderType GetSqlType() {
            SqlExpression exp = this.Left as SqlExpression;
            if (exp != null)
                return exp.SqlType;
            SqlSelect sel = this.Left as SqlSelect;
            if (sel != null)
                return sel.Selection.SqlType;
            throw Error.CouldNotGetSqlType();
        }
    }

    internal class SqlNop : SqlSimpleTypeExpression {
        internal SqlNop(Type clrType, ProviderType sqlType, Expression sourceExpression)
            : base(SqlNodeType.Nop, clrType, sqlType, sourceExpression) {
        }
    }

    internal class SqlLift : SqlExpression {
        internal SqlExpression liftedExpression;

        internal SqlLift(Type type, SqlExpression liftedExpression, Expression sourceExpression)
            : base(SqlNodeType.Lift, type, sourceExpression) {
            if (liftedExpression == null)
                throw Error.ArgumentNull("liftedExpression");
            this.liftedExpression = liftedExpression;
        }

        internal SqlExpression Expression {
            get { return this.liftedExpression; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.liftedExpression = value;
            }
        }

        internal override ProviderType SqlType {
            get { return this.liftedExpression.SqlType; }
        }
    }

    internal enum SqlOrderType {
        Ascending,
        Descending
    }

    internal class SqlOrderExpression : IEquatable<SqlOrderExpression> {
        private SqlOrderType orderType;
        private SqlExpression expression;

        internal SqlOrderExpression(SqlOrderType type, SqlExpression expr) {
            this.OrderType = type;
            this.Expression = expr;
        }

        internal SqlOrderType OrderType {
            get { return this.orderType; }
            set { this.orderType = value; }
        }

        internal SqlExpression Expression {
            get { return this.expression; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.expression != null && !this.expression.ClrType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType("value", this.expression.ClrType, value.ClrType);
                this.expression = value;
            }
        }

        public override bool Equals(object obj) {
            if (this.EqualsTo(obj as SqlOrderExpression))
                return true;

            return base.Equals(obj);
        }

        public bool Equals(SqlOrderExpression other) {
            if (this.EqualsTo(other))
                return true;

            return base.Equals(other);
        }

        private bool EqualsTo(SqlOrderExpression other) {
            if (other == null)
                return false;
            if (object.ReferenceEquals(this, other))
                return true;
            if (this.OrderType != other.OrderType)
                return false;
            if (!this.Expression.SqlType.Equals(other.Expression.SqlType))
                return false;

            SqlColumn col1 = SqlOrderExpression.UnwrapColumn(this.Expression);
            SqlColumn col2 = SqlOrderExpression.UnwrapColumn(other.Expression);

            if (col1 == null || col2 == null)
                return false;

            return col1 == col2;
        }

        public override int GetHashCode() {
            SqlColumn col = SqlOrderExpression.UnwrapColumn(this.Expression);
            if (col != null)
                return col.GetHashCode();

            return base.GetHashCode();
        }

        private static SqlColumn UnwrapColumn(SqlExpression expr) {
            System.Diagnostics.Debug.Assert(expr != null);

            SqlUnary exprAsUnary = expr as SqlUnary;
            if (exprAsUnary != null) {
                expr = exprAsUnary.Operand;
            }

            SqlColumn exprAsColumn = expr as SqlColumn;
            if (exprAsColumn != null) {
                return exprAsColumn;
            }

            SqlColumnRef exprAsColumnRef = expr as SqlColumnRef;
            if (exprAsColumnRef != null) {
                return exprAsColumnRef.GetRootColumn();
            }
            //
            // For all other types return null to revert to default behavior for Equals()
            // and GetHashCode()
            //
            return null;
        }
    }

    internal class SqlRowNumber : SqlSimpleTypeExpression {
        private List<SqlOrderExpression> orderBy;

        internal List<SqlOrderExpression> OrderBy {
            get { return orderBy; }
        }

        internal SqlRowNumber(Type clrType, ProviderType sqlType, List<SqlOrderExpression> orderByList, Expression sourceExpression)
            : base(SqlNodeType.RowNumber, clrType, sqlType, sourceExpression) {
            if (orderByList == null) {
                throw Error.ArgumentNull("orderByList");
            }

            this.orderBy = orderByList;
        }
    }

    internal class SqlUnary : SqlSimpleTypeExpression {
        private SqlExpression operand;
        private MethodInfo method;

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal SqlUnary(SqlNodeType nt, Type clrType, ProviderType sqlType, SqlExpression expr, Expression sourceExpression)
            : this(nt, clrType, sqlType, expr, null, sourceExpression) {
        }

        internal SqlUnary(SqlNodeType nt, Type clrType, ProviderType sqlType, SqlExpression expr, MethodInfo method, Expression sourceExpression)
            : base(nt, clrType, sqlType, sourceExpression) {
            switch (nt) {
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.Negate:
                case SqlNodeType.BitNot:
                case SqlNodeType.IsNull:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.Count:
                case SqlNodeType.LongCount:
                case SqlNodeType.Max:
                case SqlNodeType.Min:
                case SqlNodeType.Sum:
                case SqlNodeType.Avg:
                case SqlNodeType.Stddev:
                case SqlNodeType.Convert:
                case SqlNodeType.ValueOf:
                case SqlNodeType.Treat:
                case SqlNodeType.OuterJoinedValue:
                case SqlNodeType.ClrLength:
                    break;
                default:
                    throw Error.UnexpectedNode(nt);
            }
            this.Operand = expr;
            this.method = method;
        }

        internal SqlExpression Operand {
            get { return this.operand; }
            set {
                if (value == null && (this.NodeType != SqlNodeType.Count && this.NodeType != SqlNodeType.LongCount))
                    throw Error.ArgumentNull("value");
                this.operand = value;
            }
        }

        internal MethodInfo Method {
            get { return this.method; }
        }
    }

    internal class SqlBinary : SqlSimpleTypeExpression {
        private SqlExpression left;
        private SqlExpression right;
        private MethodInfo method;

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal SqlBinary(SqlNodeType nt, Type clrType, ProviderType sqlType, SqlExpression left, SqlExpression right)
            : this(nt, clrType, sqlType, left, right, null) {
        }

        internal SqlBinary(SqlNodeType nt, Type clrType, ProviderType sqlType, SqlExpression left, SqlExpression right, MethodInfo method)
            : base(nt, clrType, sqlType, right.SourceExpression) {
            switch (nt) {
                case SqlNodeType.Add:
                case SqlNodeType.Sub:
                case SqlNodeType.Mul:
                case SqlNodeType.Div:
                case SqlNodeType.Mod:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.And:
                case SqlNodeType.Or:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.LE:
                case SqlNodeType.LT:
                case SqlNodeType.EQ:
                case SqlNodeType.NE:
                case SqlNodeType.EQ2V:
                case SqlNodeType.NE2V:
                case SqlNodeType.Concat:
                case SqlNodeType.Coalesce:
                    break;
                default:
                    throw Error.UnexpectedNode(nt);
            }
            this.Left = left;
            this.Right = right;
            this.method = method;
        }

        internal SqlExpression Left {
            get { return this.left; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.left = value;
            }
        }

        internal SqlExpression Right {
            get { return this.right; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.right = value;
            }
        }

        internal MethodInfo Method {
            get { return this.method; }
        }
    }

    internal class SqlBetween : SqlSimpleTypeExpression {
        SqlExpression expression;
        SqlExpression start;
        SqlExpression end;

        internal SqlBetween(Type clrType, ProviderType sqlType, SqlExpression expr, SqlExpression start, SqlExpression end, Expression source)
            : base(SqlNodeType.Between, clrType, sqlType, source) {
            this.expression = expr;
            this.start = start;
            this.end = end;
        }

        internal SqlExpression Expression {
            get { return this.expression; }
            set { this.expression = value; }
        }

        internal SqlExpression Start {
            get { return this.start; }
            set { this.start = value; }
        }

        internal SqlExpression End {
            get { return this.end; }
            set { this.end = value; }
        }
    }

    internal class SqlIn : SqlSimpleTypeExpression {
        private SqlExpression expression;
        private List<SqlExpression> values;

        internal SqlIn(Type clrType, ProviderType sqlType, SqlExpression expression, IEnumerable<SqlExpression> values, Expression sourceExpression)
            :base(SqlNodeType.In, clrType, sqlType, sourceExpression) {
            this.expression = expression;
            this.values = values != null ? new List<SqlExpression>(values) : new List<SqlExpression>(0);
        }

        internal SqlExpression Expression {
            get { return this.expression; }
            set {
                if (value == null) {
                    throw Error.ArgumentNull("value");
                }
                this.expression = value;
            }
        }
        internal List<SqlExpression> Values {
            get { return this.values; }
        }
    }

    internal class SqlLike : SqlSimpleTypeExpression {
        private SqlExpression expression;
        private SqlExpression pattern;
        private SqlExpression escape;

        internal SqlLike(Type clrType, ProviderType sqlType, SqlExpression expr, SqlExpression pattern, SqlExpression escape, Expression source)
            : base(SqlNodeType.Like, clrType, sqlType, source) {
            if (expr == null)
                throw Error.ArgumentNull("expr");
            if (pattern == null)
                throw Error.ArgumentNull("pattern");
            this.Expression = expr;
            this.Pattern = pattern;
            this.Escape = escape;
        }

        internal SqlExpression Expression {
            get { return this.expression; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.ClrType != typeof(string))
                    throw Error.ArgumentWrongType("value", "string", value.ClrType);
                this.expression = value;
            }
        }

        internal SqlExpression Pattern {
            get { return this.pattern; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.ClrType != typeof(string))
                    throw Error.ArgumentWrongType("value", "string", value.ClrType);
                this.pattern = value;
            }
        }

        internal SqlExpression Escape {
            get { return this.escape; }
            set {
                if (value != null && value.ClrType != typeof(string))
                    throw Error.ArgumentWrongType("value", "string", value.ClrType);
                this.escape = value;
            }
        }
    }

    internal class SqlWhen {
        private SqlExpression matchExpression;
        private SqlExpression valueExpression;

        internal SqlWhen(SqlExpression match, SqlExpression value) {
            // 'match' may be null when this when represents the ELSE condition.
            if (value == null)
                throw Error.ArgumentNull("value");
            this.Match = match;
            this.Value = value;
        }

        internal SqlExpression Match {
            get { return this.matchExpression; }
            set {
                if (this.matchExpression != null && value != null && this.matchExpression.ClrType != value.ClrType
                    // Exception: bool types, because predicates can have type bool or bool?
                    && !TypeSystem.GetNonNullableType(this.matchExpression.ClrType).Equals(typeof(bool))
                    && !TypeSystem.GetNonNullableType(value.ClrType).Equals(typeof(bool)))
                    throw Error.ArgumentWrongType("value", this.matchExpression.ClrType, value.ClrType);
                this.matchExpression = value;
            }
        }

        internal SqlExpression Value {
            get { return this.valueExpression; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.valueExpression != null && !this.valueExpression.ClrType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType("value", this.valueExpression.ClrType, value.ClrType);
                this.valueExpression = value;
            }
        }
    }

    /*
     * Searched CASE function:
     * CASE
     * WHEN BooleanExpression THEN resultExpression 
     * [ ...n ] 
     * [ 
     * ELSE elseResultExpression 
     * ] 
     * END
     */
    internal class SqlSearchedCase : SqlExpression {
        private List<SqlWhen> whens;
        private SqlExpression @else;

        internal SqlSearchedCase(Type clrType, IEnumerable<SqlWhen> whens, SqlExpression @else, Expression sourceExpression)
            : base(SqlNodeType.SearchedCase, clrType, sourceExpression) {
            if (whens == null)
                throw Error.ArgumentNull("whens");
            this.whens = new List<SqlWhen>(whens);
            if (this.whens.Count == 0)
                throw Error.ArgumentOutOfRange("whens");
            this.Else = @else;
        }

        internal List<SqlWhen> Whens {
            get { return this.whens; }
        }

        internal SqlExpression Else {
            get { return this.@else; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.@else != null && !this.@else.ClrType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType("value", this.@else.ClrType, value.ClrType);
                this.@else = value;
            }
        }

        internal override ProviderType SqlType {
            get { return this.whens[0].Value.SqlType; }
        }
    }

    /*
     * Simple CASE function:
     * CASE inputExpression 
     * WHEN whenExpression THEN resultExpression 
     * [ ...n ] 
     * [ 
     * ELSE elseResultExpression 
     * ] 
     * END 
     */
    internal class SqlSimpleCase : SqlExpression {
        private SqlExpression expression;
        private List<SqlWhen> whens = new List<SqlWhen>();

        internal SqlSimpleCase(Type clrType, SqlExpression expr, IEnumerable<SqlWhen> whens, Expression sourceExpression)
            : base(SqlNodeType.SimpleCase, clrType, sourceExpression) {
            this.Expression = expr;
            if (whens == null)
                throw Error.ArgumentNull("whens");
            this.whens.AddRange(whens);
            if (this.whens.Count == 0)
                throw Error.ArgumentOutOfRange("whens");
        }

        internal SqlExpression Expression {
            get { return this.expression; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.expression != null && this.expression.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType("value", this.expression.ClrType, value.ClrType);
                this.expression = value;
            }
        }

        internal List<SqlWhen> Whens {
            get { return this.whens; }
        }

        internal override ProviderType SqlType {
            get { return this.whens[0].Value.SqlType; }
        }
    }

    /// <summary>
    /// A case statement that must be evaluated on the client. For example, a case statement
    /// that contains values of LINK, Element, or Multi-set are not directly handleable by 
    /// SQL.
    /// 
    /// CASE inputExpression 
    /// WHEN whenExpression THEN resultExpression 
    /// [ ...n ] 
    /// END 
    /// </summary>
    internal class SqlClientCase : SqlExpression {
        private SqlExpression expression;
        private List<SqlClientWhen> whens = new List<SqlClientWhen>();

        internal SqlClientCase(Type clrType, SqlExpression expr, IEnumerable<SqlClientWhen> whens, Expression sourceExpression)
            : base(SqlNodeType.ClientCase, clrType, sourceExpression) {
            this.Expression = expr;
            if (whens == null)
                throw Error.ArgumentNull("whens");
            this.whens.AddRange(whens);
            if (this.whens.Count == 0)
                throw Error.ArgumentOutOfRange("whens");
        }

        internal SqlExpression Expression {
            get { return this.expression; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.expression != null && this.expression.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType("value", this.expression.ClrType, value.ClrType);
                this.expression = value;
            }
        }

        internal List<SqlClientWhen> Whens {
            get { return this.whens; }
        }

        internal override ProviderType SqlType {
            get { return this.whens[0].Value.SqlType; }
        }
    }

    /// <summary>
    /// A single WHEN clause for ClientCase.
    /// </summary>
    internal class SqlClientWhen {
        private SqlExpression matchExpression;
        private SqlExpression matchValue;

        internal SqlClientWhen(SqlExpression match, SqlExpression value) {
            // 'match' may be null when this when represents the ELSE condition.
            if (value == null)
                throw Error.ArgumentNull("value");
            this.Match = match;
            this.Value = value;
        }

        internal SqlExpression Match {
            get { return this.matchExpression; }
            set {
                if (this.matchExpression != null && value != null && this.matchExpression.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType("value", this.matchExpression.ClrType, value.ClrType);
                this.matchExpression = value;
            }
        }

        internal SqlExpression Value {
            get { return this.matchValue; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.matchValue != null && this.matchValue.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType("value", this.matchValue.ClrType, value.ClrType);
                this.matchValue = value;
            }
        }
    }

    /// <summary>
    /// Represents the construction of an object in abstract 'super sql'.
    /// The type may be polymorphic. A discriminator field is used to determine 
    /// which type in a hierarchy should be instantiated.
    /// In the common degenerate case where the inheritance hierarchy is 1-deep 
    /// the discriminator will be a constant SqlValue and there will be one 
    /// type-case-when corresponding to that type.
    /// </summary>
    internal class SqlTypeCase : SqlExpression {
        private MetaType rowType;
        private SqlExpression discriminator;
        private List<SqlTypeCaseWhen> whens = new List<SqlTypeCaseWhen>();
        ProviderType sqlType;

        internal SqlTypeCase(Type clrType, ProviderType sqlType, MetaType rowType, SqlExpression discriminator, IEnumerable<SqlTypeCaseWhen> whens, Expression sourceExpression)
            : base(SqlNodeType.TypeCase, clrType, sourceExpression) {
            this.Discriminator = discriminator;
            if (whens == null)
                throw Error.ArgumentNull("whens");
            this.whens.AddRange(whens);
            if (this.whens.Count == 0)
                throw Error.ArgumentOutOfRange("whens");
            this.sqlType = sqlType;
            this.rowType = rowType;
        }

        internal SqlExpression Discriminator {
            get { return this.discriminator; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.discriminator != null && this.discriminator.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType("value", this.discriminator.ClrType, value.ClrType);
                this.discriminator = value;
            }
        }

        internal List<SqlTypeCaseWhen> Whens {
            get { return this.whens; }
        }

        internal override ProviderType SqlType {
            get { return sqlType; }
        }

        internal MetaType RowType {
            get { return this.rowType; }
        }
    }

    /// <summary>
    /// Represents one choice of object instantiation type in a type case.
    /// When 'match' is the same as type case Discriminator then the corresponding
    /// type binding is the one used for instantiation.
    /// </summary>
    internal class SqlTypeCaseWhen {
        private SqlExpression match;
        private SqlExpression @new;

        internal SqlTypeCaseWhen(SqlExpression match, SqlExpression typeBinding) {
            this.Match = match;
            this.TypeBinding = typeBinding;
        }
        internal SqlExpression Match {
            get { return this.match; }
            set {
                if (this.match != null && value != null && this.match.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType("value", this.match.ClrType, value.ClrType);
                this.match = value;
            }
        }
        internal SqlExpression TypeBinding {
            get { return this.@new; }
            set { this.@new = value; }
        }
    }

    internal class SqlValue : SqlSimpleTypeExpression {
        private object value;
        private bool isClient;

        internal SqlValue(Type clrType, ProviderType sqlType, object value, bool isClientSpecified, Expression sourceExpression)
            : base(SqlNodeType.Value, clrType, sqlType, sourceExpression) {
            this.value = value;
            this.isClient = isClientSpecified;
        }

        internal object Value {
            get { return this.value; }
        }

        internal bool IsClientSpecified {
            get { return this.isClient; }
        }
    }

    internal class SqlParameter : SqlSimpleTypeExpression {
        private string name;
        private System.Data.ParameterDirection direction;

        internal SqlParameter(Type clrType, ProviderType sqlType, string name, Expression sourceExpression)
            : base(SqlNodeType.Parameter, clrType, sqlType, sourceExpression) {
            if (name == null)
                throw Error.ArgumentNull("name");
            if (typeof(Type).IsAssignableFrom(clrType))
                throw Error.ArgumentWrongValue("clrType");
            this.name = name;
            this.direction = System.Data.ParameterDirection.Input;
        }

        internal string Name {
            get { return this.name; }
        }

        internal System.Data.ParameterDirection Direction {
            get { return this.direction; }
            set { this.direction = value; }
        }
    }

    internal class SqlVariable : SqlSimpleTypeExpression {
        private string name;

        internal SqlVariable(Type clrType, ProviderType sqlType, string name, Expression sourceExpression)
            : base(SqlNodeType.Variable, clrType, sqlType, sourceExpression) {
            if (name == null)
                throw Error.ArgumentNull("name");
            this.name = name;
        }

        internal string Name {
            get { return this.name; }
        }
    }

    internal class SqlMember : SqlSimpleTypeExpression {
        private SqlExpression expression;
        private MemberInfo member;

        internal SqlMember(Type clrType, ProviderType sqlType, SqlExpression expr, MemberInfo member)
            : base(SqlNodeType.Member, clrType, sqlType, expr.SourceExpression) {
            this.member = member;
            this.Expression = expr;
        }

        internal MemberInfo Member {
            get { return this.member; }
        }

        internal SqlExpression Expression {
            get {
                return this.expression;
            }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (!this.member.ReflectedType.IsAssignableFrom(value.ClrType) &&
                    !value.ClrType.IsAssignableFrom(this.member.ReflectedType))
                    throw Error.MemberAccessIllegal(this.member, this.member.ReflectedType, value.ClrType);
                this.expression = value;
            }
        }
    }

    internal class SqlColumn : SqlExpression {
        private SqlAlias alias;
        private string name;
        private int ordinal;
        private MetaDataMember member;
        private SqlExpression expression;
        private ProviderType sqlType;

        internal SqlColumn(Type clrType, ProviderType sqlType, string name, MetaDataMember member, SqlExpression expr, Expression sourceExpression)
            : base(SqlNodeType.Column, clrType, sourceExpression) {
            if (typeof(Type).IsAssignableFrom(clrType))
                throw Error.ArgumentWrongValue("clrType");
            this.Name = name;
            this.member = member;
            this.Expression = expr;
            this.Ordinal = -1;
            if (sqlType == null)
                throw Error.ArgumentNull("sqlType");
            this.sqlType = sqlType;
            System.Diagnostics.Debug.Assert(sqlType.CanBeColumn);
        }

        internal SqlColumn(string name, SqlExpression expr)
            : this(expr.ClrType, expr.SqlType, name, null, expr, expr.SourceExpression) {
            System.Diagnostics.Debug.Assert(expr != null);
        }

        internal SqlAlias Alias {
            get { return this.alias; }
            set { this.alias = value; }
        }

        internal string Name {
            get { return this.name; }
            set { this.name = value; }
        }

        internal int Ordinal {
            get { return this.ordinal; }
            set { this.ordinal = value; }
        }

        internal MetaDataMember MetaMember {
            get { return this.member; }
        }

        /// <summary>
        /// Set the column's Expression. This can change the type of the column.
        /// </summary>
        internal SqlExpression Expression {
            get {
                return this.expression;
            }
            set {
                if (value != null) {
                    if (!this.ClrType.IsAssignableFrom(value.ClrType))
                        throw Error.ArgumentWrongType("value", this.ClrType, value.ClrType);
                    SqlColumnRef cref = value as SqlColumnRef;
                    if (cref != null && cref.Column == this)
                        throw Error.ColumnCannotReferToItself();
                }
                this.expression = value;
            }
        }

        internal override ProviderType SqlType {
            get {
                if (this.expression != null)
                    return this.expression.SqlType;
                return this.sqlType;
            }
        }
    }

    internal class SqlColumnRef : SqlExpression {
        private SqlColumn column;
        internal SqlColumnRef(SqlColumn col)
            : base(SqlNodeType.ColumnRef, col.ClrType, col.SourceExpression) {
            this.column = col;
        }

        internal SqlColumn Column {
            get { return this.column; }
        }

        internal override ProviderType SqlType {
            get { return this.column.SqlType; }
        }

        public override bool Equals(object obj) {
            SqlColumnRef cref = obj as SqlColumnRef;
            return cref != null && cref.Column == this.column;
        }

        public override int GetHashCode() {
            return this.column.GetHashCode();
        }

        internal SqlColumn GetRootColumn() {
            SqlColumn c = this.column;
            while (c.Expression != null && c.Expression.NodeType == SqlNodeType.ColumnRef) {
                c = ((SqlColumnRef)c.Expression).Column;
            }
            return c;
        }
    }

    internal class SqlRow : SqlNode {
        private List<SqlColumn> columns;

        internal SqlRow(Expression sourceExpression)
            : base(SqlNodeType.Row, sourceExpression) {
            this.columns = new List<SqlColumn>();
        }

        internal List<SqlColumn> Columns {
            get { return this.columns; }
        }

        internal SqlColumn Find(string name) {
            foreach (SqlColumn c in this.columns) {
                if (name == c.Name)
                    return c;
            }
            return null;
        }
    }

    internal class SqlMemberAssign : SqlNode {
        private MemberInfo member;
        private SqlExpression expression;

        internal SqlMemberAssign(MemberInfo member, SqlExpression expr)
            : base(SqlNodeType.MemberAssign, expr.SourceExpression) {
            if (member == null)
                throw Error.ArgumentNull("member");
            this.member = member;
            this.Expression = expr;
        }

        internal MemberInfo Member {
            get { return this.member; }
        }

        internal SqlExpression Expression {
            get { return this.expression; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.expression = value;
            }
        }
    }

    internal class SqlGrouping : SqlSimpleTypeExpression {
        private SqlExpression key;
        private SqlExpression group;

        internal SqlGrouping(Type clrType, ProviderType sqlType, SqlExpression key, SqlExpression group, Expression sourceExpression)
            : base(SqlNodeType.Grouping, clrType, sqlType, sourceExpression) {
            if (key == null) throw Error.ArgumentNull("key");
            if (group == null) throw Error.ArgumentNull("group");
            this.key = key;
            this.group = group;
        }

        internal SqlExpression Key {
            get { return this.key; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (!this.key.ClrType.IsAssignableFrom(value.ClrType)
                    && !value.ClrType.IsAssignableFrom(this.key.ClrType))
                    throw Error.ArgumentWrongType("value", this.key.ClrType, value.ClrType);
                this.key = value;
            }
        }

        internal SqlExpression Group {
            get { return this.group; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.ClrType != this.group.ClrType)
                    throw Error.ArgumentWrongType("value", this.group.ClrType, value.ClrType);
                this.group = value;
            }
        }
    }

    internal class SqlNew : SqlSimpleTypeExpression {
        private MetaType metaType;
        private ConstructorInfo constructor;
        private List<SqlExpression> args;
        private List<MemberInfo> argMembers;
        private List<SqlMemberAssign> members;

        internal SqlNew(MetaType metaType, ProviderType sqlType, ConstructorInfo cons, IEnumerable<SqlExpression> args, IEnumerable<MemberInfo> argMembers, IEnumerable<SqlMemberAssign> members, Expression sourceExpression)
            : base(SqlNodeType.New, metaType.Type, sqlType, sourceExpression) {
            this.metaType = metaType;
            
            if (cons == null && metaType.Type.IsClass) { // structs do not need to have a constructor
                throw Error.ArgumentNull("cons");
            }
            this.constructor = cons;
            this.args = new List<SqlExpression>();
            this.argMembers = new List<MemberInfo>();
            this.members = new List<SqlMemberAssign>();
            if (args != null) {
                this.args.AddRange(args);
            }
            if (argMembers != null) {
                this.argMembers.AddRange(argMembers);
            }
            if (members != null) {
                this.members.AddRange(members);
            }
        }

        internal MetaType MetaType {
            get { return this.metaType; }
        }

        internal ConstructorInfo Constructor {
            get { return this.constructor; }
        }

        internal List<SqlExpression> Args {
            get { return this.args; }
        }

        internal List<MemberInfo> ArgMembers {
            get { return this.argMembers; }
        }

        internal List<SqlMemberAssign> Members {
            get { return this.members; }
        }

        internal SqlExpression Find(MemberInfo mi) {
            for (int i = 0, n = this.argMembers.Count; i < n; i++) {
                MemberInfo argmi = this.argMembers[i];
                if (argmi.Name == mi.Name) {
                    return this.args[i];
                }
            }

            foreach (SqlMemberAssign ma in this.Members) {
                if (ma.Member.Name == mi.Name) {
                    return ma.Expression;
                }
            }

            return null;
        }
    }

    internal class SqlMethodCall : SqlSimpleTypeExpression {
        private MethodInfo method;
        private SqlExpression obj;
        private List<SqlExpression> arguments;

        internal SqlMethodCall(Type clrType, ProviderType sqlType, MethodInfo method, SqlExpression obj, IEnumerable<SqlExpression> args, Expression sourceExpression)
            : base(SqlNodeType.MethodCall, clrType, sqlType, sourceExpression) {
            if (method == null)
                throw Error.ArgumentNull("method");
            this.method = method;
            this.Object = obj;
            this.arguments = new List<SqlExpression>();
            if (args != null)
                this.arguments.AddRange(args);
        }

        internal MethodInfo Method {
            get { return this.method; }
        }

        internal SqlExpression Object {
            get { return this.obj; }
            set {
                if (value == null && !this.method.IsStatic)
                    throw Error.ArgumentNull("value");
                if (value != null && !this.method.DeclaringType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType("value", this.method.DeclaringType, value.ClrType);
                this.obj = value;
            }
        }

        internal List<SqlExpression> Arguments {
            get { return this.arguments; }
        }
    }

    internal class SqlIncludeScope : SqlNode {
        SqlNode child;
        internal SqlIncludeScope(SqlNode child, Expression sourceExpression) 
            : base(SqlNodeType.IncludeScope, sourceExpression) { 
            this.child = child;
        }
        internal SqlNode Child {
            get {return this.child;}
            set {this.child = value;}
        }
    }

    internal class SqlClientArray : SqlSimpleTypeExpression {
        private List<SqlExpression> expressions;

        internal SqlClientArray(Type clrType, ProviderType sqlType, SqlExpression[ ] exprs, Expression sourceExpression)
            : base(SqlNodeType.ClientArray, clrType, sqlType, sourceExpression) {
            this.expressions = new List<SqlExpression>();
            if (exprs != null)
                this.Expressions.AddRange(exprs);
        }

        internal List<SqlExpression> Expressions {
            get { return this.expressions; }
        }
    }

    internal class SqlLink : SqlSimpleTypeExpression {
        private MetaType rowType;
        private SqlExpression expression;
        private MetaDataMember member;
        private List<SqlExpression> keyExpressions;
        private SqlExpression expansion;
        private object id;

        internal SqlLink(object id, MetaType rowType, Type clrType, ProviderType sqlType, SqlExpression expression, MetaDataMember member, IEnumerable<SqlExpression> keyExpressions, SqlExpression expansion, Expression sourceExpression)
            : base(SqlNodeType.Link, clrType, sqlType, sourceExpression) {
            this.id = id;
            this.rowType = rowType;
            this.expansion = expansion;
            this.expression = expression;
            this.member = member;
            this.keyExpressions = new List<SqlExpression>();
            if (keyExpressions != null)
                this.keyExpressions.AddRange(keyExpressions);
        }

        internal MetaType RowType {
            get { return this.rowType; }
        }

        internal SqlExpression Expansion {
            get { return this.expansion; }
            set { this.expansion = value; }
        }


        internal SqlExpression Expression {
            get { return this.expression; }
            set { this.expression = value; }
        }

        internal MetaDataMember Member {
            get { return this.member; }
        }

        internal List<SqlExpression> KeyExpressions {
            get { return this.keyExpressions; }
        }

        internal object Id {
            get { return this.id; }
        }
    }

    internal class SqlExprSet : SqlExpression {
        private List<SqlExpression> expressions;

        internal SqlExprSet(Type clrType, IEnumerable <SqlExpression> exprs, Expression sourceExpression)
            : base(SqlNodeType.ExprSet, clrType, sourceExpression) {
            this.expressions = new List<SqlExpression>(exprs);
        }

        internal List<SqlExpression> Expressions {
            get { return this.expressions; }
        }

        /// <summary>
        /// Get the first non-set expression of the set by drilling
        /// down the left expressions.
        /// </summary>
        internal SqlExpression GetFirstExpression() {
            SqlExpression expr = expressions[0];
            while (expr is SqlExprSet) {
                expr = ((SqlExprSet)expr).Expressions[0];
            }
            return expr;
        }

        internal override ProviderType SqlType {
            get { return this.expressions[0].SqlType; }
        }
    }

    internal class SqlSubSelect : SqlSimpleTypeExpression {
        private SqlSelect select;

        internal SqlSubSelect(SqlNodeType nt , Type clrType, ProviderType sqlType , SqlSelect select)
            : base(nt, clrType, sqlType, select.SourceExpression) {
            switch (nt) {
                case SqlNodeType.Multiset:
                case SqlNodeType.ScalarSubSelect:
                case SqlNodeType.Element:
                case SqlNodeType.Exists:
                    break;
                default:
                    throw Error.UnexpectedNode(nt);
            }
            this.Select = select;
        }

        internal SqlSelect Select {
            get { return this.select; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.select = value;
            }
        }
    }

    internal class SqlClientQuery : SqlSimpleTypeExpression {
        private SqlSubSelect query;
        private List<SqlExpression> arguments;
        private List<SqlParameter> parameters;
        int ordinal;

        internal SqlClientQuery(SqlSubSelect subquery)
            : base(SqlNodeType.ClientQuery, subquery.ClrType, subquery.SqlType, subquery.SourceExpression) {
            this.query = subquery;
            this.arguments = new List<SqlExpression>();
            this.parameters = new List<SqlParameter>();
        }

        internal SqlSubSelect Query {
            get { return this.query; }
            set {
                if (value == null || (this.query != null && this.query.ClrType != value.ClrType))
                    throw Error.ArgumentWrongType(value, this.query.ClrType, value.ClrType);
                this.query = value;
            }
        }

        internal List<SqlExpression> Arguments {
            get { return this.arguments; }
        }

        internal List<SqlParameter> Parameters {
            get { return this.parameters; }
        }

        internal int Ordinal {
            get { return this.ordinal; }
            set { this.ordinal = value; }
        }
    }

    internal class SqlJoinedCollection : SqlSimpleTypeExpression {
        private SqlExpression expression;
        private SqlExpression count;

        internal SqlJoinedCollection(Type clrType, ProviderType sqlType, SqlExpression expression, SqlExpression count, Expression sourceExpression)
            : base(SqlNodeType.JoinedCollection, clrType, sqlType, sourceExpression) {
            this.expression = expression;
            this.count = count;
        }

        internal SqlExpression Expression {
            get { return this.expression; }
            set {
                if (value == null || this.expression != null && this.expression.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType(value, this.expression.ClrType, value.ClrType);
                this.expression = value;
            }
        }

        internal SqlExpression Count {
            get { return this.count; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.ClrType != typeof(int))
                    throw Error.ArgumentWrongType(value, typeof(int), value.ClrType);
                this.count = value;
            }
        }
    }

    internal class SqlUpdate : SqlStatement {
        private SqlSelect select;
        private List<SqlAssign> assignments;

        internal SqlUpdate(SqlSelect select, IEnumerable<SqlAssign> assignments, Expression sourceExpression)
            : base(SqlNodeType.Update, sourceExpression) {
            this.Select = select;
            this.assignments = new List<SqlAssign>(assignments);
        }

        internal SqlSelect Select {
            get { return this.select; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.select = value;
            }
        }

        internal List<SqlAssign> Assignments {
            get { return this.assignments; }
        }
    }

    internal class SqlInsert : SqlStatement {
        private SqlTable table;
        private SqlRow row;
        private SqlExpression expression;
        private SqlColumn outputKey;
        private bool outputToLocal;

        internal SqlInsert(SqlTable table, SqlExpression expr, Expression sourceExpression)
            : base(SqlNodeType.Insert, sourceExpression) {
            this.Table = table;
            this.Expression = expr;
            this.Row = new SqlRow(sourceExpression);
        }

        internal SqlTable Table {
            get { return this.table; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("null");
                this.table = value;
            }
        }

        internal SqlRow Row {
            get { return this.row; }
            set { this.row = value; }
        }

        internal SqlExpression Expression {
            get { return this.expression; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("null");
                if (!this.table.RowType.Type.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType("value", this.table.RowType, value.ClrType);
                this.expression = value;
            }
        }

        internal SqlColumn OutputKey {
            get { return this.outputKey; }
            set { this.outputKey = value; }
        }

        internal bool OutputToLocal {
            get { return this.outputToLocal; }
            set { this.outputToLocal = value; }
        }
    }

    internal class SqlDelete : SqlStatement {
        private SqlSelect select;

        internal SqlDelete(SqlSelect select, Expression sourceExpression)
            : base(SqlNodeType.Delete, sourceExpression) {
            this.Select = select;
        }

        internal SqlSelect Select {
            get { return this.select; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.select = value;
            }
        }
    }

    internal class SqlBlock : SqlStatement {
        private List<SqlStatement> statements;

        internal SqlBlock(Expression sourceExpression)
            : base(SqlNodeType.Block, sourceExpression) {
            this.statements = new List<SqlStatement>();
        }

        internal List<SqlStatement> Statements {
            get { return this.statements; }
        }
    }

    internal class SqlAssign : SqlStatement {
        private SqlExpression leftValue;
        private SqlExpression rightValue;

        internal SqlAssign(SqlExpression lValue, SqlExpression rValue, Expression sourceExpression)
            : base(SqlNodeType.Assign, sourceExpression) {
            this.LValue = lValue;
            this.RValue = rValue;
        }

        internal SqlExpression LValue {
            get { return this.leftValue; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.rightValue != null && !value.ClrType.IsAssignableFrom(this.rightValue.ClrType))
                    throw Error.ArgumentWrongType("value", this.rightValue.ClrType, value.ClrType);
                this.leftValue = value;
            }
        }

        internal SqlExpression RValue {
            get { return this.rightValue; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.leftValue != null && !this.leftValue.ClrType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType("value", this.leftValue.ClrType, value.ClrType);
                this.rightValue = value;
            }
        }
    }

    internal class SqlDoNotVisitExpression : SqlExpression {
        private SqlExpression expression;

        internal SqlDoNotVisitExpression(SqlExpression expr)
            : base(SqlNodeType.DoNotVisit, expr.ClrType, expr.SourceExpression) {
            if (expr == null)
                throw Error.ArgumentNull("expr");
            this.expression = expr;
        }

        internal SqlExpression Expression {
            get { return this.expression; }
        }

        internal override ProviderType SqlType {
            get { return this.expression.SqlType; }
        }
    }

    internal class SqlOptionalValue : SqlSimpleTypeExpression {
        private SqlExpression hasValue;
        private SqlExpression expressionValue;

        internal SqlOptionalValue( SqlExpression hasValue, SqlExpression value)
            : base(SqlNodeType.OptionalValue, value.ClrType, value.SqlType, value.SourceExpression) {
            this.HasValue = hasValue;
            this.Value = value;
        }

        internal SqlExpression HasValue {
            get { return this.hasValue; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.hasValue = value;
            }
        }

        internal SqlExpression Value {
            get { return this.expressionValue; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.ClrType != this.ClrType)
                    throw Error.ArgumentWrongType("value", this.ClrType, value.ClrType);
                this.expressionValue = value;
            }
        }
    }

    internal class SqlFunctionCall : SqlSimpleTypeExpression {
        private string name;
        private List<SqlExpression> arguments;

        internal SqlFunctionCall(Type clrType, ProviderType sqlType, string name, IEnumerable <SqlExpression > args , Expression source)
            : this(SqlNodeType.FunctionCall, clrType , sqlType, name, args, source) {
        }

        internal SqlFunctionCall(SqlNodeType nodeType, Type clrType, ProviderType sqlType, string name , IEnumerable <SqlExpression> args , Expression source)
            : base(nodeType, clrType, sqlType, source) {
            this.name = name;
            this.arguments = new List<SqlExpression>(args);
        }

        internal string Name {
            get { return this.name; }
        }

        internal List<SqlExpression> Arguments {
            get { return this.arguments; }
        }
    }

    /// <summary>
    /// This class is used to represent a table value function.  It inherits normal function
    /// call functionality, and adds TVF specific members.
    /// </summary>
    internal class SqlTableValuedFunctionCall : SqlFunctionCall {
        private MetaType rowType;
        private List<SqlColumn> columns;

        internal SqlTableValuedFunctionCall(MetaType rowType, Type clrType, ProviderType sqlType, string name, IEnumerable <SqlExpression > args , Expression source)
            : base(SqlNodeType.TableValuedFunctionCall, clrType , sqlType, name, args, source) {
            this.rowType = rowType;
            this.columns = new List<SqlColumn>();
        }

        internal MetaType RowType {
            get { return this.rowType; }
        }

        internal List<SqlColumn> Columns {
            get { return this.columns; }
        }

        internal SqlColumn Find(string name) {
            foreach (SqlColumn c in this.Columns) {
                if (c.Name == name)
                    return c;
            }
            return null;
        }

    }

    internal class SqlSharedExpression : SqlExpression {
        private SqlExpression expr;

        internal SqlSharedExpression(SqlExpression expr)
          : base(SqlNodeType.SharedExpression, expr.ClrType, expr.SourceExpression) {
            this.expr = expr;
        }

        internal SqlExpression Expression {
            get { return this.expr; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (!this.ClrType.IsAssignableFrom(value.ClrType)
                    && !value.ClrType.IsAssignableFrom(this.ClrType))
                    throw Error.ArgumentWrongType("value", this.ClrType, value.ClrType);
                this.expr = value;
            }
        }

        internal override ProviderType SqlType {
            get { return this.expr.SqlType; }
        }
    }

    internal class SqlSharedExpressionRef : SqlExpression {
        private SqlSharedExpression expr;

        internal SqlSharedExpressionRef(SqlSharedExpression expr)
            : base(SqlNodeType.SharedExpressionRef, expr.ClrType, expr.SourceExpression) {
            this.expr = expr;
        }

        internal SqlSharedExpression SharedExpression {
            get { return this.expr; }
        }

        internal override ProviderType SqlType {
            get { return this.expr.SqlType; }
        }
    }

    internal class SqlSimpleExpression : SqlExpression {
        private SqlExpression expr;

        internal SqlSimpleExpression(SqlExpression expr)
            : base(SqlNodeType.SimpleExpression, expr.ClrType, expr.SourceExpression) {
            this.expr = expr;
        }

        internal SqlExpression Expression {
            get { return this.expr; }
            set {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (!TypeSystem.GetNonNullableType(this.ClrType).IsAssignableFrom(TypeSystem.GetNonNullableType(value.ClrType)))
                    throw Error.ArgumentWrongType("value", this.ClrType, value.ClrType);
                this.expr = value;
            }
        }

        internal override ProviderType SqlType {
            get { return this.expr.SqlType; }
        }
    }

    internal class SqlClientParameter : SqlSimpleTypeExpression {
        // Expression<Func<object[], T>>
        LambdaExpression accessor;
        internal SqlClientParameter(Type clrType, ProviderType sqlType, LambdaExpression accessor, Expression sourceExpression):
            base(SqlNodeType.ClientParameter, clrType, sqlType, sourceExpression) {
            this.accessor = accessor;
        }
        internal LambdaExpression Accessor {
            get { return this.accessor; }
        }
    }
}
