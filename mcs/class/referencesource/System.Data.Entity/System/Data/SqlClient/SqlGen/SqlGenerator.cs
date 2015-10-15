//---------------------------------------------------------------------
// <copyright file="SqlGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.SqlClient.SqlGen
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder.Spatial;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Data.Spatial;
    using System.Data.SqlClient;
    using System.Data.SqlClient.Internal;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Translates the command object into a SQL string that can be executed on
    /// SQL Server 2000 and SQL Server 2005.
    /// </summary>
    /// <remarks>
    /// The translation is implemented as a visitor <see cref="DbExpressionVisitor{T}"/>
    /// over the query tree.  It makes a single pass over the tree, collecting the sql
    /// fragments for the various nodes in the tree <see cref="ISqlFragment"/>.
    ///
    /// The major operations are
    /// <list type="bullet">
    /// <item>Select statement minimization.  Multiple nodes in the query tree
    /// that can be part of a single SQL select statement are merged. e.g. a
    /// Filter node that is the input of a Project node can typically share the
    /// same SQL statement.</item>
    /// <item>Alpha-renaming.  As a result of the statement minimization above, there
    /// could be name collisions when using correlated subqueries
    /// <example>
    /// <code>
    /// Filter(
    ///     b = Project( c.x
    ///         c = Extent(foo)
    ///         )
    ///     exists (
    ///         Filter(
    ///             c = Extent(foo)
    ///             b.x = c.x
    ///             )
    ///     )
    /// )
    /// </code>
    /// The first Filter, Project and Extent will share the same SQL select statement.
    /// The alias for the Project i.e. b, will be replaced with c.
    /// If the alias c for the Filter within the exists clause is not renamed,
    /// we will get <c>c.x = c.x</c>, which is incorrect.
    /// Instead, the alias c within the second filter should be renamed to c1, to give
    /// <c>c.x = c1.x</c> i.e. b is renamed to c, and c is renamed to c1.
    /// </example>
    /// </item>
    /// <item>Join flattening.  In the query tree, a list of join nodes is typically
    /// represented as a tree of Join nodes, each with 2 children. e.g.
    /// <example>
    /// <code>
    /// a = Join(InnerJoin
    ///     b = Join(CrossJoin
    ///         c = Extent(foo)
    ///         d = Extent(foo)
    ///         )
    ///     e = Extent(foo)
    ///     on b.c.x = e.x
    ///     )
    /// </code>
    /// If translated directly, this will be translated to
    /// <code>
    /// FROM ( SELECT c.*, d.*
    ///         FROM foo as c
    ///         CROSS JOIN foo as d) as b
    /// INNER JOIN foo as e on b.x' = e.x
    /// </code>
    /// It would be better to translate this as
    /// <code>
    /// FROM foo as c
    /// CROSS JOIN foo as d
    /// INNER JOIN foo as e on c.x = e.x
    /// </code>
    /// This allows the optimizer to choose an appropriate join ordering for evaluation.
    /// </example>
    /// </item>
    /// <item>Select * and column renaming.  In the example above, we noticed that
    /// in some cases we add <c>SELECT * FROM ...</c> to complete the SQL
    /// statement. i.e. there is no explicit PROJECT list.
    /// In this case, we enumerate all the columns available in the FROM clause
    /// This is particularly problematic in the case of Join trees, since the columns
    /// from the extents joined might have the same name - this is illegal.  To solve
    /// this problem, we will have to rename columns if they are part of a SELECT *
    /// for a JOIN node - we do not need renaming in any other situation.
    /// <see cref="SqlGenerator.AddDefaultColumns"/>.
    /// </item>
    /// </list>
    ///
    /// <para>
    /// Renaming issues.
    /// When rows or columns are renamed, we produce names that are unique globally
    /// with respect to the query.  The names are derived from the original names,
    /// with an integer as a suffix. e.g. CustomerId will be renamed to CustomerId1,
    /// CustomerId2 etc.
    ///
    /// Since the names generated are globally unique, they will not conflict when the
    /// columns of a JOIN SELECT statement are joined with another JOIN. 
    ///
    /// </para>
    ///
    /// <para>
    /// Record flattening.
    /// SQL server does not have the concept of records.  However, a join statement
    /// produces records.  We have to flatten the record accesses into a simple
    /// <c>alias.column</c> form.  <see cref="SqlGenerator.Visit(DbPropertyExpression)"/>
    /// </para>
    ///
    /// <para>
    /// Building the SQL.
    /// There are 2 phases
    /// <list type="numbered">
    /// <item>Traverse the tree, producing a sql builder <see cref="SqlBuilder"/></item>
    /// <item>Write the SqlBuilder into a string, renaming the aliases and columns
    /// as needed.</item>
    /// </list>
    ///
    /// In the first phase, we traverse the tree.  We cannot generate the SQL string
    /// right away, since
    /// <list type="bullet">
    /// <item>The WHERE clause has to be visited before the from clause.</item>
    /// <item>extent aliases and column aliases need to be renamed.  To minimize
    /// renaming collisions, all the names used must be known, before any renaming
    /// choice is made.</item>
    /// </list>
    /// To defer the renaming choices, we use symbols <see cref="Symbol"/>.  These
    /// are renamed in the second phase.
    ///
    /// Since visitor methods cannot transfer information to child nodes through
    /// parameters, we use some global stacks,
    /// <list type="bullet">
    /// <item>A stack for the current SQL select statement.  This is needed by
    /// <see cref="SqlGenerator.Visit(DbVariableReferenceExpression)"/> to create a
    /// list of free variables used by a select statement.  This is needed for
    /// alias renaming.
    /// </item>
    /// <item>A stack for the join context.  When visiting an extent,
    /// we need to know whether we are inside a join or not.  If we are inside
    /// a join, we do not create a new SELECT statement.</item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// Global state.
    /// To enable renaming, we maintain
    /// <list type="bullet">
    /// <item>The set of all extent aliases used.</item>
    /// <item>The set of all parameter names.</item>
    /// <item>The set of all column names that may need to be renamed.</item>
    /// </list>
    ///
    /// Finally, we have a symbol table to lookup variable references.  All references
    /// to the same extent have the same symbol.
    /// </para>
    ///
    /// <para>
    /// Sql select statement sharing.
    ///
    /// Each of the relational operator nodes
    /// <list type="bullet">
    /// <item>Project</item>
    /// <item>Filter</item>
    /// <item>GroupBy</item>
    /// <item>Sort/OrderBy</item>
    /// </list>
    /// can add its non-input (e.g. project, predicate, sort order etc.) to
    /// the SQL statement for the input, or create a new SQL statement.
    /// If it chooses to reuse the input's SQL statement, we play the following
    /// symbol table trick to accomplish renaming.  The symbol table entry for
    /// the alias of the current node points to the symbol for the input in
    /// the input's SQL statement.
    /// <example>
    /// <code>
    /// Project(b.x
    ///     b = Filter(
    ///         c = Extent(foo)
    ///         c.x = 5)
    ///     )
    /// </code>
    /// The Extent node creates a new SqlSelectStatement.  This is added to the
    /// symbol table by the Filter as {c, Symbol(c)}.  Thus, <c>c.x</c> is resolved to
    /// <c>Symbol(c).x</c>.
    /// Looking at the project node, we add {b, Symbol(c)} to the symbol table if the
    /// SQL statement is reused, and {b, Symbol(b)}, if there is no reuse.
    ///
    /// Thus, <c>b.x</c> is resolved to <c>Symbol(c).x</c> if there is reuse, and to
    /// <c>Symbol(b).x</c> if there is no reuse.
    /// </example>
    /// </para>
    /// </remarks>
    internal sealed class SqlGenerator : DbExpressionVisitor<ISqlFragment>
    {
        #region Visitor parameter stacks
        /// <summary>
        /// Every relational node has to pass its SELECT statement to its children
        /// This allows them (DbVariableReferenceExpression eventually) to update the list of
        /// outer extents (free variables) used by this select statement.
        /// </summary>
        private Stack<SqlSelectStatement> selectStatementStack;

        /// <summary>
        /// The top of the stack
        /// </summary>
        private SqlSelectStatement CurrentSelectStatement
        {
            // There is always something on the stack, so we can always Peek.
            get { return selectStatementStack.Peek(); }
        }

        /// <summary>
        /// Nested joins and extents need to know whether they should create
        /// a new Select statement, or reuse the parent's.  This flag
        /// indicates whether the parent is a join or not.
        /// </summary>
        private Stack<bool> isParentAJoinStack;

        /// <summary>
        /// Determine if the parent is a join.
        /// </summary>
        private bool IsParentAJoin
        {
            // There might be no entry on the stack if a Join node has never
            // been seen, so we return false in that case.
            get { return isParentAJoinStack.Count == 0 ? false : isParentAJoinStack.Peek(); }
        }

        #endregion

        #region Global lists and state
        Dictionary<string, int> allExtentNames;
        internal Dictionary<string, int> AllExtentNames
        {
            get { return allExtentNames; }
        }

        // For each column name, we store the last integer suffix that
        // was added to produce a unique column name.  This speeds up
        // the creation of the next unique name for this column name.
        Dictionary<string, int> allColumnNames;
        internal Dictionary<string, int> AllColumnNames
        {
            get { return allColumnNames; }
        }

        readonly SymbolTable symbolTable = new SymbolTable();

        /// <summary>
        /// VariableReferenceExpressions are allowed only as children of DbPropertyExpression
        /// or MethodExpression.  The cheapest way to ensure this is to set the following
        /// property in DbVariableReferenceExpression and reset it in the allowed parent expressions.
        /// </summary>
        private bool isVarRefSingle;

        private readonly SymbolUsageManager optionalColumnUsageManager = new SymbolUsageManager();

        /// <summary>
        /// Maintain the list of (string) DbParameterReferenceExpressions that should be compensated, viz.
        /// forced to non-unicode format. A parameter is added to the list if it is being compared to a 
        /// non-unicode store column and none of its other usages in the query tree, disqualify it 
        /// (For example - if the parameter is also being projected or compared to a unicode column)
        /// The goal of the compensation is to have the store index picked up by the server.
        /// String constants are also compensated and the decision is local, unlike parameters.
        /// </summary>
        private Dictionary<string, bool> _candidateParametersToForceNonUnicode = new Dictionary<string, bool>();
        /// <summary>
        /// Set and reset in DbComparisonExpression and DbLikeExpression visit methods. Maintains
        /// global state information that the children of these nodes are candidates for compensation.
        /// </summary>
        private bool _forceNonUnicode = false;

        /// <summary>
        /// Set when it is is safe to ignore the unicode/non-unicode aspect. See <see cref="VisitIsNullExpression"/> for an example.
        /// </summary>
        private bool _ignoreForceNonUnicodeFlag;

        #endregion

        #region Statics
        
        const byte defaultDecimalPrecision = 18;
        static private readonly char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        // Define lists of functions that take string arugments and return strings.
        static private readonly Set<string> _canonicalStringFunctionsOneArg = new Set<string>(new string[] { "Edm.Trim"    ,
                                                                                           "Edm.RTrim"   ,
                                                                                           "Edm.LTrim"            ,
                                                                                           "Edm.Left"           ,
                                                                                           "Edm.Right"  ,
                                                                                           "Edm.Substring"           ,
                                                                                           "Edm.ToLower"           ,
                                                                                           "Edm.ToUpper"           ,
                                                                                           "Edm.Reverse"           },
                                                                        StringComparer.Ordinal).MakeReadOnly();

        static private readonly Set<string> _canonicalStringFunctionsTwoArgs = new Set<string>(new string[] { "Edm.Concat" },
                                                                StringComparer.Ordinal).MakeReadOnly();

        static private readonly Set<string> _canonicalStringFunctionsThreeArgs = new Set<string>(new string[] { "Edm.Replace" },
                                                                StringComparer.Ordinal).MakeReadOnly();

        static private readonly Set<string> _storeStringFunctionsOneArg = new Set<string>(new string[] { "SqlServer.RTRIM"    ,
                                                                                           "SqlServer.LTRIM"   ,
                                                                                           "SqlServer.LEFT"           ,
                                                                                           "SqlServer.RIGHT"  ,
                                                                                           "SqlServer.SUBSTRING"           ,
                                                                                           "SqlServer.LOWER"           ,
                                                                                           "SqlServer.UPPER"           ,
                                                                                           "SqlServer.REVERSE"           },
                                                                        StringComparer.Ordinal).MakeReadOnly();

        static private readonly Set<string> _storeStringFunctionsThreeArgs = new Set<string>(new string[] { "SqlServer.REPLACE" },
                                                                StringComparer.Ordinal).MakeReadOnly();

        #endregion

        #region SqlVersion, Metadata, ...

        /// <summary>
        /// The current SQL Server version
        /// </summary>
        private SqlVersion sqlVersion;
        internal SqlVersion SqlVersion 
        {
            get { return sqlVersion; }
        }
        internal bool IsPreKatmai
        {
            get { return SqlVersionUtils.IsPreKatmai(this.SqlVersion); }
        }

        private MetadataWorkspace metadataWorkspace;
        internal MetadataWorkspace Workspace
        {
            get { return metadataWorkspace; }
        }

        private TypeUsage integerType = null;
        internal TypeUsage IntegerType
        {
            get
            {
                if (integerType == null)
                {
                    integerType = GetPrimitiveType(PrimitiveTypeKind.Int64);
                }
                return integerType;
            }
        }

        private string defaultStringTypeName;
        internal string DefaultStringTypeName
        {
            get
            {
                if (defaultStringTypeName == null)
                {
                    defaultStringTypeName = GetSqlPrimitiveType(TypeUsage.CreateStringTypeUsage(this.metadataWorkspace.GetModelPrimitiveType(PrimitiveTypeKind.String), isUnicode: true, isFixedLength: false));
                }
                return defaultStringTypeName;
            }
        }

        private StoreItemCollection _storeItemCollection;
        internal StoreItemCollection StoreItemCollection
        {
            get { return _storeItemCollection; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Basic constructor. 
        /// </summary>
        /// <param name="sqlVersion">server version</param>
        private SqlGenerator(SqlVersion sqlVersion)
        {
            this.sqlVersion = sqlVersion;
        }
        #endregion

        #region Entry points
        /// <summary>
        /// General purpose static function that can be called from System.Data assembly
        /// </summary>
        /// <param name="sqlVersion">Server version</param>
        /// <param name="tree">command tree</param>
        /// <param name="parameters">Parameters to add to the command tree corresponding
        /// to constants in the command tree. Used only in ModificationCommandTrees.</param>
        /// <param name="commandType">CommandType for generated command.</param>
        /// <returns>The string representing the SQL to be executed.</returns>
        internal static string GenerateSql(DbCommandTree tree, SqlVersion sqlVersion, out List<SqlParameter> parameters, out CommandType commandType, out HashSet<string> paramsToForceNonUnicode)
        {
            SqlGenerator sqlGen;
            commandType = CommandType.Text;
            parameters = null;
            paramsToForceNonUnicode = null;

            switch (tree.CommandTreeKind)
            {
                case DbCommandTreeKind.Query:
                    sqlGen = new SqlGenerator(sqlVersion);
                    return sqlGen.GenerateSql((DbQueryCommandTree)tree, out paramsToForceNonUnicode);
                    
                case DbCommandTreeKind.Insert:
                    return DmlSqlGenerator.GenerateInsertSql((DbInsertCommandTree)tree, sqlVersion, out parameters);

                case DbCommandTreeKind.Delete:
                    return DmlSqlGenerator.GenerateDeleteSql((DbDeleteCommandTree)tree, sqlVersion, out parameters);

                case DbCommandTreeKind.Update:
                    return DmlSqlGenerator.GenerateUpdateSql((DbUpdateCommandTree)tree, sqlVersion, out parameters);

                case DbCommandTreeKind.Function:
                    sqlGen = new SqlGenerator(sqlVersion);
                    return GenerateFunctionSql((DbFunctionCommandTree)tree, out commandType);

                default:
                    //We have covered all command tree kinds
                    Debug.Assert(false, "Unknown command tree kind");
                    parameters = null;
                    return null;
            }
        }

        private static string GenerateFunctionSql(DbFunctionCommandTree tree, out CommandType commandType)
        {
            Debug.Assert(tree.EdmFunction != null, "DbFunctionCommandTree function cannot be null");

            EdmFunction function = tree.EdmFunction;

            if (String.IsNullOrEmpty(function.CommandTextAttribute))
            {
                // build a quoted description of the function
                commandType = CommandType.StoredProcedure;

                // if the schema name is not explicitly given, it is assumed to be the metadata namespace
                string schemaName = String.IsNullOrEmpty(function.Schema) ?
                    function.NamespaceName : function.Schema;

                // if the function store name is not explicitly given, it is assumed to be the metadata name
                string functionName = String.IsNullOrEmpty(function.StoreFunctionNameAttribute) ?
                    function.Name : function.StoreFunctionNameAttribute;

                // quote elements of function text
                string quotedSchemaName = QuoteIdentifier(schemaName);
                string quotedFunctionName = QuoteIdentifier(functionName);

                // separator
                const string schemaSeparator = ".";

                // concatenate elements of function text
                string quotedFunctionText = quotedSchemaName + schemaSeparator + quotedFunctionName;

                return quotedFunctionText;
            }
            else
            {
                // if the user has specified the command text, pass it through verbatim and choose CommandType.Text
                commandType = CommandType.Text;
                return function.CommandTextAttribute;
            }
        }
        #endregion

        #region Driver Methods
        /// <summary>
        /// Translate a command tree to a SQL string.
        ///
        /// The input tree could be translated to either a SQL SELECT statement
        /// or a SELECT expression.  This choice is made based on the return type
        /// of the expression
        /// CollectionType => select statement
        /// non collection type => select expression
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>The string representing the SQL to be executed.</returns>
        private string GenerateSql(DbQueryCommandTree tree, out HashSet<string> paramsToForceNonUnicode)
        {
            Debug.Assert(tree.Query != null, "DbQueryCommandTree Query cannot be null");

            DbQueryCommandTree targetTree = tree;

            //If we are on Sql 8.0 rewrite the tree if needed
            if (this.SqlVersion == SqlVersion.Sql8)
            {
                if (Sql8ConformanceChecker.NeedsRewrite(tree.Query))
                {
                    targetTree = Sql8ExpressionRewriter.Rewrite(tree);
                }
            }
            
            this.metadataWorkspace = targetTree.MetadataWorkspace;
            // needed in Private Type Helpers section bellow
            _storeItemCollection = (StoreItemCollection)this.Workspace.GetItemCollection(DataSpace.SSpace);

            selectStatementStack = new Stack<SqlSelectStatement>();
            isParentAJoinStack = new Stack<bool>();

            allExtentNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            allColumnNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Literals will not be converted to parameters.

            ISqlFragment result;
            if (TypeSemantics.IsCollectionType(targetTree.Query.ResultType))
            {
                SqlSelectStatement sqlStatement = VisitExpressionEnsureSqlStatement(targetTree.Query);
                Debug.Assert(sqlStatement != null, "The outer most sql statment is null");
                sqlStatement.IsTopMost = true;
                result = sqlStatement;

            }
            else
            {
                SqlBuilder sqlBuilder = new SqlBuilder();
                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append(targetTree.Query.Accept(this));

                result = sqlBuilder;
            }

            if (isVarRefSingle)
            {
                throw EntityUtil.NotSupported();
                // A DbVariableReferenceExpression has to be a child of DbPropertyExpression or MethodExpression
            }

            // Check that the parameter stacks are not leaking.
            Debug.Assert(selectStatementStack.Count == 0);
            Debug.Assert(isParentAJoinStack.Count == 0);

            paramsToForceNonUnicode = new HashSet<string>(_candidateParametersToForceNonUnicode.Where(p => p.Value).Select(q => q.Key).ToList());

            return WriteSql(result);
        }

        /// <summary>
        /// Convert the SQL fragments to a string.
        /// We have to setup the Stream for writing.
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <returns>A string representing the SQL to be executed.</returns>
        private string WriteSql(ISqlFragment sqlStatement)
        {
            StringBuilder builder = new StringBuilder(1024);
            using (SqlWriter writer = new SqlWriter(builder))
            {
                sqlStatement.WriteSql(writer, this);
            }

            return builder.ToString();
        }
        #endregion

        #region IExpressionVisitor Members

        /// <summary>
        /// Translate(left) AND Translate(right)
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>.</returns>
        public override ISqlFragment Visit(DbAndExpression e)
        {
            return VisitBinaryExpression(" AND ", DbExpressionKind.And, e.Left, e.Right);
        }

        /// <summary>
        /// An apply is just like a join, so it shares the common join processing
        /// in <see cref="VisitJoinExpression"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/>.</returns>
        public override ISqlFragment Visit(DbApplyExpression e)
        {
            Debug.Assert(this.SqlVersion != SqlVersion.Sql8, "DbApplyExpression when translating for SQL Server 2000.");

            List<DbExpressionBinding> inputs = new List<DbExpressionBinding>();
            inputs.Add(e.Input);
            inputs.Add(e.Apply);

            string joinString;
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.CrossApply:
                    joinString = "CROSS APPLY";
                    break;

                case DbExpressionKind.OuterApply:
                    joinString = "OUTER APPLY";
                    break;

                default:
                    Debug.Assert(false);
                    throw EntityUtil.InvalidOperation(String.Empty);
            }

            // The join condition does not exist in this case, so we use null.
            // WE do not have a on clause, so we use JoinType.CrossJoin.
            return VisitJoinExpression(inputs, DbExpressionKind.CrossJoin, joinString, null);
        }

        /// <summary>
        /// For binary expressions, we delegate to <see cref="VisitBinaryExpression"/>.
        /// We handle the other expressions directly.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbArithmeticExpression e)
        {
            SqlBuilder result;

            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Divide:
                    result = VisitBinaryExpression(" / ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;
                case DbExpressionKind.Minus:
                    result = VisitBinaryExpression(" - ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;
                case DbExpressionKind.Modulo:
                    result = VisitBinaryExpression(" % ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;
                case DbExpressionKind.Multiply:
                    result = VisitBinaryExpression(" * ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;
                case DbExpressionKind.Plus:
                    result = VisitBinaryExpression(" + ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;

                case DbExpressionKind.UnaryMinus:
                    result = new SqlBuilder();
                    result.Append(" -(");
                    result.Append(e.Arguments[0].Accept(this));
                    result.Append(")");
                    break;

                default:
                    Debug.Assert(false);
                    throw EntityUtil.InvalidOperation(String.Empty);
            }

            return result;
        }

        /// <summary>
        /// If the ELSE clause is null, we do not write it out.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbCaseExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            Debug.Assert(e.When.Count == e.Then.Count);

            result.Append("CASE");
            for (int i = 0; i < e.When.Count; ++i)
            {
                result.Append(" WHEN (");
                result.Append(e.When[i].Accept(this));
                result.Append(") THEN ");
                result.Append(e.Then[i].Accept(this));
            }
            // 

            if (e.Else != null && !(e.Else is DbNullExpression))
            {
                result.Append(" ELSE ");
                result.Append(e.Else.Accept(this));
            }

            result.Append(" END");

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbCastExpression e)
        {
            if (Helper.IsSpatialType(e.ResultType))
            {
                return e.Argument.Accept(this);
            }
            else
            {
                SqlBuilder result = new SqlBuilder();
                result.Append(" CAST( ");
                result.Append(e.Argument.Accept(this));
                result.Append(" AS ");
                result.Append(GetSqlPrimitiveType(e.ResultType));
                result.Append(")");

                return result;
            }
        }

        /// <summary>
        /// The parser generates Not(Equals(...)) for &lt;&gt;.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>.</returns>
        public override ISqlFragment Visit(DbComparisonExpression e)
        {
            SqlBuilder result;

            // Don't try to optimize the comparison, if one of the sides isn't of type string.
            if (TypeSemantics.IsPrimitiveType(e.Left.ResultType, PrimitiveTypeKind.String))
            {
                // Check if the Comparison expression is a candidate for compensation in order to optimize store performance.
                _forceNonUnicode = CheckIfForceNonUnicodeRequired(e);
            }

            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Equals:
                    result = VisitComparisonExpression(" = ", e.Left, e.Right);
                    break;
                case DbExpressionKind.LessThan:
                    result = VisitComparisonExpression(" < ", e.Left, e.Right);
                    break;
                case DbExpressionKind.LessThanOrEquals:
                    result = VisitComparisonExpression(" <= ", e.Left, e.Right);
                    break;
                case DbExpressionKind.GreaterThan:
                    result = VisitComparisonExpression(" > ", e.Left, e.Right);
                    break;
                case DbExpressionKind.GreaterThanOrEquals:
                    result = VisitComparisonExpression(" >= ", e.Left, e.Right);
                    break;
                    // The parser does not generate the expression kind below.
                case DbExpressionKind.NotEquals:
                    result = VisitComparisonExpression(" <> ", e.Left, e.Right);
                    break;

                default:
                    Debug.Assert(false);  // The constructor should have prevented this
                    throw EntityUtil.InvalidOperation(String.Empty);
            }

            // Reset the force non-unicode, global state variable if it was set by CheckIfForceNonUnicodeRequired().
            _forceNonUnicode = false;

            return result;
        }

        /// <summary>
        /// Checks if the arguments of the input Comparison or Like expression are candidates 
        /// for compensation. If yes, sets global state variable - _forceNonUnicode.
        /// </summary>
        /// <param name="e">DBComparisonExpression or DbLikeExpression</param>
        private bool CheckIfForceNonUnicodeRequired(DbExpression e)
        {
            if (_forceNonUnicode)
            {
                Debug.Assert(false);
                throw EntityUtil.NotSupported();
            }
            return MatchPatternForForcingNonUnicode(e);
        }

        /// <summary>
        /// The grammar for the pattern that we are looking for is -
        /// 
        /// Pattern := Target OP Source | Source OP Target
        /// OP := Like | Comparison
        /// Source := Non-unicode DbPropertyExpression
        /// Target := Target FUNC Target | DbConstantExpression | DBParameterExpression
        /// FUNC := CONCAT | RTRIM | LTRIM | TRIM | SUBSTRING | TOLOWER | TOUPPER | REVERSE | REPLACE
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool MatchPatternForForcingNonUnicode(DbExpression e)
        {
            if (e.ExpressionKind == DbExpressionKind.Like)
            {
                DbLikeExpression likeExpr = (DbLikeExpression)e;
                return MatchSourcePatternForForcingNonUnicode(likeExpr.Argument) &&
                    MatchTargetPatternForForcingNonUnicode(likeExpr.Pattern) &&
                    MatchTargetPatternForForcingNonUnicode(likeExpr.Escape);
            }

            // DBExpressionKind is any of (Equals, LessThan, LessThanOrEquals, GreaterThan, GreaterThanOrEquals, NotEquals)
            DbComparisonExpression compareExpr = (DbComparisonExpression)e;
            DbExpression left = compareExpr.Left;
            DbExpression right = compareExpr.Right;

            return (MatchSourcePatternForForcingNonUnicode(left) && MatchTargetPatternForForcingNonUnicode(right)) ||
                    (MatchSourcePatternForForcingNonUnicode(right) && MatchTargetPatternForForcingNonUnicode(left));
        }

        /// <summary>
        /// Matches the non-terminal symbol "target" in above grammar.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private bool MatchTargetPatternForForcingNonUnicode(DbExpression expr)
        {
            if (IsConstParamOrNullExpressionUnicodeNotSpecified(expr))
            {
                return true;
            }

            if (expr.ExpressionKind == DbExpressionKind.Function)
            {
                DbFunctionExpression functionExpr = (DbFunctionExpression)expr;
                EdmFunction function = functionExpr.Function;

                if (!TypeHelpers.IsCanonicalFunction(function) && !SqlFunctionCallHandler.IsStoreFunction(function))
                {
                    return false;
                }

                // All string arguments to the function must be candidates to match target pattern.
                String functionFullName = function.FullName;
                bool ifQualifies = false;

                if (_canonicalStringFunctionsOneArg.Contains(functionFullName) ||
                    _storeStringFunctionsOneArg.Contains(functionFullName))
                {
                    ifQualifies = MatchTargetPatternForForcingNonUnicode(functionExpr.Arguments[0]);
                }
                else if (_canonicalStringFunctionsTwoArgs.Contains(functionFullName))
                {
                    ifQualifies = ( MatchTargetPatternForForcingNonUnicode(functionExpr.Arguments[0]) && 
                                    MatchTargetPatternForForcingNonUnicode(functionExpr.Arguments[1]) );
                }
                else if (_canonicalStringFunctionsThreeArgs.Contains(functionFullName) ||
                    _storeStringFunctionsThreeArgs.Contains(functionFullName))
                {
                    ifQualifies = ( MatchTargetPatternForForcingNonUnicode(functionExpr.Arguments[0]) &&
                                    MatchTargetPatternForForcingNonUnicode(functionExpr.Arguments[1]) &&
                                    MatchTargetPatternForForcingNonUnicode(functionExpr.Arguments[2]) );
                }
                return ifQualifies;
            }

            return false;
        }

        /// <summary>
        /// Determines if the expression represents a non-unicode string column(char/varchar store type)
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        private bool MatchSourcePatternForForcingNonUnicode(DbExpression argument)
        {
            bool isUnicode;
            return (argument.ExpressionKind == DbExpressionKind.Property) &&
                 (TypeHelpers.TryGetIsUnicode(argument.ResultType, out isUnicode)) &&
                 (!isUnicode);
        }

        /// <summary>
        /// Determines if the expression represents a string constant or parameter with the facet, unicode=null.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        private bool IsConstParamOrNullExpressionUnicodeNotSpecified(DbExpression argument)
        {
            bool isUnicode;
            DbExpressionKind expressionKind = argument.ExpressionKind;
            TypeUsage type = argument.ResultType;

            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String))
            {
                return false;
            }

            return  (expressionKind == DbExpressionKind.Constant || 
                  expressionKind == DbExpressionKind.ParameterReference ||
                  expressionKind == DbExpressionKind.Null) &&
                 (!TypeHelpers.TryGetBooleanFacetValue(type, DbProviderManifest.UnicodeFacetName, out isUnicode));
        }

        /// <summary>
        /// Generate tsql for a constant. Avoid the explicit cast (if possible) when
        /// the isCastOptional parameter is set
        /// </summary>
        /// <param name="e">the constant expression</param>
        /// <param name="isCastOptional">can we avoid the CAST</param>
        /// <returns>the tsql fragment</returns>
        private ISqlFragment VisitConstant(DbConstantExpression e, bool isCastOptional)
        {
            // Constants will be sent to the store as part of the generated TSQL, not as parameters
            SqlBuilder result = new SqlBuilder();

            TypeUsage resultType = e.ResultType;
            PrimitiveTypeKind typeKind;
            // Model Types can be (at the time of this implementation):
            //      Binary, Boolean, Byte, Date, DateTime, DateTimeOffset, Decimal, Double, Guid, Int16, Int32, Int64, Single, String, Time
            if (TypeHelpers.TryGetPrimitiveTypeKind(resultType, out typeKind))
            {
                switch (typeKind)
                {
                    case PrimitiveTypeKind.Int32:
                        // default sql server type for integral values.
                        result.Append(e.Value.ToString());
                        break;

                    case PrimitiveTypeKind.Binary:
                        result.Append(" 0x");
                        result.Append(ByteArrayToBinaryString((Byte[])e.Value));
                        result.Append(" ");
                        break;

                    case PrimitiveTypeKind.Boolean:
                        // Bugs 450277, 430294: Need to preserve the boolean type-ness of
                        // this value for round-trippability
                        WrapWithCastIfNeeded(!isCastOptional, (bool)e.Value ? "1" : "0", "bit", result);
                        break;

                    case PrimitiveTypeKind.Byte:
                        WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "tinyint", result);
                        break;

                    case PrimitiveTypeKind.DateTime:
                        result.Append("convert(");
                        result.Append(this.IsPreKatmai ? "datetime" : "datetime2");
                        result.Append(", ");
                        result.Append(EscapeSingleQuote(((System.DateTime)e.Value).ToString(this.IsPreKatmai ? "yyyy-MM-dd HH:mm:ss.fff" : "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture), false /* IsUnicode */));
                        result.Append(", 121)");
                        break;

                    case PrimitiveTypeKind.Time:
                        AssertKatmaiOrNewer(typeKind);
                        result.Append("convert(");
                        result.Append(e.ResultType.EdmType.Name);
                        result.Append(", ");
                        result.Append(EscapeSingleQuote(e.Value.ToString(), false /* IsUnicode */));
                        result.Append(", 121)");
                        break;

                    case PrimitiveTypeKind.DateTimeOffset:
                        AssertKatmaiOrNewer(typeKind);
                        result.Append("convert(");
                        result.Append(e.ResultType.EdmType.Name);
                        result.Append(", ");
                        result.Append(EscapeSingleQuote(((System.DateTimeOffset)e.Value).ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture), false /* IsUnicode */));
                        result.Append(", 121)");
                        break;

                    case PrimitiveTypeKind.Decimal:
                        string strDecimal = ((Decimal)e.Value).ToString(CultureInfo.InvariantCulture);                        
                        // if the decimal value has no decimal part, cast as decimal to preserve type
                        // if the number has precision > int64 max precision, it will be handled as decimal by sql server
                        // and does not need cast. if precision is lest then 20, then cast using Max(literal precision, sql default precision)
                        bool needsCast = -1 == strDecimal.IndexOf('.') && (strDecimal.TrimStart(new char[] { '-' }).Length < 20);
                  
                        byte precision = (byte)Math.Max((Byte)strDecimal.Length, defaultDecimalPrecision);
                        Debug.Assert(precision > 0, "Precision must be greater than zero");

                        string decimalType = "decimal(" + precision.ToString(CultureInfo.InvariantCulture) + ")";

                        WrapWithCastIfNeeded(needsCast, strDecimal, decimalType, result);
                        break;

                    case PrimitiveTypeKind.Double:
                        {
                            double doubleValue = (Double)e.Value;
                            AssertValidDouble(doubleValue);
                            WrapWithCastIfNeeded(true, doubleValue.ToString("R", CultureInfo.InvariantCulture), "float(53)", result);
                        }
                        break;

                    case PrimitiveTypeKind.Geography:
                        AppendSpatialConstant(result, ((DbGeography)e.Value).AsSpatialValue());
                        break;

                    case PrimitiveTypeKind.Geometry:
                        AppendSpatialConstant(result, ((DbGeometry)e.Value).AsSpatialValue());
                        break;

                    case PrimitiveTypeKind.Guid:
                        WrapWithCastIfNeeded(true, EscapeSingleQuote(e.Value.ToString(), false /* IsUnicode */), "uniqueidentifier", result);
                        break;

                    case PrimitiveTypeKind.Int16:
                        WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "smallint", result);
                        break;

                    case PrimitiveTypeKind.Int64:
                        WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "bigint", result);
                        break;

                    case PrimitiveTypeKind.Single:
                        {
                            float singleValue = (float)e.Value;
                            AssertValidSingle(singleValue);
                            WrapWithCastIfNeeded(true, singleValue.ToString("R", CultureInfo.InvariantCulture), "real", result);
                        }
                        break;

                    case PrimitiveTypeKind.String:
                        bool isUnicode;

                        if (!TypeHelpers.TryGetIsUnicode(e.ResultType, out isUnicode))
                        {
                            // If the unicode facet is not specified, if needed force non-unicode, otherwise default to unicode.
                            isUnicode = !_forceNonUnicode;
                        }
                        result.Append(EscapeSingleQuote(e.Value as string, isUnicode));
                        break;

                    default:
                        // all known scalar types should been handled already.
                        throw EntityUtil.NotSupported(System.Data.Entity.Strings.NoStoreTypeForEdmType(resultType.Identity, ((PrimitiveType)(resultType.EdmType)).PrimitiveTypeKind));
                }
            }
            else
            {
                throw EntityUtil.NotSupported();
                //if/when Enum types are supported, then handle appropriately, for now is not a valid type for constants.
                //result.Append(e.Value.ToString());
            }

            return result;
        }

        private void AppendSpatialConstant(SqlBuilder result, IDbSpatialValue spatialValue)
        {
            // Spatial constants are represented by calls to a static constructor function. The attempt is made to extract an
            // appropriate representation from the value (which may not implement the required methods). If an SRID value and
            // a text, binary or GML representation of the spatial value can be extracted, the the corresponding function call
            // expression is built and processed.
            DbFunctionExpression functionExpression = null;
            int? srid = spatialValue.CoordinateSystemId;
            if (srid.HasValue)
            {
                string wellKnownText = spatialValue.WellKnownText;
                if (wellKnownText != null)
                {
                    functionExpression = (spatialValue.IsGeography ? SpatialEdmFunctions.GeographyFromText(wellKnownText, srid.Value) : SpatialEdmFunctions.GeometryFromText(wellKnownText, srid.Value));
                }
                else
                {
                    byte[] wellKnownBinary = spatialValue.WellKnownBinary;
                    if (wellKnownBinary != null)
                    {
                        functionExpression = (spatialValue.IsGeography ? SpatialEdmFunctions.GeographyFromBinary(wellKnownBinary, srid.Value) : SpatialEdmFunctions.GeometryFromBinary(wellKnownBinary, srid.Value));
                    }
                    else
                    {
                        string gmlString = spatialValue.GmlString;
                        if (gmlString != null)
                        {
                            functionExpression = (spatialValue.IsGeography ? SpatialEdmFunctions.GeographyFromGml(gmlString, srid.Value) : SpatialEdmFunctions.GeometryFromGml(gmlString, srid.Value));
                        }
                    }
                }
            }

            if (functionExpression != null)
            {
                result.Append(SqlFunctionCallHandler.GenerateFunctionCallSql(this, functionExpression));
            }
            else
            {
                throw spatialValue.NotSqlCompatible();
            }
        }

        /// <summary>
        /// Helper method for <see cref="VisitConstant"/>
        /// </summary>
        /// <param name="value">A double value</param>
        /// <exception cref="NotSupportedException">If a value of positive or negative infinity, or <see cref="double.NaN"/> is specified</exception>
        private static void AssertValidDouble(double value)
        {
            if (double.IsNaN(value))
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.SqlGen_TypedNaNNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.Double)));
            }
            else if(double.IsPositiveInfinity(value))
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.SqlGen_TypedPositiveInfinityNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.Double), typeof(Double).Name));
            }
            else if(double.IsNegativeInfinity(value))
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.SqlGen_TypedNegativeInfinityNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.Double), typeof(Double).Name));
            }
        }

        /// <summary>
        /// Helper method for <see cref="VisitConstant"/>
        /// </summary>
        /// <param name="value">A single value</param>
        /// <exception cref="NotSupportedException">If a value of positive or negative infinity, or <see cref="float.NaN"/> is specified</exception>
        private static void AssertValidSingle(float value)
        {
            if (float.IsNaN(value))
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.SqlGen_TypedNaNNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.Single)));
            }
            else if(float.IsPositiveInfinity(value))
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.SqlGen_TypedPositiveInfinityNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.Single), typeof(Single).Name));
            }
            else if(float.IsNegativeInfinity(value))
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.SqlGen_TypedNegativeInfinityNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.Single), typeof(Single).Name));
            }
        }

        /// <summary>
        /// Helper function for <see cref="VisitConstant"/>
        /// Appends the given constant value to the result either 'as is' or wrapped with a cast to the given type.
        /// </summary>
        /// <param name="cast"></param>
        /// <param name="value"></param>
        /// <param name="typeName"></param>
        /// <param name="result"></param>
        private static void WrapWithCastIfNeeded(bool cast, string value, string typeName, SqlBuilder result)
        {
            if (!cast)
            {
                result.Append(value);
            }
            else
            {
                result.Append("cast(");
                result.Append(value);
                result.Append(" as ");
                result.Append(typeName);
                result.Append(")");
            }
        }

        /// <summary>
        /// We do not pass constants as parameters.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>.  Strings are wrapped in single
        /// quotes and escaped.  Numbers are written literally.</returns>
        public override ISqlFragment Visit(DbConstantExpression e)
        {
            return VisitConstant(e, false /* isCastOptional */);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbDerefExpression e)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// The DISTINCT has to be added to the beginning of SqlSelectStatement.Select,
        /// but it might be too late for that.  So, we use a flag on SqlSelectStatement
        /// instead, and add the "DISTINCT" in the second phase.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        public override ISqlFragment Visit(DbDistinctExpression e)
        {
            SqlSelectStatement result = VisitExpressionEnsureSqlStatement(e.Argument);

            if (!IsCompatible(result, e.ExpressionKind))
            {
                Symbol fromSymbol;
                TypeUsage inputType = TypeHelpers.GetElementTypeUsage(e.Argument.ResultType);
                result = CreateNewSelectStatement(result, "distinct", inputType, out fromSymbol);
                AddFromSymbol(result, "distinct", fromSymbol, false);
            }
            
            result.Select.IsDistinct = true;
            return result;
        }

        /// <summary>
        /// An element expression returns a scalar - so it is translated to
        /// ( Select ... )
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbElementExpression e)
        {
            // ISSUE: What happens if the DbElementExpression is used as an input expression?
            // i.e. adding the '('  might not be right in all cases.
            SqlBuilder result = new SqlBuilder();
            result.Append("(");
            result.Append(VisitExpressionEnsureSqlStatement(e.Argument));
            result.Append(")");

            return result;
        }

        /// <summary>
        /// <see cref="Visit(DbUnionAllExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbExceptExpression e)
        {
            Debug.Assert(this.SqlVersion != SqlVersion.Sql8, "DbExceptExpression when translating for SQL Server 2000.");
        
            return VisitSetOpExpression(e.Left, e.Right, "EXCEPT");
        }

        /// <summary>
        /// Only concrete expression types will be visited.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbExpression e)
        {
            throw EntityUtil.InvalidOperation(String.Empty);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>If we are in a Join context, returns a <see cref="SqlBuilder"/>
        /// with the extent name, otherwise, a new <see cref="SqlSelectStatement"/>
        /// with the From field set.</returns>
        public override ISqlFragment Visit(DbScanExpression e)
        {
            EntitySetBase target = e.Target;

            // ISSUE: Should we just return a string all the time, and let
            // VisitInputExpression create the SqlSelectStatement?
                        
            if (IsParentAJoin)
            {
                SqlBuilder result = new SqlBuilder();
                result.Append(GetTargetTSql(target));

                return result;
            }
            else
            {
                SqlSelectStatement result = new SqlSelectStatement();
                result.From.Append(GetTargetTSql(target));

                return result;
            }
        }

        /// <summary>
        /// Gets escaped TSql identifier describing this entity set.
        /// </summary>
        /// <returns></returns>
        internal static string GetTargetTSql(EntitySetBase entitySetBase)
        {
            if (null == entitySetBase.CachedProviderSql)
            {
                if (null == entitySetBase.DefiningQuery)
                {
                    // construct escaped T-SQL referencing entity set
                    StringBuilder builder = new StringBuilder(50);
                    if (!string.IsNullOrEmpty(entitySetBase.Schema))
                    {
                        builder.Append(SqlGenerator.QuoteIdentifier(entitySetBase.Schema));
                        builder.Append(".");
                    }
                    else
                    {
                        builder.Append(SqlGenerator.QuoteIdentifier(entitySetBase.EntityContainer.Name));
                        builder.Append(".");
                    }

                    if (!string.IsNullOrEmpty(entitySetBase.Table))
                    {
                        builder.Append(SqlGenerator.QuoteIdentifier(entitySetBase.Table));
                    }
                    else
                    {
                        builder.Append(SqlGenerator.QuoteIdentifier(entitySetBase.Name));
                    }
                    entitySetBase.CachedProviderSql = builder.ToString();
                }
                else
                {
                    entitySetBase.CachedProviderSql = "(" + entitySetBase.DefiningQuery + ")";
                }
            }
            return entitySetBase.CachedProviderSql;
        }
                
        /// <summary>
        /// The bodies of <see cref="Visit(DbFilterExpression)"/>, <see cref="Visit(DbGroupByExpression)"/>,
        /// <see cref="Visit(DbProjectExpression)"/>, <see cref="Visit(DbSortExpression)"/> are similar.
        /// Each does the following.
        /// <list type="number">
        /// <item> Visit the input expression</item>
        /// <item> Determine if the input's SQL statement can be reused, or a new
        /// one must be created.</item>
        /// <item>Create a new symbol table scope</item>
        /// <item>Push the Sql statement onto a stack, so that children can
        /// update the free variable list.</item>
        /// <item>Visit the non-input expression.</item>
        /// <item>Cleanup</item>
        /// </list>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        public override ISqlFragment Visit(DbFilterExpression e)
        {
            return VisitFilterExpression(e.Input, e.Predicate, false);
        }

        /// <summary>
        /// Lambda functions are not supported.
        /// The functions supported are:
        /// <list type="number">
        /// <item>Canonical Functions - We recognize these by their dataspace, it is DataSpace.CSpace</item>
        /// <item>Store Functions - We recognize these by the BuiltInAttribute and not being Canonical</item>
        /// <item>User-defined Functions - All the rest</item>
        /// </list>
        /// We handle Canonical and Store functions the same way: If they are in the list of functions 
        /// that need special handling, we invoke the appropriate handler, otherwise we translate them to
        /// FunctionName(arg1, arg2, ..., argn).
        /// We translate user-defined functions to NamespaceName.FunctionName(arg1, arg2, ..., argn).
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbFunctionExpression e)
        {
            return SqlFunctionCallHandler.GenerateFunctionCallSql(this, e);
        }

        public override ISqlFragment Visit(DbLambdaExpression expression)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbEntityRefExpression e)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbRefKeyExpression e)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// <see cref="Visit(DbFilterExpression)"/> for general details.
        /// We modify both the GroupBy and the Select fields of the SqlSelectStatement.
        /// GroupBy gets just the keys without aliases,
        /// and Select gets the keys and the aggregates with aliases.
        /// 
        /// Sql Server does not support arbitrarily complex expressions inside aggregates, 
        /// and requires keys to have reference to the input scope, 
        /// so in some cases we create a nested query in which we alias the arguments to the aggregates. 
        /// The exact limitations of Sql Server are:
        /// <list type="number">
        /// <item>If an expression being aggregated contains an outer reference, then that outer 
        /// reference must be the only column referenced in the expression (SQLBUDT #488741)</item>
        /// <item>Sql Server cannot perform an aggregate function on an expression containing 
        /// an aggregate or a subquery. (SQLBUDT #504600)</item>
        ///<item>Sql Server requries each GROUP BY expression (key) to contain at least one column 
        /// that is not an outer reference. (SQLBUDT #616523)</item>
        /// <item>Aggregates on the right side of an APPLY cannot reference columns from the left side.
        /// (SQLBUDT #617683) </item>
        /// </list>
        /// 
        /// The default translation, without inner query is: 
        /// 
        ///     SELECT 
        ///         kexp1 AS key1, kexp2 AS key2,... kexpn AS keyn, 
        ///         aggf1(aexpr1) AS agg1, .. aggfn(aexprn) AS aggn
        ///     FROM input AS a
        ///     GROUP BY kexp1, kexp2, .. kexpn
        /// 
        /// When we inject an innner query, the equivalent translation is:
        /// 
        ///     SELECT 
        ///         key1 AS key1, key2 AS key2, .. keyn AS keys,  
        ///         aggf1(agg1) AS agg1, aggfn(aggn) AS aggn
        ///     FROM (
        ///             SELECT 
        ///                 kexp1 AS key1, kexp2 AS key2,... kexpn AS keyn, 
        ///                 aexpr1 AS agg1, .. aexprn AS aggn
        ///             FROM input AS a
        ///         ) as a
        ///     GROUP BY key1, key2, keyn
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        public override ISqlFragment Visit(DbGroupByExpression e)
        {
            Symbol fromSymbol;
            SqlSelectStatement innerQuery = VisitInputExpression(e.Input.Expression,
                e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // GroupBy is compatible with Filter and OrderBy
            // but not with Project, GroupBy
            if (!IsCompatible(innerQuery, e.ExpressionKind))
            {
                innerQuery = CreateNewSelectStatement(innerQuery, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(innerQuery);
            symbolTable.EnterScope();

            AddFromSymbol(innerQuery, e.Input.VariableName, fromSymbol);
            // This line is not present for other relational nodes.
            symbolTable.Add(e.Input.GroupVariableName, fromSymbol);

            // The enumerator is shared by both the keys and the aggregates,
            // so, we do not close it in between.
            RowType groupByType = TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage);

            //SQL Server does not support arbitrarily complex expressions inside aggregates, 
            // and requires keys to have reference to the input scope, 
            // so we check for the specific restrictions and if need we inject an inner query.
            bool needsInnerQuery = GroupByAggregatesNeedInnerQuery(e.Aggregates, e.Input.GroupVariableName) || GroupByKeysNeedInnerQuery(e.Keys, e.Input.VariableName);

            SqlSelectStatement result;
            if (needsInnerQuery)
            {
                //Create the inner query
                result = CreateNewSelectStatement(innerQuery, e.Input.VariableName, e.Input.VariableType, false, out fromSymbol);
                AddFromSymbol(result, e.Input.VariableName, fromSymbol, false);
            }
            else
            {
                result = innerQuery;
            }

            using (IEnumerator<EdmProperty> members = groupByType.Properties.GetEnumerator())
            {
                members.MoveNext();
                Debug.Assert(result.Select.IsEmpty);

                string separator = "";

                foreach (DbExpression key in e.Keys)
                {
                    EdmProperty member = members.Current;
                    string alias = QuoteIdentifier(member.Name);

                    result.GroupBy.Append(separator);

                    ISqlFragment keySql = key.Accept(this);

                    if (!needsInnerQuery)
                    {
                        //Default translation: Key AS Alias
                        result.Select.Append(separator);
                        result.Select.AppendLine();
                        result.Select.Append(keySql);
                        result.Select.Append(" AS ");
                        result.Select.Append(alias);

                        result.GroupBy.Append(keySql);
                    }
                    else
                    {
                        // The inner query contains the default translation Key AS Alias
                        innerQuery.Select.Append(separator);
                        innerQuery.Select.AppendLine();
                        innerQuery.Select.Append(keySql);
                        innerQuery.Select.Append(" AS ");
                        innerQuery.Select.Append(alias);

                        //The outer resulting query projects over the key aliased in the inner query: 
                        //  fromSymbol.Alias AS Alias
                        result.Select.Append(separator);
                        result.Select.AppendLine();
                        result.Select.Append(fromSymbol);
                        result.Select.Append(".");
                        result.Select.Append(alias);
                        result.Select.Append(" AS ");
                        result.Select.Append(alias);
                        
                        result.GroupBy.Append(alias);
                    }

                    separator = ", ";
                    members.MoveNext();
                }

                foreach (DbAggregate aggregate in e.Aggregates)
                {
                    EdmProperty member = members.Current;
                    string alias = QuoteIdentifier(member.Name);

                    Debug.Assert(aggregate.Arguments.Count == 1);
                    ISqlFragment translatedAggregateArgument = aggregate.Arguments[0].Accept(this);

                    object aggregateArgument;

                    if (needsInnerQuery)
                    {
                        //In this case the argument to the aggratete is reference to the one projected out by the
                        // inner query
                        SqlBuilder wrappingAggregateArgument = new SqlBuilder();
                        wrappingAggregateArgument.Append(fromSymbol);
                        wrappingAggregateArgument.Append(".");
                        wrappingAggregateArgument.Append(alias);
                        aggregateArgument = wrappingAggregateArgument;

                        innerQuery.Select.Append(separator);
                        innerQuery.Select.AppendLine();
                        innerQuery.Select.Append(translatedAggregateArgument);
                        innerQuery.Select.Append(" AS ");
                        innerQuery.Select.Append(alias);
                    }
                    else
                    {
                        aggregateArgument = translatedAggregateArgument;
                    }

                    ISqlFragment aggregateResult = VisitAggregate(aggregate, aggregateArgument);

                    result.Select.Append(separator);
                    result.Select.AppendLine();
                    result.Select.Append(aggregateResult);
                    result.Select.Append(" AS ");
                    result.Select.Append(alias);

                    separator = ", ";
                    members.MoveNext();
                }
            }

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        /// <summary>
        /// <see cref="Visit(DbUnionAllExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbIntersectExpression e)
        {
            Debug.Assert(this.SqlVersion != SqlVersion.Sql8, "DbIntersectExpression when translating for SQL Server 2000.");

            return VisitSetOpExpression(e.Left, e.Right, "INTERSECT");
        }

        /// <summary>
        /// Not(IsEmpty) has to be handled specially, so we delegate to
        /// <see cref="VisitIsEmptyExpression"/>.
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>.
        /// <code>[NOT] EXISTS( ... )</code>
        /// </returns>
        public override ISqlFragment Visit(DbIsEmptyExpression e)
        {
            return VisitIsEmptyExpression(e, false);
        }

        /// <summary>
        /// Not(IsNull) is handled specially, so we delegate to
        /// <see cref="VisitIsNullExpression"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>
        /// <code>IS [NOT] NULL</code>
        /// </returns>
        public override ISqlFragment Visit(DbIsNullExpression e)
        {
            return VisitIsNullExpression(e, false);
        }

        /// <summary>
        /// No error is raised if the store cannot support this.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbIsOfExpression e)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// <see cref="VisitJoinExpression"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/>.</returns>
        public override ISqlFragment Visit(DbCrossJoinExpression e)
        {
            return VisitJoinExpression(e.Inputs, e.ExpressionKind, "CROSS JOIN", null);
        }

        /// <summary>
        /// <see cref="VisitJoinExpression"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/>.</returns>
        public override ISqlFragment Visit(DbJoinExpression e)
        {
            #region Map join type to a string
            string joinString;
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.FullOuterJoin:
                    joinString = "FULL OUTER JOIN";
                    break;

                case DbExpressionKind.InnerJoin:
                    joinString = "INNER JOIN";
                    break;

                case DbExpressionKind.LeftOuterJoin:
                    joinString = "LEFT OUTER JOIN";
                    break;

                default:
                    Debug.Assert(false);
                    joinString = null;
                    break;
            }
            #endregion

            List<DbExpressionBinding> inputs = new List<DbExpressionBinding>(2);
            inputs.Add(e.Left);
            inputs.Add(e.Right);

            return VisitJoinExpression(inputs, e.ExpressionKind, joinString, e.JoinCondition);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbLikeExpression e)
        {
            // Check if the LIKE expression is a candidate for compensation in order to optimize store performance.
            _forceNonUnicode = CheckIfForceNonUnicodeRequired(e);

            SqlBuilder result = new SqlBuilder();
            result.Append(e.Argument.Accept(this));
            result.Append(" LIKE ");
            result.Append(e.Pattern.Accept(this));

            // if the ESCAPE expression is a DbNullExpression, then that's tantamount to 
            // not having an ESCAPE at all
            if (e.Escape.ExpressionKind != DbExpressionKind.Null)
            {
                result.Append(" ESCAPE ");
                result.Append(e.Escape.Accept(this));
            }

            // Reset the force non-unicode, global state variable if it was set by CheckIfForceNonUnicodeRequired().
            _forceNonUnicode = false;

            return result;
        }

        /// <summary>
        ///  Translates to TOP expression. For Sql8, limit can only be a constant expression
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbLimitExpression e)
        {
            Debug.Assert(e.Limit is DbConstantExpression || e.Limit is DbParameterReferenceExpression, "DbLimitExpression.Limit is of invalid expression type");
            Debug.Assert(!((this.SqlVersion == SqlVersion.Sql8) && (e.Limit is DbParameterReferenceExpression)), "DbLimitExpression.Limit is DbParameterReferenceExpression for SQL Server 2000.");

            SqlSelectStatement result = VisitExpressionEnsureSqlStatement(e.Argument, false, false);
            Symbol fromSymbol;

            if (!IsCompatible(result, e.ExpressionKind))
            {
                TypeUsage inputType = TypeHelpers.GetElementTypeUsage(e.Argument.ResultType);

                result = CreateNewSelectStatement(result, "top", inputType, out fromSymbol);
                AddFromSymbol(result, "top", fromSymbol, false);
            }

            ISqlFragment topCount = HandleCountExpression(e.Limit) ;
            
            result.Select.Top = new TopClause(topCount, e.WithTies);
            return result;
        }

#if METHOD_EXPRESSION
        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(MethodExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            result.Append(e.Instance.Accept(this));
            result.Append(".");
            result.Append(QuoteIdentifier(e.Method.Name));
            result.Append("(");

            // Since the VariableReferenceExpression is a proper child of ours, we can reset
            // isVarSingle.
            VariableReferenceExpression VariableReferenceExpression = e.Instance as VariableReferenceExpression;
            if (VariableReferenceExpression != null)
            {
                isVarRefSingle = false;
            }

            string separator = "";
            foreach (Expression argument in e.Arguments)
            {
                result.Append(separator);
                result.Append(argument.Accept(this));
                separator = ", ";
            }
            result.Append(")");

            return result;
        }
#endif

        /// <summary>
        /// DbNewInstanceExpression is allowed as a child of DbProjectExpression only.
        /// If anyone else is the parent, we throw.
        /// We also perform special casing for collections - where we could convert
        /// them into Unions
        ///
        /// <see cref="VisitNewInstanceExpression"/> for the actual implementation.
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbNewInstanceExpression e)
        {
            if (TypeSemantics.IsCollectionType(e.ResultType))
            {
                return VisitCollectionConstructor(e);
            }
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// The Not expression may cause the translation of its child to change.
        /// These children are
        /// <list type="bullet">
        /// <item><see cref="DbNotExpression"/>NOT(Not(x)) becomes x</item>
        /// <item><see cref="DbIsEmptyExpression"/>NOT EXISTS becomes EXISTS</item>
        /// <item><see cref="DbIsNullExpression"/>IS NULL becomes IS NOT NULL</item>
        /// <item><see cref="DbComparisonExpression"/>= becomes&lt;&gt; </item>
        /// </list>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbNotExpression e)
        {
            // Flatten Not(Not(x)) to x.
            DbNotExpression notExpression = e.Argument as DbNotExpression;
            if (notExpression != null)
            {
                return notExpression.Argument.Accept(this);
            }

            DbIsEmptyExpression isEmptyExpression = e.Argument as DbIsEmptyExpression;
            if (isEmptyExpression != null)
            {
                return VisitIsEmptyExpression(isEmptyExpression, true);
            }

            DbIsNullExpression isNullExpression = e.Argument as DbIsNullExpression;
            if (isNullExpression != null)
            {
                return VisitIsNullExpression(isNullExpression, true);
            }

            DbComparisonExpression comparisonExpression = e.Argument as DbComparisonExpression;
            if (comparisonExpression != null)
            {
                if (comparisonExpression.ExpressionKind == DbExpressionKind.Equals)
                {
                    bool forceNonUnicodeLocal = _forceNonUnicode; // Save flag
                    // Don't try to optimize the comparison, if one of the sides isn't of type string.
                    if (TypeSemantics.IsPrimitiveType(comparisonExpression.Left.ResultType, PrimitiveTypeKind.String))
                    {
                        _forceNonUnicode = CheckIfForceNonUnicodeRequired(comparisonExpression);
                    }
                    SqlBuilder binaryResult = VisitBinaryExpression(" <> ", DbExpressionKind.NotEquals, comparisonExpression.Left, comparisonExpression.Right);
                    _forceNonUnicode = forceNonUnicodeLocal; // Reset flag
                    return binaryResult;
                }
            }

            SqlBuilder result = new SqlBuilder();
            result.Append(" NOT (");
            result.Append(e.Argument.Accept(this));
            result.Append(")");

            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        /// <returns><see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbNullExpression e)
        {
            SqlBuilder result = new SqlBuilder();
            
            // always cast nulls - sqlserver doesn't like case expressions where the "then" clause is null
            result.Append("CAST(NULL AS ");
            TypeUsage type = e.ResultType;

            //
            // Use the narrowest type possible - especially for strings where we don't want 
            // to produce unicode strings always.
            //
            Debug.Assert(Helper.IsPrimitiveType(type.EdmType), "Type must be primitive type");
            PrimitiveType primitiveType = type.EdmType as PrimitiveType;
            switch(primitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.String:
                    result.Append("varchar(1)");
                    break;
                case PrimitiveTypeKind.Binary:
                    result.Append("varbinary(1)");
                    break;
                default:
                    result.Append(GetSqlPrimitiveType(type));
                    break;
            }

            result.Append(")");
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbOfTypeExpression e)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// Visit a DbOrExpression and consider the subexpressions
        /// for whether to generate OR conditions or an IN clause.
        /// </summary>
        /// <param name="e">DbOrExpression to be visited</param>
        /// <returns>A <see cref="SqlBuilder"/>Fragment of SQL generated</returns>
        /// <seealso cref="Visit(DbAndExpression)"/>
        public override ISqlFragment Visit(DbOrExpression e)
        {
            ISqlFragment result = null;
            if (TryTranslateIntoIn(e, out result))
            {
                return result;
            }

            return VisitBinaryExpression(" OR ", e.ExpressionKind, e.Left, e.Right);
        }

        /// <summary>
        /// Determine if a DbOrExpression can be optimized into one or more IN clauses
        /// and generate an ISqlFragment if it is possible.
        /// </summary>
        /// <param name="e">DbOrExpression to attempt translation upon</param>
        /// <param name="sqlFragment">Fragment of SQL generated</param>
        /// <returns>True if an IN clause is possible and sqlFragment has been generated, false otherwise</returns>
        private bool TryTranslateIntoIn(DbOrExpression e, out ISqlFragment sqlFragment)
        {
            var map = new KeyToListMap<DbExpression, DbExpression>(KeyFieldExpressionComparer.Singleton);
            bool useInClause = HasBuiltMapForIn(e, map) && map.Keys.Count() > 0;

            if (!useInClause)
            {
                sqlFragment = null;
                return false;
            }

            var sqlBuilder = new SqlBuilder();
            bool firstKey = true;
            foreach (var key in map.Keys)
            {
                var values = map.ListForKey(key);
                if (!firstKey)
                {
                    sqlBuilder.Append(" OR ");
                }
                else
                {
                    firstKey = false;
                }

                var realValues = values.Where(v => v.ExpressionKind != DbExpressionKind.IsNull);
                int realValueCount = realValues.Count();

                // 
                // Should non-unicode be forced over the key or any of the values
                // If the key qualifies as a source, we force it over the values that qualify as targets
                // If all the values qualify as sources, we force it over the key
                //
                bool forceNonUnicodeOnQualifyingValues = false;
                bool forceNonUnicodeOnKey =  false;
                if (TypeSemantics.IsPrimitiveType(key.ResultType, PrimitiveTypeKind.String))
                {
                    forceNonUnicodeOnQualifyingValues = MatchSourcePatternForForcingNonUnicode(key);
                    forceNonUnicodeOnKey = !forceNonUnicodeOnQualifyingValues && MatchTargetPatternForForcingNonUnicode(key) && realValues.All(v => MatchSourcePatternForForcingNonUnicode(v));                      
                }
                
                if (realValueCount == 1)
                {
                    // When only one value we leave it as an equality test
                    HandleInKey(sqlBuilder, key, forceNonUnicodeOnKey);
                    sqlBuilder.Append(" = ");
                    var value = realValues.First();

                    HandleInValue(sqlBuilder, value, key.ResultType.EdmType == value.ResultType.EdmType, forceNonUnicodeOnQualifyingValues);
                }

                if (realValueCount > 1)
                {
                    // More than one value becomes an IN
                    HandleInKey(sqlBuilder, key, forceNonUnicodeOnKey);
                    sqlBuilder.Append(" IN (");

                    bool firstValue = true;
                    foreach(var value in realValues)
                    {
                        if (!firstValue)
                        {
                            sqlBuilder.Append(",");
                        }
                        else
                        {
                            firstValue = false;
                        }
                        HandleInValue(sqlBuilder, value, key.ResultType.EdmType == value.ResultType.EdmType, forceNonUnicodeOnQualifyingValues);
                    }
                    sqlBuilder.Append(")");
                }

                // Deal with a null for this key
                DbIsNullExpression isNullExpression = values.FirstOrDefault(v => v.ExpressionKind == DbExpressionKind.IsNull) as DbIsNullExpression;
                if (isNullExpression != null)
                {
                    if (realValueCount > 0)
                    {
                        sqlBuilder.Append(" OR ");
                    }                   
                    sqlBuilder.Append(VisitIsNullExpression(isNullExpression, false)); // We never try to build IN with a NOT in the tree
                }
            }

            sqlFragment = sqlBuilder;
            return true;
        }

        private void HandleInValue(SqlBuilder sqlBuilder, DbExpression value, bool isSameEdmType, bool forceNonUnicodeOnQualifyingValues)
        {
            ForcingNonUnicode(() => ParenthesizeExpressionWithoutRedundantConstantCasts(value, sqlBuilder, isSameEdmType),
                forceNonUnicodeOnQualifyingValues && MatchTargetPatternForForcingNonUnicode(value));
        }

        private void HandleInKey(SqlBuilder sqlBuilder, DbExpression key, bool forceNonUnicodeOnKey)
        {
            ForcingNonUnicode(() => ParenthesizeExpressionIfNeeded(key, sqlBuilder), forceNonUnicodeOnKey);
        }

        private void ForcingNonUnicode(Action action, bool forceNonUnicode)
        {
            bool reset = false;
            if (forceNonUnicode && !_forceNonUnicode)
            {
                _forceNonUnicode = true;
                reset = true;
            }
            action();
            if (reset)
            {
                _forceNonUnicode = false;
            }
        }

        private void ParenthesizeExpressionWithoutRedundantConstantCasts(DbExpression value, SqlBuilder sqlBuilder, Boolean isSameEdmType)
        {
            switch (value.ExpressionKind)
            {
                case DbExpressionKind.Constant:
                    {
                        // We don't want unnecessary casts
                        sqlBuilder.Append(VisitConstant((DbConstantExpression)value, isSameEdmType));
                        break;
                    }
                default:
                    {
                        ParenthesizeExpressionIfNeeded(value, sqlBuilder);
                        break;
                    }
            }
        }

        #region Helper methods and classes for translation into "In"

        /// <summary>
        /// Required by the KeyToListMap to allow certain DbExpression subclasses to be used as a key
        /// which is not normally possible given their lack of Equals and GetHashCode implementations
        /// for testing object value equality.
        /// </summary>
        private class KeyFieldExpressionComparer: IEqualityComparer<DbExpression>
        {
            internal static readonly KeyFieldExpressionComparer Singleton = new KeyFieldExpressionComparer();
            private KeyFieldExpressionComparer() { }

            /// <summary>
            /// Compare two DbExpressions to see if they are equal for the purposes of
            /// our key management. We only support DbPropertyExpression, DbParameterReferenceExpression, 
            /// VariableReferenceExpression and DbCastExpression types. Everything else will fail to
            /// be considered equal.
            /// </summary>
            /// <param name="x">First DbExpression to consider for equality</param>
            /// <param name="y">Second DbExpression to consider for equality</param>
            /// <returns>True if the types are allowed and equal, false otherwise</returns>
            public bool Equals(DbExpression x, DbExpression y)
            {
                if (x.ExpressionKind != y.ExpressionKind)
                {
                    return false;
                }
                switch (x.ExpressionKind)
                {
                    case DbExpressionKind.Property:
                        {
                            var first = (DbPropertyExpression)x;
                            var second = (DbPropertyExpression)y;
                            return first.Property == second.Property && this.Equals(first.Instance, second.Instance);
                        }
                    case DbExpressionKind.ParameterReference:
                        {
                            var first = (DbParameterReferenceExpression)x;
                            var second = (DbParameterReferenceExpression)y;
                            return first.ParameterName == second.ParameterName;
                        }
                    case DbExpressionKind.VariableReference:
                        {
                            return x == y;
                        }
                    case DbExpressionKind.Cast:
                        {
                            var first = (DbCastExpression)x;
                            var second = (DbCastExpression)y;
                            return first.ResultType == second.ResultType && this.Equals(first.Argument, second.Argument);
                        }
                }
                return false;
            }

            /// <summary>
            /// Calculates a hashcode for a given number of DbExpression subclasses to allow the KeyToListMap
            /// to efficiently and reliably locate existing keys.
            /// </summary>
            /// <param name="obj">DbExpression to calculate a hashcode for</param>
            /// <returns>Integer containing the hashcode</returns>
            public int GetHashCode(DbExpression obj)
            {
                switch(obj.ExpressionKind)
                {
                    case DbExpressionKind.Property:
                        {
                            return ((DbPropertyExpression)obj).Property.GetHashCode();
                        }
                    case DbExpressionKind.ParameterReference:
                        {
                            return ((DbParameterReferenceExpression)obj).ParameterName.GetHashCode() ^ Int32.MaxValue;
                        }
                    case DbExpressionKind.VariableReference:
                        {
                            return ((DbVariableReferenceExpression)obj).VariableName.GetHashCode();
                        }
                    case DbExpressionKind.Cast:
                        {
                            return GetHashCode(((DbCastExpression)obj).Argument);
                        }
                    default:
                        {
                            return obj.GetHashCode();
                        }
                }
            }
        }

        /// <summary>
        /// Determines if a DbExpression is a valid key for the purposes of generating an In clause optimization.
        /// </summary>
        /// <param name="e">DbExpression to consider</param>
        /// <returns>True if the expression can be used as a key, false otherwise</returns>
        private bool IsKeyForIn(DbExpression e)
        {
            return (e.ExpressionKind == DbExpressionKind.Property
                || e.ExpressionKind == DbExpressionKind.VariableReference
                || e.ExpressionKind == DbExpressionKind.ParameterReference);
        }

        /// <summary>
        /// Looks at both sides of a DbBinaryExpression to consider if either side is a valid candidate to
        /// be a key and if so adds it to the KeyToListMap as a key with the other side as the value.
        /// </summary>
        /// <param name="e">DbBinaryExpression to consider</param>
        /// <param name="values">KeyToListMap to add the sides of the binary expression to</param>
        /// <returns>True if the expression was added, false otherwise</returns>
        private bool TryAddExpressionForIn(DbBinaryExpression e, KeyToListMap<DbExpression, DbExpression> values)
        {
            if (IsKeyForIn(e.Left))
            {
                values.Add(e.Left, e.Right);
                return true;
            }
            else if (IsKeyForIn(e.Right))
            {
                values.Add(e.Right, e.Left);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to build a KeyToListMap containing valid references and the appropriate value equality
        /// tests associated with each so that they can be optimized into IN clauses. Calls itself recursively
        /// to consider additional OR branches.
        /// </summary>
        /// <param name="e">DbExpression representing the branch to evaluate</param>
        /// <param name="values">KeyToListMap to which to add references and value equality tests encountered</param>
        /// <returns>True if this branch contained just equality tests or further OR branches, false otherwise</returns>
        private bool HasBuiltMapForIn(DbExpression e, KeyToListMap<DbExpression, DbExpression> values)
        {
            switch(e.ExpressionKind)
            {
                case DbExpressionKind.Equals:
                    {
                        return TryAddExpressionForIn((DbBinaryExpression)e, values);
                    }
                case DbExpressionKind.IsNull:
                    {
                        var potentialKey = ((DbIsNullExpression)e).Argument;
                        if (IsKeyForIn(potentialKey))
                        {
                            values.Add(potentialKey, e);
                            return true;
                        }
                        return false;
                    }
                case DbExpressionKind.Or:
                    {
                        var comparisonExpression = e as DbBinaryExpression;
                        return HasBuiltMapForIn(comparisonExpression.Left, values) && HasBuiltMapForIn(comparisonExpression.Right, values);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        #endregion

        /// <summary>
        /// This method handles the DBParameterReference expressions. If the parameter is in 
        /// a part of the tree, which matches our criteria for forcing to non-unicode, then
        /// we add it to the list of candidate parameters. If the parameter occurs in a different
        /// usage scenario, then disqualify it from being forced to non-unicode.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbParameterReferenceExpression e)
        {    
            // Update the dictionary value only if we are not inside a DbIsNullExpression.
            if (!_ignoreForceNonUnicodeFlag)
            {
                if (!_forceNonUnicode)
                {
                    //This parameter is being used in a different way than in the force non-unicode pattern. So disqualify it.
                    _candidateParametersToForceNonUnicode[e.ParameterName] = false;
                }
                else if (!_candidateParametersToForceNonUnicode.ContainsKey(e.ParameterName))
                {
                    //This parameter matches our criteria for forcing to non-unicode. So add to dictionary
                    _candidateParametersToForceNonUnicode[e.ParameterName] = true;
                }
            }

            SqlBuilder result = new SqlBuilder();
            // Do not quote this name.
            // ISSUE: We are not checking that e.Name has no illegal characters. e.g. space
            result.Append("@" + e.ParameterName);

            return result;
        }

        /// <summary>
        /// <see cref="Visit(DbFilterExpression)"/> for the general ideas.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        /// <seealso cref="Visit(DbFilterExpression)"/>
        public override ISqlFragment Visit(DbProjectExpression e)
        {
            Symbol fromSymbol;
            SqlSelectStatement result = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            //#444002 Aliases need renaming only for Sql8 when there is Order By 
            bool aliasesNeedRenaming = false;

            // Project is compatible with Filter
            // but not with Project, GroupBy
            if (!IsCompatible(result, e.ExpressionKind))
            {
                result = CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }
            else if ((this.SqlVersion == SqlVersion.Sql8) && !result.OrderBy.IsEmpty)
            {
                aliasesNeedRenaming = true;
            }

            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, e.Input.VariableName, fromSymbol);

            // Project is the only node that can have DbNewInstanceExpression as a child
            // so we have to check it here.
            // We call VisitNewInstanceExpression instead of Visit(DbNewInstanceExpression), since
            // the latter throws.
            DbNewInstanceExpression newInstanceExpression = e.Projection as DbNewInstanceExpression;
            if (newInstanceExpression != null)
            {
                Dictionary<string, Symbol> newColumns;
                result.Select.Append(VisitNewInstanceExpression(newInstanceExpression, aliasesNeedRenaming, out newColumns));
                if (aliasesNeedRenaming)
                {
                    result.OutputColumnsRenamed = true;
                }
                result.OutputColumns = newColumns;
            }
            else
            {
                result.Select.Append(e.Projection.Accept(this));
            }

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        /// <summary>
        /// This method handles record flattening, which works as follows.
        /// consider an expression <c>Prop(y, Prop(x, Prop(d, Prop(c, Prop(b, Var(a)))))</c>
        /// where a,b,c are joins, d is an extent and x and y are fields.
        /// b has been flattened into a, and has its own SELECT statement.
        /// c has been flattened into b.
        /// d has been flattened into c.
        ///
        /// We visit the instance, so we reach Var(a) first.  This gives us a (join)symbol.
        /// Symbol(a).b gives us a join symbol, with a SELECT statement i.e. Symbol(b).
        /// From this point on , we need to remember Symbol(b) as the source alias,
        /// and then try to find the column.  So, we use a SymbolPair.
        ///
        /// We have reached the end when the symbol no longer points to a join symbol.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="JoinSymbol"/> if we have not reached the first
        /// Join node that has a SELECT statement.
        /// A <see cref="SymbolPair"/> if we have seen the JoinNode, and it has
        /// a SELECT statement.
        /// A <see cref="SqlBuilder"/> with {Input}.propertyName otherwise.
        /// </returns>
        public override ISqlFragment Visit(DbPropertyExpression e)
        {
            SqlBuilder result;

            ISqlFragment instanceSql = e.Instance.Accept(this);

            // Since the DbVariableReferenceExpression is a proper child of ours, we can reset
            // isVarSingle.
            DbVariableReferenceExpression VariableReferenceExpression = e.Instance as DbVariableReferenceExpression;
            if (VariableReferenceExpression != null)
            {
                isVarRefSingle = false;
            }

            // We need to flatten, and have not yet seen the first nested SELECT statement.
            JoinSymbol joinSymbol = instanceSql as JoinSymbol;
            if (joinSymbol != null)
            {
                Debug.Assert(joinSymbol.NameToExtent.ContainsKey(e.Property.Name));
                if (joinSymbol.IsNestedJoin)
                {
                    return new SymbolPair(joinSymbol, joinSymbol.NameToExtent[e.Property.Name]);
                }
                else
                {
                    return joinSymbol.NameToExtent[e.Property.Name];
                }
            }

            // ---------------------------------------
            // We have seen the first nested SELECT statement, but not the column.
            SymbolPair symbolPair = instanceSql as SymbolPair;
            if (symbolPair != null)
            {
                JoinSymbol columnJoinSymbol = symbolPair.Column as JoinSymbol;
                if (columnJoinSymbol != null)
                {
                    symbolPair.Column = columnJoinSymbol.NameToExtent[e.Property.Name];
                    return symbolPair;
                }
                else
                {
                    // symbolPair.Column has the base extent.
                    // we need the symbol for the column, since it might have been renamed
                    // when handling a JOIN.
                    if (symbolPair.Column.Columns.ContainsKey(e.Property.Name))
                    {
                        result = new SqlBuilder();
                        result.Append(symbolPair.Source);
                        result.Append(".");
                        Symbol columnSymbol = symbolPair.Column.Columns[e.Property.Name];
                        optionalColumnUsageManager.MarkAsUsed(columnSymbol);
                        result.Append(columnSymbol);
                        return result;
                    }
                }
            }
            // ---------------------------------------

            result = new SqlBuilder();
            result.Append(instanceSql);
            result.Append(".");

            Symbol symbol = instanceSql as Symbol;
            Symbol colSymbol;
            if (symbol != null && symbol.OutputColumns.TryGetValue(e.Property.Name, out colSymbol))
            {
                optionalColumnUsageManager.MarkAsUsed(colSymbol);
                if (symbol.OutputColumnsRenamed)
                {
                    result.Append(colSymbol);
                }
                else
                {
                    result.Append(QuoteIdentifier(e.Property.Name));
                }
            }
            else
            {
                // At this point the column name cannot be renamed, so we do
                // not use a symbol.
                result.Append(QuoteIdentifier(e.Property.Name));
            }
            return result;
        }

        /// <summary>
        /// Any(input, x) => Exists(Filter(input,x))
        /// All(input, x) => Not Exists(Filter(input, not(x))
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbQuantifierExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            bool negatePredicate = (e.ExpressionKind == DbExpressionKind.All);
            if (e.ExpressionKind == DbExpressionKind.Any)
            {
                result.Append("EXISTS (");
            }
            else
            {
                Debug.Assert(e.ExpressionKind == DbExpressionKind.All);
                result.Append("NOT EXISTS (");
            }

            SqlSelectStatement filter = VisitFilterExpression(e.Input, e.Predicate, negatePredicate);
            if (filter.Select.IsEmpty)
            {
                AddDefaultColumns(filter);
            }

            result.Append(filter);
            result.Append(")");

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbRefExpression e)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbRelationshipNavigationExpression e)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// For Sql9 it translates to:
        /// SELECT Y.x1, Y.x2, ..., Y.xn
        /// FROM (
        ///     SELECT X.x1, X.x2, ..., X.xn, row_number() OVER (ORDER BY sk1, sk2, ...) AS [row_number] 
        ///     FROM input as X 
        ///     ) as Y
        /// WHERE Y.[row_number] > count 
        /// ORDER BY sk1, sk2, ...
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbSkipExpression e)
        {
            Debug.Assert(this.SqlVersion != SqlVersion.Sql8, "DbSkipExpression when translating for SQL Server 2000.");

            Debug.Assert(e.Count is DbConstantExpression || e.Count is DbParameterReferenceExpression, "DbSkipExpression.Count is of invalid expression type");

            //Visit the input
            Symbol fromSymbol;
            SqlSelectStatement input = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // Skip is not compatible with anything that OrderBy is not compatible with, as well as with distinct
            if (!IsCompatible(input, e.ExpressionKind))
            {
                input = CreateNewSelectStatement(input, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(input);
            symbolTable.EnterScope();

            AddFromSymbol(input, e.Input.VariableName, fromSymbol);

            //Add the default columns
            Debug.Assert(input.Select.IsEmpty);
            List<Symbol> inputColumns = AddDefaultColumns(input);

            input.Select.Append("row_number() OVER (ORDER BY ");
            AddSortKeys(input.Select, e.SortOrder);
            input.Select.Append(") AS ");

            string row_numberName = "row_number";
            Symbol row_numberSymbol = new Symbol(row_numberName, IntegerType);
            if (inputColumns.Any(c => String.Equals(c.Name, row_numberName, StringComparison.OrdinalIgnoreCase)))
            {
                row_numberSymbol.NeedsRenaming = true;
            }

            input.Select.Append(row_numberSymbol);

            //The inner statement is complete, its scopes need not be valid any longer
            symbolTable.ExitScope();
            selectStatementStack.Pop();

            //Create the resulting statement 
            //See CreateNewSelectStatement, it is very similar
            //Future Enhancement (Microsoft): Refactor to avoid duplication with CreateNewSelectStatement if we 
            // don't switch to using ExtensionExpression here
            SqlSelectStatement result = new SqlSelectStatement();
            result.From.Append("( ");
            result.From.Append(input);
            result.From.AppendLine();
            result.From.Append(") ");

            //Create a symbol for the input
            Symbol resultFromSymbol = null;
 
            if (input.FromExtents.Count == 1)
            {
                JoinSymbol oldJoinSymbol = input.FromExtents[0] as JoinSymbol;
                if (oldJoinSymbol != null)
                {
                    // Note: input.FromExtents will not do, since it might
                    // just be an alias of joinSymbol, and we want an actual JoinSymbol.
                    JoinSymbol newJoinSymbol = new JoinSymbol(e.Input.VariableName, e.Input.VariableType, oldJoinSymbol.ExtentList);
                    // This indicates that the oldStatement is a blocking scope
                    // i.e. it hides/renames extent columns
                    newJoinSymbol.IsNestedJoin = true;
                    newJoinSymbol.ColumnList = inputColumns;
                    newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;

                    resultFromSymbol = newJoinSymbol;
                }
            }

            if (resultFromSymbol == null)
            {
                // This is just a simple extent/SqlSelectStatement,
                // and we can get the column list from the type.
                resultFromSymbol = new Symbol(e.Input.VariableName, e.Input.VariableType, input.OutputColumns, false);
            }
            //Add the ORDER BY part
            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, e.Input.VariableName, resultFromSymbol);

            //Add the predicate 
            result.Where.Append(resultFromSymbol);
            result.Where.Append(".");
            result.Where.Append(row_numberSymbol);
            result.Where.Append(" > ");
            result.Where.Append(HandleCountExpression(e.Count));

            AddSortKeys(result.OrderBy, e.SortOrder);
            
            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        /// <summary>
        /// <see cref="Visit(DbFilterExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        /// <seealso cref="Visit(DbFilterExpression)"/>
        public override ISqlFragment Visit(DbSortExpression e)
        {
            Symbol fromSymbol;
            SqlSelectStatement result = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // OrderBy is compatible with Filter
            // and nothing else
            if (!IsCompatible(result, e.ExpressionKind))
            {
                result = CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, e.Input.VariableName, fromSymbol);

            AddSortKeys(result.OrderBy, e.SortOrder);

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbTreatExpression e)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// This code is shared by <see cref="Visit(DbExceptExpression)"/>
        /// and <see cref="Visit(DbIntersectExpression)"/>
        ///
        /// <see cref="VisitSetOpExpression"/>
        /// Since the left and right expression may not be Sql select statements,
        /// we must wrap them up to look like SQL select statements.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbUnionAllExpression e)
        {
            return VisitSetOpExpression(e.Left, e.Right, "UNION ALL");
        }

        /// <summary>
        /// This method determines whether an extent from an outer scope(free variable)
        /// is used in the CurrentSelectStatement.
        ///
        /// An extent in an outer scope, if its symbol is not in the FromExtents
        /// of the CurrentSelectStatement.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="Symbol"/>.</returns>
        public override ISqlFragment Visit(DbVariableReferenceExpression e)
        {
            if (isVarRefSingle)
            {
                throw EntityUtil.NotSupported();
                // A DbVariableReferenceExpression has to be a child of DbPropertyExpression or MethodExpression
                // This is also checked in GenerateSql(...) at the end of the visiting.
            }
            isVarRefSingle = true; // This will be reset by DbPropertyExpression or MethodExpression

            Symbol result = symbolTable.Lookup(e.VariableName);
            optionalColumnUsageManager.MarkAsUsed(result);
            if (!CurrentSelectStatement.FromExtents.Contains(result))
            {
                CurrentSelectStatement.OuterExtents[result] = true;
            }

            return result;
        }
        #endregion

        #region Visitor Helper Methods

        #region 'Visitor' methods - Shared visitors and methods that do most of the visiting

        /// <summary>
        /// Aggregates are not visited by the normal visitor walk.
        /// </summary>
        /// <param name="aggregate">The aggregate go be translated</param>
        /// <param name="aggregateArgument">The translated aggregate argument</param>
        /// <returns></returns>
        private static SqlBuilder VisitAggregate(DbAggregate aggregate, object aggregateArgument)
        {
            SqlBuilder aggregateResult = new SqlBuilder();
            DbFunctionAggregate functionAggregate = aggregate as DbFunctionAggregate;

            if (functionAggregate == null)
            {
                throw EntityUtil.NotSupported();
            }

            //The only aggregate function with different name is Big_Count
            //Note: If another such function is to be added, a dictionary should be created
            if (TypeHelpers.IsCanonicalFunction(functionAggregate.Function)
                && String.Equals(functionAggregate.Function.Name, "BigCount", StringComparison.Ordinal))
            {
                aggregateResult.Append("COUNT_BIG");
            }
            else
            {
                SqlFunctionCallHandler.WriteFunctionName(aggregateResult, functionAggregate.Function);
            }

            aggregateResult.Append("(");

            DbFunctionAggregate fnAggr = functionAggregate;
            if ((null != fnAggr) && (fnAggr.Distinct))
            {
                aggregateResult.Append("DISTINCT ");
            }

            aggregateResult.Append(aggregateArgument);

            aggregateResult.Append(")");
            return aggregateResult;
        }

        /// <summary>
        /// Dump out an expression - optionally wrap it with parantheses if possible
        /// </summary>
        /// <param name="e"></param>
        /// <param name="result"></param>
        internal void ParenthesizeExpressionIfNeeded(DbExpression e, SqlBuilder result)
        {
            if (IsComplexExpression(e))
            {
                result.Append("(");
                result.Append(e.Accept(this));
                result.Append(")");
            }
            else
            {
                result.Append(e.Accept(this));
            }
        }

        /// <summary>
        /// Handler for inline binary expressions.
        /// Produces left op right. 
        /// For associative operations does flattening. 
        /// Puts parenthesis around the arguments if needed.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="expressionKind"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private SqlBuilder VisitBinaryExpression(string op, DbExpressionKind expressionKind, DbExpression left, DbExpression right)
        {
            SqlBuilder result = new SqlBuilder();

            bool isFirst = true;
            foreach (DbExpression argument in SqlGenerator.FlattenAssociativeExpression(expressionKind, left, right))
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.Append(op);
                }
                ParenthesizeExpressionIfNeeded(argument, result);
            }
            return result;
        }

        /// <summary>
        /// Creates a flat list of the associative arguments.
        /// For example, for ((A1 + (A2 - A3)) + A4) it will create A1, (A2 - A3), A4
        /// Only 'unfolds' the given arguments that are of the given expression kind.        
        /// </summary>
        /// <param name="expressionKind"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static IEnumerable<DbExpression> FlattenAssociativeExpression(DbExpressionKind kind, DbExpression left, DbExpression right)
        {
            if(kind != DbExpressionKind.Or &&
               kind != DbExpressionKind.And &&
               kind != DbExpressionKind.Plus &&
               kind != DbExpressionKind.Multiply)
            {
                return new[] { left, right };
            }             

            List<DbExpression> argumentList = new List<DbExpression>();
            ExtractAssociativeArguments(kind, argumentList, left);
            ExtractAssociativeArguments(kind, argumentList, right);

            return argumentList;
        }

        /// <summary>
        /// Helper method for FlattenAssociativeExpression.
        /// Creates a flat list of the associative arguments and appends to the given argument list.
        /// For example, for ((A1 + (A2 - A3)) + A4) it will add A1, (A2 - A3), A4 to the list.
        /// Only 'unfolds' the given expression if it is of the given expression kind.
        /// </summary>
        /// <param name="expressionKind"></param>
        /// <param name="argumentList"></param>
        /// <param name="expression"></param>
        private static void ExtractAssociativeArguments(DbExpressionKind expressionKind, List<DbExpression> argumentList, DbExpression expression)
        {
            IEnumerable<DbExpression> result = Helpers.GetLeafNodes(
                expression, 
                exp => exp.ExpressionKind != expressionKind,
                exp =>
                {
                    //All associative expressions are binary, thus we must be dealing with a DbBinaryExpresson or 
                    // a DbArithmeticExpression with 2 arguments.
                    DbBinaryExpression binaryExpression = exp as DbBinaryExpression;
                    if(binaryExpression != null)
                    {
                        return new[] { binaryExpression.Left, binaryExpression.Right };
                    }
                    DbArithmeticExpression arithmeticExpression = (DbArithmeticExpression)exp;
                    return arithmeticExpression.Arguments;
                }
            );

            argumentList.AddRange(result);
        }
        
        /// <summary>
        /// Private handler for comparison expressions - almost identical to VisitBinaryExpression.
        /// We special case constants, so that we don't emit unnecessary casts
        /// </summary>
        /// <param name="op">the comparison op</param>
        /// <param name="left">the left-side expression</param>
        /// <param name="right">the right-side expression</param>
        /// <returns></returns>
        private SqlBuilder VisitComparisonExpression(string op, DbExpression left, DbExpression right)
        {
            SqlBuilder result = new SqlBuilder();

            bool isCastOptional = left.ResultType.EdmType == right.ResultType.EdmType;

            if (left.ExpressionKind == DbExpressionKind.Constant)
            {
                result.Append(VisitConstant((DbConstantExpression)left, isCastOptional));
            }
            else
            {
                ParenthesizeExpressionIfNeeded(left, result);
            }

            result.Append(op);

            if (right.ExpressionKind == DbExpressionKind.Constant)
            {
                result.Append(VisitConstant((DbConstantExpression)right, isCastOptional));
            }
            else
            {
                ParenthesizeExpressionIfNeeded(right, result);
            }

            return result;
        }

        /// <summary>
        /// This is called by the relational nodes.  It does the following
        /// <list>
        /// <item>If the input is not a SqlSelectStatement, it assumes that the input
        /// is a collection expression, and creates a new SqlSelectStatement </item>
        /// </list>
        /// </summary>
        /// <param name="inputExpression"></param>
        /// <param name="inputVarName"></param>
        /// <param name="inputVarType"></param>
        /// <param name="fromSymbol"></param>
        /// <returns>A <see cref="SqlSelectStatement"/> and the main fromSymbol
        /// for this select statement.</returns>
        private SqlSelectStatement VisitInputExpression(DbExpression inputExpression,
            string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
        {
            SqlSelectStatement result;
            ISqlFragment sqlFragment = inputExpression.Accept(this);
            result = sqlFragment as SqlSelectStatement;

            if (result == null)
            {
                result = new SqlSelectStatement();
                WrapNonQueryExtent(result, sqlFragment, inputExpression.ExpressionKind);
            }

            if (result.FromExtents.Count == 0)
            {
                // input was an extent
                fromSymbol = new Symbol(inputVarName, inputVarType);
            }
            else if (result.FromExtents.Count == 1)
            {
                // input was Filter/GroupBy/Project/OrderBy
                // we are likely to reuse this statement.
                fromSymbol = result.FromExtents[0];
            }
            else
            {
                // input was a join.
                // we are reusing the select statement produced by a Join node
                // we need to remove the original extents, and replace them with a
                // new extent with just the Join symbol.
                JoinSymbol joinSymbol = new JoinSymbol(inputVarName, inputVarType, result.FromExtents);
                joinSymbol.FlattenedExtentList = result.AllJoinExtents;

                fromSymbol = joinSymbol;
                result.FromExtents.Clear();
                result.FromExtents.Add(fromSymbol);
            }

            return result;
        }

        /// <summary>
        /// <see cref="Visit(DbIsEmptyExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <param name="negate">Was the parent a DbNotExpression?</param>
        /// <returns></returns>
        SqlBuilder VisitIsEmptyExpression(DbIsEmptyExpression e, bool negate)
        {
            SqlBuilder result = new SqlBuilder();
            if (!negate)
            {
                result.Append(" NOT");
            }
            result.Append(" EXISTS (");
            result.Append(VisitExpressionEnsureSqlStatement(e.Argument));
            result.AppendLine();
            result.Append(")");

            return result;
        }


        /// <summary>
        /// Translate a NewInstance(Element(X)) expression into
        ///   "select top(1) * from X"
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private ISqlFragment VisitCollectionConstructor(DbNewInstanceExpression e)
        {
            Debug.Assert(e.Arguments.Count <= 1);

            if (e.Arguments.Count == 1 && e.Arguments[0].ExpressionKind == DbExpressionKind.Element)
            {
                DbElementExpression elementExpr = e.Arguments[0] as DbElementExpression;
                SqlSelectStatement result = VisitExpressionEnsureSqlStatement(elementExpr.Argument);

                if (!IsCompatible(result, DbExpressionKind.Element))
                {
                    Symbol fromSymbol;
                    TypeUsage inputType = TypeHelpers.GetElementTypeUsage(elementExpr.Argument.ResultType);

                    result = CreateNewSelectStatement(result, "element", inputType, out fromSymbol);
                    AddFromSymbol(result, "element", fromSymbol, false);
                }
                result.Select.Top = new TopClause(1, false);
                return result;
            }


            // Otherwise simply build this out as a union-all ladder
            CollectionType collectionType = TypeHelpers.GetEdmType<CollectionType>(e.ResultType);
            Debug.Assert(collectionType != null);
            bool isScalarElement = TypeSemantics.IsPrimitiveType(collectionType.TypeUsage);

            SqlBuilder resultSql = new SqlBuilder();
            string separator = "";

            // handle empty table
            if (e.Arguments.Count == 0)
            {
                Debug.Assert(isScalarElement);
                resultSql.Append(" SELECT CAST(null as ");
                resultSql.Append(GetSqlPrimitiveType(collectionType.TypeUsage));
                resultSql.Append(") AS X FROM (SELECT 1) AS Y WHERE 1=0");
            }

            foreach (DbExpression arg in e.Arguments)
            {
                resultSql.Append(separator);
                resultSql.Append(" SELECT ");
                resultSql.Append(arg.Accept(this));
                // For scalar elements, no alias is appended yet. Add this.
                if (isScalarElement)
                {
                    resultSql.Append(" AS X ");
                }
                separator = " UNION ALL ";
            }

            return resultSql;
        }

        /// <summary>
        /// <see cref="Visit(DbIsNullExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <param name="negate">Was the parent a DbNotExpression?</param>
        /// <returns></returns>
        private SqlBuilder VisitIsNullExpression(DbIsNullExpression e, bool negate)
        {
            SqlBuilder result = new SqlBuilder();
            if (e.Argument.ExpressionKind == DbExpressionKind.ParameterReference)
            {
                _ignoreForceNonUnicodeFlag = true;
            }
            result.Append(e.Argument.Accept(this));
            // reset flag, it is not possible to reach this function with this flag set to true.
            _ignoreForceNonUnicodeFlag = false;
            if (!negate)
            {
                result.Append(" IS NULL");
            }
            else
            {
                result.Append(" IS NOT NULL");
            }

            return result;
        }

        /// <summary>
        /// This handles the processing of join expressions.
        /// The extents on a left spine are flattened, while joins
        /// not on the left spine give rise to new nested sub queries.
        ///
        /// Joins work differently from the rest of the visiting, in that
        /// the parent (i.e. the join node) creates the SqlSelectStatement
        /// for the children to use.
        ///
        /// The "parameter" IsInJoinContext indicates whether a child extent should
        /// add its stuff to the existing SqlSelectStatement, or create a new SqlSelectStatement
        /// By passing true, we ask the children to add themselves to the parent join,
        /// by passing false, we ask the children to create new Select statements for
        /// themselves.
        ///
        /// This method is called from <see cref="Visit(DbApplyExpression)"/> and
        /// <see cref="Visit(DbJoinExpression)"/>.
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="joinKind"></param>
        /// <param name="joinString"></param>
        /// <param name="joinCondition"></param>
        /// <returns> A <see cref="SqlSelectStatement"/></returns>
        private ISqlFragment VisitJoinExpression(IList<DbExpressionBinding> inputs, DbExpressionKind joinKind,
            string joinString, DbExpression joinCondition)
        {
            SqlSelectStatement result;
            // If the parent is not a join( or says that it is not),
            // we should create a new SqlSelectStatement.
            // otherwise, we add our child extents to the parent's FROM clause.
            if (!IsParentAJoin)
            {
                result = new SqlSelectStatement();
                result.AllJoinExtents = new List<Symbol>();
                selectStatementStack.Push(result);
            }
            else
            {
                result = CurrentSelectStatement;
            }

            // Process each of the inputs, and then the joinCondition if it exists.
            // It would be nice if we could call VisitInputExpression - that would
            // avoid some code duplication
            // but the Join postprocessing is messy and prevents this reuse.
            symbolTable.EnterScope();

            string separator = "";
            bool isLeftMostInput = true;
            int inputCount = inputs.Count;
            for(int idx = 0; idx < inputCount; idx++)
            {
                DbExpressionBinding input = inputs[idx];

                if (separator.Length != 0)
                {
                    result.From.AppendLine();
                }
                result.From.Append(separator + " ");
                // Change this if other conditions are required
                // to force the child to produce a nested SqlStatement.
                bool needsJoinContext = (input.Expression.ExpressionKind == DbExpressionKind.Scan)
                                        || (isLeftMostInput &&
                                            (IsJoinExpression(input.Expression)
                                             || IsApplyExpression(input.Expression)))
                                        ;

                isParentAJoinStack.Push(needsJoinContext ? true : false);
                // if the child reuses our select statement, it will append the from
                // symbols to our FromExtents list.  So, we need to remember the
                // start of the child's entries.
                int fromSymbolStart = result.FromExtents.Count;

                ISqlFragment fromExtentFragment = input.Expression.Accept(this);

                isParentAJoinStack.Pop();

                ProcessJoinInputResult(fromExtentFragment, result, input, fromSymbolStart);
                separator = joinString;

                isLeftMostInput = false;
            }

            // Visit the on clause/join condition.
            switch (joinKind)
            {
                case DbExpressionKind.FullOuterJoin:
                case DbExpressionKind.InnerJoin:
                case DbExpressionKind.LeftOuterJoin:
                    result.From.Append(" ON ");
                    isParentAJoinStack.Push(false);
                    result.From.Append(joinCondition.Accept(this));
                    isParentAJoinStack.Pop();
                    break;
            }

            symbolTable.ExitScope();

            if (!IsParentAJoin)
            {
                selectStatementStack.Pop();
            }

            return result;
        }

        /// <summary>
        /// This is called from <see cref="VisitJoinExpression"/>.
        ///
        /// This is responsible for maintaining the symbol table after visiting
        /// a child of a join expression.
        ///
        /// The child's sql statement may need to be completed.
        ///
        /// The child's result could be one of
        /// <list type="number">
        /// <item>The same as the parent's - this is treated specially.</item>
        /// <item>A sql select statement, which may need to be completed</item>
        /// <item>An extent - just copy it to the from clause</item>
        /// <item>Anything else (from a collection-valued expression) -
        /// unnest and copy it.</item>
        /// </list>
        ///
        /// If the input was a Join, we need to create a new join symbol,
        /// otherwise, we create a normal symbol.
        ///
        /// We then call AddFromSymbol to add the AS clause, and update the symbol table.
        ///
        ///
        ///
        /// If the child's result was the same as the parent's, we have to clean up
        /// the list of symbols in the FromExtents list, since this contains symbols from
        /// the children of both the parent and the child.
        /// The happens when the child visited is a Join, and is the leftmost child of
        /// the parent.
        /// </summary>
        /// <param name="fromExtentFragment"></param>
        /// <param name="result"></param>
        /// <param name="input"></param>
        /// <param name="fromSymbolStart"></param>
        private void ProcessJoinInputResult(ISqlFragment fromExtentFragment, SqlSelectStatement result,
            DbExpressionBinding input, int fromSymbolStart)
        {
            Symbol fromSymbol = null;

            if (result != fromExtentFragment)
            {
                // The child has its own select statement, and is not reusing
                // our select statement.
                // This should look a lot like VisitInputExpression().
                SqlSelectStatement sqlSelectStatement = fromExtentFragment as SqlSelectStatement;
                if (sqlSelectStatement != null)
                {
                    if (sqlSelectStatement.Select.IsEmpty)
                    {
                        List<Symbol> columns = AddDefaultColumns(sqlSelectStatement);

                        if (IsJoinExpression(input.Expression)
                            || IsApplyExpression(input.Expression))
                        {
                            List<Symbol> extents = sqlSelectStatement.FromExtents;
                            JoinSymbol newJoinSymbol = new JoinSymbol(input.VariableName, input.VariableType, extents);
                            newJoinSymbol.IsNestedJoin = true;
                            newJoinSymbol.ColumnList = columns;

                            fromSymbol = newJoinSymbol;
                        }
                        else
                        {
                            // this is a copy of the code in CreateNewSelectStatement.

                            // if the oldStatement has a join as its input, ...
                            // clone the join symbol, so that we "reuse" the
                            // join symbol.  Normally, we create a new symbol - see the next block
                            // of code.
                            JoinSymbol oldJoinSymbol = sqlSelectStatement.FromExtents[0] as JoinSymbol;
                            if (oldJoinSymbol != null)
                            {
                                // Note: sqlSelectStatement.FromExtents will not do, since it might
                                // just be an alias of joinSymbol, and we want an actual JoinSymbol.
                                JoinSymbol newJoinSymbol = new JoinSymbol(input.VariableName, input.VariableType, oldJoinSymbol.ExtentList);
                                // This indicates that the sqlSelectStatement is a blocking scope
                                // i.e. it hides/renames extent columns
                                newJoinSymbol.IsNestedJoin = true;
                                newJoinSymbol.ColumnList = columns;
                                newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;

                                fromSymbol = newJoinSymbol;
                            }
                            else
                            {
                                fromSymbol = new Symbol(input.VariableName, input.VariableType, sqlSelectStatement.OutputColumns, sqlSelectStatement.OutputColumnsRenamed);
                            }
                        }
                    }
                    else
                    {
                        fromSymbol = new Symbol(input.VariableName, input.VariableType, sqlSelectStatement.OutputColumns, sqlSelectStatement.OutputColumnsRenamed);
                    }
                    result.From.Append(" (");
                    result.From.Append(sqlSelectStatement);
                    result.From.Append(" )");
                }
                else if (input.Expression is DbScanExpression)
                {
                    result.From.Append(fromExtentFragment);
                }
                else // bracket it
                {
                    WrapNonQueryExtent(result, fromExtentFragment, input.Expression.ExpressionKind);
                }

                if (fromSymbol == null) // i.e. not a join symbol
                {
                    fromSymbol = new Symbol(input.VariableName, input.VariableType);
                }


                AddFromSymbol(result, input.VariableName, fromSymbol);
                result.AllJoinExtents.Add(fromSymbol);
            }
            else // result == fromExtentFragment.  The child extents have been merged into the parent's.
            {
                // we are adding extents to the current sql statement via flattening.
                // We are replacing the child's extents with a single Join symbol.
                // The child's extents are all those following the index fromSymbolStart.
                //
                List<Symbol> extents = new List<Symbol>();

                // We cannot call extents.AddRange, since the is no simple way to
                // get the range of symbols fromSymbolStart..result.FromExtents.Count
                // from result.FromExtents.
                // We copy these symbols to create the JoinSymbol later.
                for (int i = fromSymbolStart; i < result.FromExtents.Count; ++i)
                {
                    extents.Add(result.FromExtents[i]);
                }
                result.FromExtents.RemoveRange(fromSymbolStart, result.FromExtents.Count - fromSymbolStart);
                fromSymbol = new JoinSymbol(input.VariableName, input.VariableType, extents);
                result.FromExtents.Add(fromSymbol);
                // this Join Symbol does not have its own select statement, so we
                // do not set IsNestedJoin


                // We do not call AddFromSymbol(), since we do not want to add
                // "AS alias" to the FROM clause- it has been done when the extent was added earlier.
                symbolTable.Add(input.VariableName, fromSymbol);
            }
        }

        /// <summary>
        /// We assume that this is only called as a child of a Project.
        ///
        /// This replaces <see cref="Visit(DbNewInstanceExpression)"/>, since
        /// we do not allow DbNewInstanceExpression as a child of any node other than
        /// DbProjectExpression.
        ///
        /// We write out the translation of each of the columns in the record.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="aliasesNeedRenaming"></param>
        /// <param name="newColumns"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        private ISqlFragment VisitNewInstanceExpression(DbNewInstanceExpression e, bool aliasesNeedRenaming, out Dictionary<string, Symbol> newColumns)
        {
            SqlBuilder result = new SqlBuilder();
            RowType rowType = e.ResultType.EdmType as RowType;

            if (null != rowType)
            {
                newColumns = new Dictionary<string, Symbol>(e.Arguments.Count);

                ReadOnlyMetadataCollection<EdmProperty> members = rowType.Properties;
                string separator = "";
                for(int i = 0; i < e.Arguments.Count; ++i)
                {
                    DbExpression argument = e.Arguments[i];
                    if (TypeSemantics.IsRowType(argument.ResultType))
                    {
                        // We do not support nested records or other complex objects.
                        throw EntityUtil.NotSupported();
                    }

                    EdmProperty member = members[i];
                    result.Append(separator);
                    result.AppendLine();
                    result.Append(argument.Accept(this));
                    result.Append(" AS ");
                    if (aliasesNeedRenaming)
                    {
                        Symbol column = new Symbol(member.Name, member.TypeUsage);
                        column.NeedsRenaming = true;
                        column.NewName = String.Concat("Internal_", member.Name);
                        result.Append(column);
                        newColumns.Add(member.Name, column);
                    }
                    else
                    {
                        result.Append(QuoteIdentifier(member.Name));
                    }
                    separator = ", ";
                }
            }
            else
            {
                //
                // 


                throw EntityUtil.NotSupported();
            }

            return result;
        }

        /// <summary>
        /// Handler for set operations
        /// It generates left separator right.
        /// Only for SQL 8.0 it may need to create a new select statement 
        /// above the set operation if the left child's output columns got renamed
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        private ISqlFragment VisitSetOpExpression(DbExpression left, DbExpression right, string separator)
        {

            SqlSelectStatement leftSelectStatement = VisitExpressionEnsureSqlStatement(left, true, true);
            SqlSelectStatement rightSelectStatement = VisitExpressionEnsureSqlStatement(right, true, true);

            SqlBuilder setStatement = new SqlBuilder();
            setStatement.Append(leftSelectStatement);
            setStatement.AppendLine();
            setStatement.Append(separator); // e.g. UNION ALL
            setStatement.AppendLine();
            setStatement.Append(rightSelectStatement);


            //This is the common scenario
            if (!leftSelectStatement.OutputColumnsRenamed)
            {
                return setStatement;
            }

            else
            {
                // This is case only for SQL 8.0 when the left child has order by in it
                // If the output columns of the left child got renamed, 
                // then the output of the union all is renamed
                // All this currenlty only happens for UNION ALL, because INTERSECT and
                // EXCEPT get translated for SQL 8.0 before SqlGen.
                SqlSelectStatement selectStatement = new SqlSelectStatement();
                selectStatement.From.Append("( ");
                selectStatement.From.Append(setStatement);
                selectStatement.From.AppendLine();
                selectStatement.From.Append(") ");

                Symbol fromSymbol = new Symbol("X", TypeHelpers.GetElementTypeUsage(left.ResultType), leftSelectStatement.OutputColumns, true);
                AddFromSymbol(selectStatement, null, fromSymbol, false);

                return selectStatement;
            }
        }

        #endregion

        #region Other Helpers
        /// <summary>
        /// <see cref="AddDefaultColumns"/>
        /// Add the column names from the referenced extent/join to the
        /// select statement.
        ///
        /// If the symbol is a JoinSymbol, we recursively visit all the extents,
        /// halting at real extents and JoinSymbols that have an associated SqlSelectStatement.
        ///
        /// The column names for a real extent can be derived from its type.
        /// The column names for a Join Select statement can be got from the
        /// list of columns that was created when the Join's select statement
        /// was created.
        ///
        /// We do the following for each column.
        /// <list type="number">
        /// <item>Add the SQL string for each column to the SELECT clause</item>
        /// <item>Add the column to the list of columns - so that it can
        /// become part of the "type" of a JoinSymbol</item>
        /// <item>Check if the column name collides with a previous column added
        /// to the same select statement.  Flag both the columns for renaming if true.</item>
        /// <item>Add the column to a name lookup dictionary for collision detection.</item>
        /// </list>
        /// </summary>
        /// <param name="selectStatement">The select statement that started off as SELECT *</param>
        /// <param name="symbol">The symbol containing the type information for
        /// the columns to be added.</param>
        /// <param name="columnList">Columns that have been added to the Select statement.
        /// This is created in <see cref="AddDefaultColumns"/>.</param>
        /// <param name="columnDictionary">A dictionary of the columns above.</param>
        private void AddColumns(SqlSelectStatement selectStatement, Symbol symbol,
            List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary)
        {
            JoinSymbol joinSymbol = symbol as JoinSymbol;
            if (joinSymbol != null)
            {
                if (!joinSymbol.IsNestedJoin)
                {
                    // Recurse if the join symbol is a collection of flattened extents
                    foreach (Symbol sym in joinSymbol.ExtentList)
                    {
                        // if sym is ScalarType means we are at base case in the
                        // recursion and there are not columns to add, just skip
                        if ((sym.Type == null) || TypeSemantics.IsPrimitiveType(sym.Type))
                        {
                            continue;
                        }

                        AddColumns(selectStatement, sym, columnList, columnDictionary);
                    }
                }
                else
                {
                    foreach (Symbol joinColumn in joinSymbol.ColumnList)
                    {
                        // we write tableName.columnName
                        // rather than tableName.columnName as alias
                        // since the column name is unique (by the way we generate new column names)
                        //
                        // We use the symbols for both the table and the column,
                        // since they are subject to renaming.

                        //This is called from AddDefaultColumns. To avoid adding columns that may not be used later,
                        // we add an optional column, that will only be added if needed.
                        OptionalColumn optionalColumn = CreateOptionalColumn(null, joinColumn);

                        optionalColumn.Append(symbol);
                        optionalColumn.Append(".");
                        optionalColumn.Append(joinColumn);

                        selectStatement.Select.AddOptionalColumn(optionalColumn);

                        // check for name collisions.  If there is,
                        // flag both the colliding symbols.
                        if (columnDictionary.ContainsKey(joinColumn.Name))
                        {
                            columnDictionary[joinColumn.Name].NeedsRenaming = true; // the original symbol
                            joinColumn.NeedsRenaming = true; // the current symbol.
                        }
                        else
                        {
                            columnDictionary[joinColumn.Name] = joinColumn;
                        }

                        columnList.Add(joinColumn);
                    }
                }
            }
            else
            {
                // This is a non-join extent/select statement, and the CQT type has
                // the relevant column information.

                // The type could be a record type(e.g. Project(...),
                // or an entity type ( e.g. EntityExpression(...)
                // so, we check whether it is a structuralType.

                // Consider an expression of the form J(a, b=P(E))
                // The inner P(E) would have been translated to a SQL statement
                // We should not use the raw names from the type, but the equivalent
                // symbols (they are present in symbol.Columns) if they exist.
                //
                // We add the new columns to the symbol's columns if they do
                // not already exist.
                //
                // If the symbol represents a SqlStatement with renamed output columns,
                // we should use these instead of the rawnames and we should also mark
                // this selectStatement as one with renamed columns

                if (symbol.OutputColumnsRenamed)
                {
                    selectStatement.OutputColumnsRenamed = true;
                }

                if (selectStatement.OutputColumns == null)
                {
                    selectStatement.OutputColumns = new Dictionary<string, Symbol>();
                }

                if ((symbol.Type == null) || TypeSemantics.IsPrimitiveType(symbol.Type))
                {
                    AddColumn(selectStatement, symbol, columnList, columnDictionary, "X");
                }
                else
                {
                    foreach (EdmProperty property in TypeHelpers.GetProperties(symbol.Type))
                    {
                        AddColumn(selectStatement, symbol, columnList, columnDictionary, property.Name);
                    }
               }
            }
        }

        /// <summary>
        /// Creates an optional column and registers the corresponding symbol with 
        /// the optionalColumnUsageManager it has not already been registered.
        /// </summary>
        /// <param name="inputColumnSymbol"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private OptionalColumn CreateOptionalColumn(Symbol inputColumnSymbol, Symbol column)
        {
            if (!optionalColumnUsageManager.ContainsKey(column))
            {
                optionalColumnUsageManager.Add(inputColumnSymbol, column);
            }
            return new OptionalColumn(optionalColumnUsageManager, column);
        }

        /// <summary>
        /// Helper method for AddColumns. Adds a column with the given column name 
        /// to the Select list of the given select statement.
        /// </summary>
        /// <param name="selectStatement">The select statement to whose SELECT part the column should be added</param>
        /// <param name="symbol">The symbol from which the column to be added originated</param>
        /// <param name="columnList">Columns that have been added to the Select statement.
        /// This is created in <see cref="AddDefaultColumns"/>.</param>
        /// <param name="columnDictionary">A dictionary of the columns above.</param>
        /// <param name="columnName">The name of the column to be added.</param>
        private void AddColumn(SqlSelectStatement selectStatement, Symbol symbol, 
            List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary, string columnName)
        {
            // Since all renaming happens in the second phase
            // we lose nothing by setting the next column name index to 0
            // many times.
            allColumnNames[columnName] = 0;

            Symbol inputSymbol = null;
            symbol.OutputColumns.TryGetValue(columnName, out inputSymbol);

            // Create a new symbol/reuse existing symbol for the column
            Symbol columnSymbol;
            if (!symbol.Columns.TryGetValue(columnName, out columnSymbol))
            {
                // we do not care about the types of columns, so we pass null
                // when construction the symbol.
                columnSymbol = ((inputSymbol != null) && symbol.OutputColumnsRenamed)? inputSymbol : new Symbol(columnName, null);
                symbol.Columns.Add(columnName, columnSymbol);
            }

            OptionalColumn optionalColumn = CreateOptionalColumn(inputSymbol, columnSymbol);

            optionalColumn.Append(symbol);
            optionalColumn.Append(".");

            if (symbol.OutputColumnsRenamed)
            {
                optionalColumn.Append(inputSymbol);
            }
            else
            {
                optionalColumn.Append(QuoteIdentifier(columnName));
            }

            optionalColumn.Append(" AS ");
            optionalColumn.Append(columnSymbol);

            selectStatement.Select.AddOptionalColumn(optionalColumn);

            //If the columnName is already in the output columns, it means it is being tracked
            // via the join symbol mechanism
            if (!selectStatement.OutputColumns.ContainsKey(columnName))
            {
                selectStatement.OutputColumns.Add(columnName, columnSymbol);
            }

            // Check for column name collisions.
            if (columnDictionary.ContainsKey(columnName))
            {
                columnDictionary[columnName].NeedsRenaming = true;
                columnSymbol.NeedsRenaming = true;
            }
            else
            {
                columnDictionary[columnName] = symbol.Columns[columnName];
            }

            columnList.Add(columnSymbol);
        }

        /// <summary>
        /// Expands Select * to "select the_list_of_columns"
        /// If the columns are taken from an extent, they are written as
        /// {original_column_name AS Symbol(original_column)} to allow renaming.
        ///
        /// If the columns are taken from a Join, they are written as just
        /// {original_column_name}, since there cannot be a name collision.
        ///
        /// We concatenate the columns from each of the inputs to the select statement.
        /// Since the inputs may be joins that are flattened, we need to recurse.
        /// The inputs are inferred from the symbols in FromExtents.
        /// </summary>
        /// <param name="selectStatement"></param>
        /// <returns></returns>
        private List<Symbol> AddDefaultColumns(SqlSelectStatement selectStatement)
        {
            // This is the list of columns added in this select statement
            // This forms the "type" of the Select statement, if it has to
            // be expanded in another SELECT *
            List<Symbol> columnList = new List<Symbol>();

            // A lookup for the previous set of columns to aid column name
            // collision detection.
            Dictionary<string, Symbol> columnDictionary = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);

            foreach (Symbol symbol in selectStatement.FromExtents)
            {
                AddColumns(selectStatement, symbol, columnList, columnDictionary);
            }

            return columnList;
        }

        /// <summary>
        /// <see cref="AddFromSymbol(SqlSelectStatement, string, Symbol, bool)"/>
        /// </summary>
        /// <param name="selectStatement"></param>
        /// <param name="inputVarName"></param>
        /// <param name="fromSymbol"></param>
        private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol)
        {
            AddFromSymbol(selectStatement, inputVarName, fromSymbol, true);
        }

        /// <summary>
        /// This method is called after the input to a relational node is visited.
        /// <see cref="Visit(DbProjectExpression)"/> and <see cref="ProcessJoinInputResult"/>
        /// There are 2 scenarios
        /// <list type="number">
        /// <item>The fromSymbol is new i.e. the select statement has just been
        /// created, or a join extent has been added.</item>
        /// <item>The fromSymbol is old i.e. we are reusing a select statement.</item>
        /// </list>
        ///
        /// If we are not reusing the select statement, we have to complete the
        /// FROM clause with the alias
        /// <code>
        /// -- if the input was an extent
        /// FROM = [SchemaName].[TableName]
        /// -- if the input was a Project
        /// FROM = (SELECT ... FROM ... WHERE ...)
        /// </code>
        ///
        /// These become
        /// <code>
        /// -- if the input was an extent
        /// FROM = [SchemaName].[TableName] AS alias
        /// -- if the input was a Project
        /// FROM = (SELECT ... FROM ... WHERE ...) AS alias
        /// </code>
        /// and look like valid FROM clauses.
        ///
        /// Finally, we have to add the alias to the global list of aliases used,
        /// and also to the current symbol table.
        /// </summary>
        /// <param name="selectStatement"></param>
        /// <param name="inputVarName">The alias to be used.</param>
        /// <param name="fromSymbol"></param>
        /// <param name="addToSymbolTable"></param>
        private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol, bool addToSymbolTable)
        {
            // the first check is true if this is a new statement
            // the second check is true if we are in a join - we do not
            // check if we are in a join context.
            // We do not want to add "AS alias" if it has been done already
            // e.g. when we are reusing the Sql statement.
            if (selectStatement.FromExtents.Count == 0 || fromSymbol != selectStatement.FromExtents[0])
            {
                selectStatement.FromExtents.Add(fromSymbol);
                selectStatement.From.Append(" AS ");
                selectStatement.From.Append(fromSymbol);

                // We have this inside the if statement, since
                // we only want to add extents that are actually used.
                allExtentNames[fromSymbol.Name] = 0;
            }

            if (addToSymbolTable)
            {
                symbolTable.Add(inputVarName, fromSymbol);
            }
        }

        /// <summary>
        /// Translates a list of SortClauses.
        /// Used in the translation of OrderBy 
        /// </summary>
        /// <param name="orderByClause">The SqlBuilder to which the sort keys should be appended</param>
        /// <param name="sortKeys"></param>
        private void AddSortKeys(SqlBuilder orderByClause, IList<DbSortClause> sortKeys)
        {
            string separator = "";
            foreach (DbSortClause sortClause in sortKeys)
            {
                orderByClause.Append(separator);
                orderByClause.Append(sortClause.Expression.Accept(this));
                // 
                Debug.Assert(sortClause.Collation != null);
                if (!String.IsNullOrEmpty(sortClause.Collation))
                {
                    orderByClause.Append(" COLLATE ");
                    orderByClause.Append(sortClause.Collation);
                }

                orderByClause.Append(sortClause.Ascending ? " ASC" : " DESC");

                separator = ", ";
            }
        }

        /// <summary>
        /// <see cref="CreateNewSelectStatement(SqlSelectStatement, string, TypeUsage, bool, out Symbol)"/>
        /// </summary>
        /// <param name="oldStatement"></param>
        /// <param name="inputVarName"></param>
        /// <param name="inputVarType"></param>
        /// <param name="fromSymbol"></param>
        /// <returns></returns>
        private SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement,
            string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
        {
            return CreateNewSelectStatement(oldStatement, inputVarName, inputVarType, true, out fromSymbol);
        }

        /// <summary>
        /// This is called after a relational node's input has been visited, and the
        /// input's sql statement cannot be reused.  <see cref="Visit(DbProjectExpression)"/>
        ///
        /// When the input's sql statement cannot be reused, we create a new sql
        /// statement, with the old one as the from clause of the new statement.
        ///
        /// The old statement must be completed i.e. if it has an empty select list,
        /// the list of columns must be projected out.
        ///
        /// If the old statement being completed has a join symbol as its from extent,
        /// the new statement must have a clone of the join symbol as its extent.
        /// We cannot reuse the old symbol, but the new select statement must behave
        /// as though it is working over the "join" record.
        /// </summary>
        /// <param name="oldStatement"></param>
        /// <param name="inputVarName"></param>
        /// <param name="inputVarType"></param>
        /// <param name="finalizeOldStatement"></param>
        /// <param name="fromSymbol"></param>
        /// <returns>A new select statement, with the old one as the from clause.</returns>
        private SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement,
            string inputVarName, TypeUsage inputVarType, bool finalizeOldStatement, out Symbol fromSymbol)
        {
            fromSymbol = null;

            // Finalize the old statement
            if (finalizeOldStatement && oldStatement.Select.IsEmpty)
            {
                List<Symbol> columns = AddDefaultColumns(oldStatement);

                // Thid could not have been called from a join node.
                Debug.Assert(oldStatement.FromExtents.Count == 1);

                // if the oldStatement has a join as its input, ...
                // clone the join symbol, so that we "reuse" the
                // join symbol.  Normally, we create a new symbol - see the next block
                // of code.
                JoinSymbol oldJoinSymbol = oldStatement.FromExtents[0] as JoinSymbol;
                if (oldJoinSymbol != null)
                {
                    // Note: oldStatement.FromExtents will not do, since it might
                    // just be an alias of joinSymbol, and we want an actual JoinSymbol.
                    JoinSymbol newJoinSymbol = new JoinSymbol(inputVarName, inputVarType, oldJoinSymbol.ExtentList);
                    // This indicates that the oldStatement is a blocking scope
                    // i.e. it hides/renames extent columns
                    newJoinSymbol.IsNestedJoin = true;
                    newJoinSymbol.ColumnList = columns;
                    newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;

                    fromSymbol = newJoinSymbol;
                }
            }

            if (fromSymbol == null)
            {
                fromSymbol = new Symbol(inputVarName, inputVarType, oldStatement.OutputColumns, oldStatement.OutputColumnsRenamed);
            }

            // Observe that the following looks like the body of Visit(ExtentExpression).
            SqlSelectStatement selectStatement = new SqlSelectStatement();
            selectStatement.From.Append("( ");
            selectStatement.From.Append(oldStatement);
            selectStatement.From.AppendLine();
            selectStatement.From.Append(") ");


            return selectStatement;
        }

        /// <summary>
        /// Before we embed a string literal in a SQL string, we should
        /// convert all ' to '', and enclose the whole string in single quotes.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isUnicode"></param>
        /// <returns>The escaped sql string.</returns>
        private static string EscapeSingleQuote(string s, bool isUnicode)
        {
            return (isUnicode ? "N'" : "'") + s.Replace("'", "''") + "'";
        }

        /// <summary>
        /// Returns the sql primitive/native type name. 
        /// It will include size, precision or scale depending on type information present in the 
        /// type facets
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetSqlPrimitiveType(TypeUsage type)
        {
            Debug.Assert(type.EdmType.DataSpace == DataSpace.CSpace, "Type must be in cSpace");

            TypeUsage storeTypeUsage = this._storeItemCollection.StoreProviderManifest.GetStoreType(type);
            return GenerateSqlForStoreType(this.sqlVersion, storeTypeUsage);
        }

        internal static string GenerateSqlForStoreType(SqlVersion sqlVersion, TypeUsage storeTypeUsage)
        {
            Debug.Assert(Helper.IsPrimitiveType(storeTypeUsage.EdmType), "Type must be primitive type");

            string typeName = storeTypeUsage.EdmType.Name;
            bool hasFacet = false;
            int maxLength = 0;
            byte decimalPrecision = 0;
            byte decimalScale = 0;

            PrimitiveTypeKind primitiveTypeKind = ((PrimitiveType)storeTypeUsage.EdmType).PrimitiveTypeKind;

            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.Binary:
                    if (!TypeHelpers.IsFacetValueConstant(storeTypeUsage, DbProviderManifest.MaxLengthFacetName))
                    {
                        hasFacet = TypeHelpers.TryGetMaxLength(storeTypeUsage, out maxLength);
                        Debug.Assert(hasFacet, "Binary type did not have MaxLength facet");
                        typeName = typeName + "(" + maxLength.ToString(CultureInfo.InvariantCulture) + ")";
                    }
                    break;

                case PrimitiveTypeKind.String:
                    if (!TypeHelpers.IsFacetValueConstant(storeTypeUsage, DbProviderManifest.MaxLengthFacetName))
                    {
                        hasFacet = TypeHelpers.TryGetMaxLength(storeTypeUsage, out maxLength);
                        Debug.Assert(hasFacet, "String type did not have MaxLength facet");
                        typeName = typeName + "(" + maxLength.ToString(CultureInfo.InvariantCulture) + ")";
                    }
                    break;

                case PrimitiveTypeKind.DateTime:
                    typeName = SqlVersionUtils.IsPreKatmai(sqlVersion) ? "datetime" : "datetime2";
                    break;
                case PrimitiveTypeKind.Time:
                    AssertKatmaiOrNewer(sqlVersion, primitiveTypeKind);
                    typeName = "time";
                    break;
                case PrimitiveTypeKind.DateTimeOffset:
                    AssertKatmaiOrNewer(sqlVersion, primitiveTypeKind);
                    typeName = "datetimeoffset";
                    break;

                case PrimitiveTypeKind.Decimal:
                    if (!TypeHelpers.IsFacetValueConstant(storeTypeUsage, DbProviderManifest.PrecisionFacetName))
                    {
                        hasFacet = TypeHelpers.TryGetPrecision(storeTypeUsage, out decimalPrecision);
                        Debug.Assert(hasFacet, "decimal must have precision facet");
                        Debug.Assert(decimalPrecision > 0, "decimal precision must be greater than zero");
                        hasFacet = TypeHelpers.TryGetScale(storeTypeUsage, out decimalScale);
                        Debug.Assert(hasFacet, "decimal must have scale facet");
                        Debug.Assert(decimalPrecision >= decimalScale, "decimalPrecision must be greater or equal to decimalScale");
                        typeName = typeName + "(" + decimalPrecision + "," + decimalScale + ")";
                    }
                    break;

                default:
                    break;
            }

            return typeName;
        }

        /// <summary>
        /// Handles the expression represending DbLimitExpression.Limit and DbSkipExpression.Count.
        /// If it is a constant expression, it simply does to string thus avoiding casting it to the specific value
        /// (which would be done if <see cref="Visit(DbConstantExpression)"/> is called)
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private ISqlFragment HandleCountExpression(DbExpression e)
        {
            ISqlFragment result;

            if (e.ExpressionKind == DbExpressionKind.Constant)
            {
                //For constant expression we should not cast the value, 
                // thus we don't go throught the default DbConstantExpression handling
                SqlBuilder sqlBuilder = new SqlBuilder();
                sqlBuilder.Append(((DbConstantExpression)e).Value.ToString());
                result = sqlBuilder;
            }
            else
            {
                result = e.Accept(this);
            }

            return result;
        }

        /// <summary>
        /// This is used to determine if a particular expression is an Apply operation.
        /// This is only the case when the DbExpressionKind is CrossApply or OuterApply.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        static bool IsApplyExpression(DbExpression e)
        {
            return (DbExpressionKind.CrossApply == e.ExpressionKind || DbExpressionKind.OuterApply == e.ExpressionKind);
        }

        /// <summary>
        /// This is used to determine if a particular expression is a Join operation.
        /// This is true for DbCrossJoinExpression and DbJoinExpression, the
        /// latter of which may have one of several different ExpressionKinds.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        static bool IsJoinExpression(DbExpression e)
        {
            return (DbExpressionKind.CrossJoin == e.ExpressionKind ||
                    DbExpressionKind.FullOuterJoin == e.ExpressionKind ||
                    DbExpressionKind.InnerJoin == e.ExpressionKind ||
                    DbExpressionKind.LeftOuterJoin == e.ExpressionKind);
        }

        /// <summary>
        /// This is used to determine if a calling expression needs to place
        /// round brackets around the translation of the expression e.
        ///
        /// Constants, parameters and properties do not require brackets,
        /// everything else does.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>true, if the expression needs brackets </returns>
        private static bool IsComplexExpression(DbExpression e)
        {
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Constant:
                case DbExpressionKind.ParameterReference:
                case DbExpressionKind.Property:
                case DbExpressionKind.Cast:
                    return false;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Determine if the owner expression can add its unique sql to the input's
        /// SqlSelectStatement
        /// </summary>
        /// <param name="result">The SqlSelectStatement of the input to the relational node.</param>
        /// <param name="expressionKind">The kind of the expression node(not the input's)</param>
        /// <returns></returns>
        private static bool IsCompatible(SqlSelectStatement result, DbExpressionKind expressionKind)
        {
            switch (expressionKind)
            {
                case DbExpressionKind.Distinct:
                    return result.Select.Top == null
                        // #494803: The projection after distinct may not project all 
                        // columns used in the Order By
                        // Improvement: Consider getting rid of the Order By instead
                        && result.OrderBy.IsEmpty;

                case DbExpressionKind.Filter:
                    return result.Select.IsEmpty
                            && result.Where.IsEmpty
                            && result.GroupBy.IsEmpty
                            && result.Select.Top == null;

                case DbExpressionKind.GroupBy:
                    return result.Select.IsEmpty
                            && result.GroupBy.IsEmpty
                            && result.OrderBy.IsEmpty
                            && result.Select.Top == null
                            && !result.Select.IsDistinct;

                case DbExpressionKind.Limit:
                case DbExpressionKind.Element:
                    return result.Select.Top == null;

                case DbExpressionKind.Project:
                    // SQLBUDT #427998: Allow a Project to be compatible with an OrderBy
                    // Otherwise we won't be able to sort an input, and project out only
                    // a subset of the input columns
                    return result.Select.IsEmpty
                            && result.GroupBy.IsEmpty
                            // SQLBUDT #513640 - If distinct is specified, the projection may affect
                            // the cardinality of the results, thus a new statement must be started.
                            && !result.Select.IsDistinct;

                case DbExpressionKind.Skip:
                    return result.Select.IsEmpty
                            && result.GroupBy.IsEmpty
                            && result.OrderBy.IsEmpty
                            && !result.Select.IsDistinct;

                case DbExpressionKind.Sort:
                    return result.Select.IsEmpty
                            && result.GroupBy.IsEmpty
                            && result.OrderBy.IsEmpty
                            // SQLBUDT #513640 - A Project may be on the top of the Sort, and if so, it would need
                            // to be in the same statement as the Sort (see comment above for the Project case).
                            // A Distinct in the same statement would prevent that, and therefore if Distinct is present,
                            // we need to start a new statement. 
                            && !result.Select.IsDistinct;

                default:
                    Debug.Assert(false);
                    throw EntityUtil.InvalidOperation(String.Empty);
            }

        }

        /// <summary>
        /// We use the normal box quotes for SQL server.  We do not deal with ANSI quotes
        /// i.e. double quotes.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string QuoteIdentifier(string name)
        {
            Debug.Assert(!String.IsNullOrEmpty(name));
            // We assume that the names are not quoted to begin with.
            return "[" + name.Replace("]", "]]") + "]";
        }

        /// <summary>
        /// Simply calls <see cref="VisitExpressionEnsureSqlStatement(DbExpression, bool, bool)"/>
        /// with addDefaultColumns set to true and markAllDefaultColumnsAsUsed set to false.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e)
        {
            return VisitExpressionEnsureSqlStatement(e, true, false);
        }

        /// <summary>
        /// This is called from <see cref="GenerateSql(DbQueryCommandTree, out Dictionary<string, bool>)"/> and nodes which require a
        /// select statement as an argument e.g. <see cref="Visit(DbIsEmptyExpression)"/>,
        /// <see cref="Visit(DbUnionAllExpression)"/>.
        ///
        /// SqlGenerator needs its child to have a proper alias if the child is
        /// just an extent or a join.
        ///
        /// The normal relational nodes result in complete valid SQL statements.
        /// For the rest, we need to treat them as there was a dummy
        /// <code>
        /// -- originally {expression}
        /// -- change that to
        /// SELECT *
        /// FROM {expression} as c
        /// </code>
        /// 
        /// DbLimitExpression needs to start the statement but not add the default columns
        /// </summary>
        /// <param name="e"></param>
        /// <param name="addDefaultColumns"></param>
        /// <param name="markAllDefaultColumnsAsUsed"></param>
        /// <returns></returns>
        private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e, bool addDefaultColumns, bool markAllDefaultColumnsAsUsed)
        {
            Debug.Assert(TypeSemantics.IsCollectionType(e.ResultType));

            SqlSelectStatement result;
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Project:
                case DbExpressionKind.Filter:
                case DbExpressionKind.GroupBy:
                case DbExpressionKind.Sort:
                    result = e.Accept(this) as SqlSelectStatement;
                    break;

                default:
                    Symbol fromSymbol;
                    string inputVarName = "c";  // any name will do - this is my random choice.
                    symbolTable.EnterScope();

                    TypeUsage type = null;
                    switch (e.ExpressionKind)
                    {
                        case DbExpressionKind.Scan:
                        case DbExpressionKind.CrossJoin:
                        case DbExpressionKind.FullOuterJoin:
                        case DbExpressionKind.InnerJoin:
                        case DbExpressionKind.LeftOuterJoin:
                        case DbExpressionKind.CrossApply:
                        case DbExpressionKind.OuterApply:
                            // #490026: It used to be type = e.ResultType. 
                            type = TypeHelpers.GetElementTypeUsage(e.ResultType);
                            break;

                        default:
                            Debug.Assert(TypeSemantics.IsCollectionType(e.ResultType));
                            type = TypeHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage;
                            break;
                    }


                    result = VisitInputExpression(e, inputVarName, type, out fromSymbol);
                    AddFromSymbol(result, inputVarName, fromSymbol);
                    symbolTable.ExitScope();
                    break;
            }

            if (addDefaultColumns && result.Select.IsEmpty)
            {
                List<Symbol> defaultColumns = AddDefaultColumns(result);
                if (markAllDefaultColumnsAsUsed)
                {
                    foreach (Symbol symbol in defaultColumns)
                    {
                        this.optionalColumnUsageManager.MarkAsUsed(symbol);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// This method is called by <see cref="Visit(DbFilterExpression)"/> and
        /// <see cref="Visit(DbQuantifierExpression)"/>
        ///
        /// </summary>
        /// <param name="input"></param>
        /// <param name="predicate"></param>
        /// <param name="negatePredicate">This is passed from <see cref="Visit(DbQuantifierExpression)"/>
        /// in the All(...) case.</param>
        /// <returns></returns>
        private SqlSelectStatement VisitFilterExpression(DbExpressionBinding input, DbExpression predicate, bool negatePredicate)
        {
            Symbol fromSymbol;
            SqlSelectStatement result = VisitInputExpression(input.Expression,
                input.VariableName, input.VariableType, out fromSymbol);

            // Filter is compatible with OrderBy
            // but not with Project, another Filter or GroupBy
            if (!IsCompatible(result, DbExpressionKind.Filter))
            {
                result = CreateNewSelectStatement(result, input.VariableName, input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, input.VariableName, fromSymbol);

            if (negatePredicate)
            {
                result.Where.Append("NOT (");
            }
            result.Where.Append(predicate.Accept(this));
            if (negatePredicate)
            {
                result.Where.Append(")");
            }

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        /// <summary>
        /// If the sql fragment for an input expression is not a SqlSelect statement
        /// or other acceptable form (e.g. an extent as a SqlBuilder), we need
        /// to wrap it in a form acceptable in a FROM clause.  These are
        /// primarily the
        /// <list type="bullet">
        /// <item>The set operation expressions - union all, intersect, except</item>
        /// <item>TVFs, which are conceptually similar to tables</item>
        /// </list>
        /// </summary>
        /// <param name="result"></param>
        /// <param name="sqlFragment"></param>
        /// <param name="expressionKind"></param>
        private static void WrapNonQueryExtent(SqlSelectStatement result, ISqlFragment sqlFragment, DbExpressionKind expressionKind)
        {
            switch (expressionKind)
            {
                case DbExpressionKind.Function:
                    // TVF
                    result.From.Append(sqlFragment);
                    break;

                default:
                    result.From.Append(" (");
                    result.From.Append(sqlFragment);
                    result.From.Append(")");
                    break;
            }
        }
        
        private static string ByteArrayToBinaryString( Byte[] binaryArray )
        {
            StringBuilder sb = new StringBuilder( binaryArray.Length * 2 );
            for (int i = 0 ; i < binaryArray.Length ; i++)
            {
                sb.Append(hexDigits[(binaryArray[i]&0xF0) >>4]).Append(hexDigits[binaryArray[i]&0x0F]);
            }
            return sb.ToString();
        }

        private TypeUsage GetPrimitiveType(PrimitiveTypeKind modelType)
        {
            TypeUsage type = null;
            PrimitiveType mappedType = this._storeItemCollection.GetMappedPrimitiveType(modelType);

            Debug.Assert(mappedType != null, "Could not get type usage for primitive type");

            type = TypeUsage.CreateDefaultTypeUsage(mappedType);
            return type;
        }

        /// <summary>
        /// Helper method for the Group By visitor
        /// Returns true if at least one of the aggregates in the given list
        /// has an argument that is not a <see cref="DbConstantExpression"/> and is not 
        /// a <see cref="DbPropertyExpression"/> over <see cref="DbVariableReferenceExpression"/>, 
        /// either potentially capped with a <see cref="DbCastExpression"/>
        /// 
        /// This is really due to the following two limitations of Sql Server:
        /// <list type="number">
        /// <item>If an expression being aggregated contains an outer reference, then that outer 
        /// reference must be the only column referenced in the expression (SQLBUDT #488741)</item>
        /// <item>Sql Server cannot perform an aggregate function on an expression containing 
        /// an aggregate or a subquery. (SQLBUDT #504600)</item>
        /// </list>
        /// Potentially, we could furhter optimize this.
        /// </summary>
        /// <param name="aggregates"></param>
        /// <param name="inputVarRefName"></param>
        /// <returns></returns>
        static bool GroupByAggregatesNeedInnerQuery(IList<DbAggregate> aggregates, string inputVarRefName)
        {
            foreach (DbAggregate aggregate in aggregates)
            {
                Debug.Assert(aggregate.Arguments.Count == 1);
                if (GroupByAggregateNeedsInnerQuery(aggregate.Arguments[0], inputVarRefName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given expression is not a <see cref="DbConstantExpression"/> or a
        /// <see cref="DbPropertyExpression"/> over  a <see cref="DbVariableReferenceExpression"/> 
        /// referencing the given inputVarRefName, either 
        /// potentially capped with a <see cref="DbCastExpression"/>.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="inputVarRefName"></param>
        /// <returns></returns>
        private static bool GroupByAggregateNeedsInnerQuery(DbExpression expression, string inputVarRefName)
        {
            return GroupByExpressionNeedsInnerQuery(expression, inputVarRefName, true);
        }

        /// <summary>
        /// Helper method for the Group By visitor
        /// Returns true if at least one of the expressions in the given list
        /// is not <see cref="DbPropertyExpression"/> over <see cref="DbVariableReferenceExpression"/> 
        /// referencing the given inputVarRefName potentially capped with a <see cref="DbCastExpression"/>.
        /// 
        /// This is really due to the following limitation: Sql Server requires each GROUP BY expression 
        /// (key) to contain at least one column that is not an outer reference. (SQLBUDT #616523)
        /// Potentially, we could further optimize this.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="inputVarRefName"></param>
        /// <returns></returns>
        static bool GroupByKeysNeedInnerQuery(IList<DbExpression> keys, string inputVarRefName)
        {
            foreach (DbExpression key in keys)
            {
                if (GroupByKeyNeedsInnerQuery(key, inputVarRefName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given expression is not <see cref="DbPropertyExpression"/> over 
        /// <see cref="DbVariableReferenceExpression"/> referencing the given inputVarRefName
        /// potentially capped with a <see cref="DbCastExpression"/>.
        /// This is really due to the following limitation: Sql Server requires each GROUP BY expression 
        /// (key) to contain at least one column that is not an outer reference. (SQLBUDT #616523)
        /// Potentially, we could further optimize this.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="inputVarRefName"></param>
        /// <returns></returns>
        private static bool GroupByKeyNeedsInnerQuery(DbExpression expression, string inputVarRefName)
        {
            return GroupByExpressionNeedsInnerQuery(expression, inputVarRefName, false);
        }

        /// <summary>
        /// Helper method for processing Group By keys and aggregates.
        /// Returns true if the given expression is not a <see cref="DbConstantExpression"/> 
        /// (and allowConstants is specified)or a <see cref="DbPropertyExpression"/> over 
        /// a <see cref="DbVariableReferenceExpression"/> referencing the given inputVarRefName,
        /// either potentially capped with a <see cref="DbCastExpression"/>.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="inputVarRefName"></param>
        /// <param name="allowConstants"></param>
        /// <returns></returns>
        private static bool GroupByExpressionNeedsInnerQuery(DbExpression expression, string inputVarRefName, bool allowConstants)
        {
            //Skip a constant if constants are allowed
            if (allowConstants && (expression.ExpressionKind == DbExpressionKind.Constant))
            {
                return false;
            }

            //Skip a cast expression
            if (expression.ExpressionKind == DbExpressionKind.Cast)
            {
                DbCastExpression castExpression = (DbCastExpression)expression;
                return GroupByExpressionNeedsInnerQuery(castExpression.Argument, inputVarRefName, allowConstants);
            }

            //Allow Property(Property(...)), needed when the input is a join
            if (expression.ExpressionKind == DbExpressionKind.Property)
            {
                DbPropertyExpression propertyExpression = (DbPropertyExpression)expression;
                return GroupByExpressionNeedsInnerQuery(propertyExpression.Instance, inputVarRefName, allowConstants);
            }

            if (expression.ExpressionKind == DbExpressionKind.VariableReference)
            {
                DbVariableReferenceExpression varRefExpression = expression as DbVariableReferenceExpression;
                return !varRefExpression.VariableName.Equals(inputVarRefName);
            }

            return true;
        }
                
        /// <summary>
        /// Throws not supported exception if the server is pre-katmai
        /// </summary>
        /// <param name="primitiveTypeKind"></param>
        private void AssertKatmaiOrNewer(PrimitiveTypeKind primitiveTypeKind)
        {
            AssertKatmaiOrNewer(this.sqlVersion, primitiveTypeKind);
        }

        private static void AssertKatmaiOrNewer(SqlVersion sqlVersion, PrimitiveTypeKind primitiveTypeKind)
        {
            if (SqlVersionUtils.IsPreKatmai(sqlVersion))
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.SqlGen_PrimitiveTypeNotSupportedPriorSql10(primitiveTypeKind));
            }
        }

        /// <summary>
        /// Throws not supported exception if the server is pre-katmai
        /// </summary>
        /// <param name="e"></param>
        internal void AssertKatmaiOrNewer(DbFunctionExpression e)
        {
            if (this.IsPreKatmai)
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.SqlGen_CanonicalFunctionNotSupportedPriorSql10(e.Function.Name));
            }
        }

        #endregion
        
        #endregion
    }
}

