//---------------------------------------------------------------------
// <copyright file="DmlSqlGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.SqlClient.SqlGen
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.Utils;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class generating SQL for a DML command tree.
    /// </summary>
    internal static class DmlSqlGenerator
    {
        private const int s_commandTextBuilderInitialCapacity = 256;
        private const string s_generatedValuesVariableName = "@generated_keys";

        internal static string GenerateUpdateSql(DbUpdateCommandTree tree, SqlVersion sqlVersion, out List<SqlParameter> parameters)
        {
            const string dummySetParameter = "@p";

            StringBuilder commandText = new StringBuilder(s_commandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, null != tree.Returning, sqlVersion);

            if (tree.SetClauses.Count == 0)
            {
                commandText.AppendLine("declare " + dummySetParameter + " int");
            }

            // update [schemaName].[tableName]
            commandText.Append("update ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();

            // set c1 = ..., c2 = ..., ...
            bool first = true;
            commandText.Append("set ");
            foreach (DbSetClause setClause in tree.SetClauses)
            {
                if (first) { first = false; }
                else { commandText.Append(", "); }
                setClause.Property.Accept(translator);
                commandText.Append(" = ");
                setClause.Value.Accept(translator);
            }

            if (first)
            {
                // If first is still true, it indicates there were no set
                // clauses. Introduce a fake set clause so that:
                // - we acquire the appropriate locks
                // - server-gen columns (e.g. timestamp) get recomputed
                //
                // We use the following pattern:
                //
                //  update Foo
                //  set @p = 0
                //  where ...
                commandText.Append(dummySetParameter + " = 0");
            }
            commandText.AppendLine();

            // where c1 = ..., c2 = ...
            commandText.Append("where ");
            tree.Predicate.Accept(translator);
            commandText.AppendLine();

            // generate returning sql
            GenerateReturningSql(commandText, tree, null, translator, tree.Returning, false); 

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        internal static string GenerateDeleteSql(DbDeleteCommandTree tree, SqlVersion sqlVersion, out List<SqlParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(s_commandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, false, sqlVersion);

            // delete [schemaName].[tableName]
            commandText.Append("delete ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();
            
            // where c1 = ... AND c2 = ...
            commandText.Append("where ");
            tree.Predicate.Accept(translator);

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        internal static string GenerateInsertSql(DbInsertCommandTree tree, SqlVersion sqlVersion, out List<SqlParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(s_commandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree,
                null != tree.Returning, sqlVersion);

            bool useGeneratedValuesVariable = UseGeneratedValuesVariable(tree, sqlVersion, translator);
            EntityType tableType = (EntityType)((DbScanExpression)tree.Target.Expression).Target.ElementType;

            if (useGeneratedValuesVariable)
            {
                // manufacture the variable, e.g. "declare @generated_values table(id uniqueidentifier)"
                commandText
                    .Append("declare ")
                    .Append(s_generatedValuesVariableName)
                    .Append(" table(");
                bool first = true;
                foreach (EdmMember column in tableType.KeyMembers)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        commandText.Append(", ");
                    }
                    string columnType = SqlGenerator.GenerateSqlForStoreType(sqlVersion, column.TypeUsage);
                    if (columnType == "rowversion" || columnType == "timestamp")
                    {
                        // rowversion and timestamp are intrinsically read-only. use binary to gather server generated
                        // values for these types.
                        columnType = "binary(8)";
                    }
                    commandText
                        .Append(GenerateMemberTSql(column))
                        .Append(" ")
                        .Append(columnType);
                    Facet collationFacet;
                    if (column.TypeUsage.Facets.TryGetValue(DbProviderManifest.CollationFacetName, false, out collationFacet))
                    {
                        string collation = collationFacet.Value as string;
                        if (!string.IsNullOrEmpty(collation))
                        {
                            commandText.Append(" collate ").Append(collation);
                        }
                    }
                }
                Debug.Assert(!first, "if useGeneratedValuesVariable is true, it implies some columns do not have values");
                commandText.AppendLine(")");
            }

            // insert [schemaName].[tableName]
            commandText.Append("insert ");
            tree.Target.Expression.Accept(translator);

            if (0 < tree.SetClauses.Count)
            {
                // (c1, c2, c3, ...)
                commandText.Append("(");
                bool first = true;
                foreach (DbSetClause setClause in tree.SetClauses)
                {
                    if (first) { first = false; }
                    else { commandText.Append(", "); }
                    setClause.Property.Accept(translator);
                }
                commandText.AppendLine(")");
            }
            else
            {
                commandText.AppendLine();
            }

            if (useGeneratedValuesVariable)
            {
                // output inserted.id into @generated_values
                commandText.Append("output ");
                bool first = true;
                foreach (EdmMember column in tableType.KeyMembers)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        commandText.Append(", ");
                    }
                    commandText.Append("inserted.");
                    commandText.Append(GenerateMemberTSql(column));
                }
                commandText
                    .Append(" into ")
                    .AppendLine(s_generatedValuesVariableName);
            }

            if (0 < tree.SetClauses.Count)
            {
                // values c1, c2, ...
                bool first = true;
                commandText.Append("values (");
                foreach (DbSetClause setClause in tree.SetClauses)
                {
                    if (first) { first = false; }
                    else { commandText.Append(", "); }
                    setClause.Value.Accept(translator);

                    translator.RegisterMemberValue(setClause.Property, setClause.Value);
                }
                commandText.AppendLine(")");
            }
            else
            {
                // default values
                commandText.AppendLine("default values");
            }
            // generate returning sql
            GenerateReturningSql(commandText, tree, tableType, translator, tree.Returning, useGeneratedValuesVariable); 

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        /// <summary>
        /// Determine whether we should use a generated values variable to return server generated values.
        /// This is true when we're attempting to insert a row where the primary key is server generated
        /// but is not an integer type (and therefore can't be used with scope_identity()). It is also true
        /// where there is a compound server generated key.        
        /// </summary>
        private static bool UseGeneratedValuesVariable(DbInsertCommandTree tree, SqlVersion sqlVersion, ExpressionTranslator translator)
        {
            bool useGeneratedValuesVariable = false;
            if (sqlVersion > SqlVersion.Sql8 && tree.Returning != null)
            {
                // Figure out which columns have values
                HashSet<EdmMember> columnsWithValues = new HashSet<EdmMember>(tree.SetClauses.Cast<DbSetClause>().Select(s => ((DbPropertyExpression)s.Property).Property));

                // Only SQL Server 2005+ support an output clause for inserts
                bool firstKeyFound = false;
                foreach (EdmMember keyMember in ((DbScanExpression)tree.Target.Expression).Target.ElementType.KeyMembers)
                {
                    if (!columnsWithValues.Contains(keyMember))
                    {
                        if (firstKeyFound)
                        {
                            // compound server gen key
                            useGeneratedValuesVariable = true;
                            break;
                        }
                        else
                        {
                            firstKeyFound = true;
                            if (!IsValidScopeIdentityColumnType(keyMember.TypeUsage))
                            {
                                // unsupported type
                                useGeneratedValuesVariable = true;
                                break;
                            }
                        }
                    }
                }
            }
            return useGeneratedValuesVariable;
        }

        // Generates T-SQL describing a member
        // Requires: member must belong to an entity type (a safe requirement for DML
        // SQL gen, where we only access table columns)
        private static string GenerateMemberTSql(EdmMember member)
        {
            EntityType entityType = (EntityType)member.DeclaringType;
            string sql;
            if (!entityType.TryGetMemberSql(member, out sql))
            {
                sql = SqlGenerator.QuoteIdentifier(member.Name);
                entityType.SetMemberSql(member, sql);
            }
            return sql;
        }

        /// <summary>
        /// Generates SQL fragment returning server-generated values.
        /// Requires: translator knows about member values so that we can figure out
        /// how to construct the key predicate.
        /// <code>
        /// Sample SQL:
        ///     
        ///     select IdentityValue
        ///     from dbo.MyTable
        ///     where @@ROWCOUNT > 0 and IdentityValue = scope_identity()
        /// 
        /// or
        /// 
        ///     select TimestampValue
        ///     from dbo.MyTable
        ///     where @@ROWCOUNT > 0 and Id = 1
        /// 
        /// Note that we filter on rowcount to ensure no rows are returned if no rows were modified.
        /// 
        /// On SQL Server 2005 and up, we have an additional syntax used for non integer return types:
        /// 
        ///     declare @generatedValues table(ID uniqueidentifier)
        ///     insert dbo.MyTable
        ///     output ID into @generated_values
        ///     values (...);
        ///     select ID
        ///     from @generatedValues as g join dbo.MyTable as t on g.ID = t.ID
        ///     where @@ROWCOUNT > 0;
        /// </code>
        /// </summary>
        /// <param name="commandText">Builder containing command text</param>
        /// <param name="tree">Modification command tree</param>
        /// <param name="tableType">Type of table.</param>
        /// <param name="translator">Translator used to produce DML SQL statement
        /// for the tree</param>
        /// <param name="returning">Returning expression. If null, the method returns
        /// immediately without producing a SELECT statement.</param>
        private static void GenerateReturningSql(StringBuilder commandText, DbModificationCommandTree tree, EntityType tableType,
            ExpressionTranslator translator, DbExpression returning, bool useGeneratedValuesVariable)
        {
            // Nothing to do if there is no Returning expression
            if (null == returning) { return; }

            // select
            commandText.Append("select ");
            if (useGeneratedValuesVariable)
            {
                translator.PropertyAlias = "t";
            }
            returning.Accept(translator);
            if (useGeneratedValuesVariable)
            {
                translator.PropertyAlias = null;
            }
            commandText.AppendLine();

            if (useGeneratedValuesVariable)
            {
                // from @generated_values
                commandText.Append("from ");
                commandText.Append(s_generatedValuesVariableName);
                commandText.Append(" as g join ");
                tree.Target.Expression.Accept(translator);
                commandText.Append(" as t on ");
                string separator = string.Empty;
                foreach (EdmMember keyMember in tableType.KeyMembers)
                {
                    commandText.Append(separator);
                    separator = " and ";
                    commandText.Append("g.");
                    string memberTSql = GenerateMemberTSql(keyMember);
                    commandText.Append(memberTSql);
                    commandText.Append(" = t.");
                    commandText.Append(memberTSql);
                }
                commandText.AppendLine();
                commandText.Append("where @@ROWCOUNT > 0");
            }
            else
            {
                // from
                commandText.Append("from ");
                tree.Target.Expression.Accept(translator);
                commandText.AppendLine();

                // where
                commandText.Append("where @@ROWCOUNT > 0");
                EntitySetBase table = ((DbScanExpression)tree.Target.Expression).Target;
                bool identity = false;
                foreach (EdmMember keyMember in table.ElementType.KeyMembers)
                {
                    commandText.Append(" and ");
                    commandText.Append(GenerateMemberTSql(keyMember));
                    commandText.Append(" = ");

                    // retrieve member value sql. the translator remembers member values
                    // as it constructs the DML statement (which precedes the "returning"
                    // SQL)
                    SqlParameter value;
                    if (translator.MemberValues.TryGetValue(keyMember, out value))
                    {
                        commandText.Append(value.ParameterName);
                    }
                    else
                    {
                        // if no value is registered for the key member, it means it is an identity
                        // which can be retrieved using the scope_identity() function
                        if (identity)
                        {
                            // there can be only one server generated key
                            throw EntityUtil.NotSupported(System.Data.Entity.Strings.Update_NotSupportedServerGenKey(table.Name));
                        }

                        if (!IsValidScopeIdentityColumnType(keyMember.TypeUsage))
                        {
                            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.Update_NotSupportedIdentityType(
                                keyMember.Name, keyMember.TypeUsage.ToString()));
                        }

                        commandText.Append("scope_identity()");
                        identity = true;
                    }
                }
            }
        }

        private static bool IsValidScopeIdentityColumnType(TypeUsage typeUsage)
        {
            // SQL Server supports the following types for identity columns:
            // tinyint, smallint, int, bigint, decimal(p,0), or numeric(p,0)

            // make sure it's a primitive type
            if (typeUsage.EdmType.BuiltInTypeKind != BuiltInTypeKind.PrimitiveType)
            {
                return false;
            }

            // check if this is a supported primitive type (compare by name)
            string typeName = typeUsage.EdmType.Name;

            // integer types
            if (typeName == "tinyint" || typeName == "smallint" ||
                typeName == "int" || typeName == "bigint")
            {
                return true;
            }

            // variable scale types (require scale = 0)
            if (typeName == "decimal" || typeName == "numeric")
            {
                Facet scaleFacet;
                return (typeUsage.Facets.TryGetValue(DbProviderManifest.ScaleFacetName,
                    false, out scaleFacet) && Convert.ToInt32(scaleFacet.Value, CultureInfo.InvariantCulture) == 0);
            }

            // type not in supported list
            return false;
        }

        /// <summary>
        /// Lightweight expression translator for DML expression trees, which have constrained
        /// scope and support.
        /// </summary>
        private class ExpressionTranslator : BasicExpressionVisitor
        {
            /// <summary>
            /// Initialize a new expression translator populating the given string builder
            /// with command text. Command text builder and command tree must not be null.
            /// </summary>
            /// <param name="commandText">Command text with which to populate commands</param>
            /// <param name="commandTree">Command tree generating SQL</param>
            /// <param name="preserveMemberValues">Indicates whether the translator should preserve
            /// member values while compiling t-SQL (only needed for server generation)</param>
            internal ExpressionTranslator(StringBuilder commandText, DbModificationCommandTree commandTree, 
                bool preserveMemberValues, SqlVersion version)
            {
                Debug.Assert(null != commandText);
                Debug.Assert(null != commandTree);
                _commandText = commandText;
                _commandTree = commandTree;
                _version = version;
                _parameters = new List<SqlParameter>();
                _memberValues = preserveMemberValues ? new Dictionary<EdmMember, SqlParameter>() :
                    null;
            }

            private readonly StringBuilder _commandText;
            private readonly DbModificationCommandTree _commandTree;
            private readonly List<SqlParameter> _parameters;
            private readonly Dictionary<EdmMember, SqlParameter> _memberValues;
            private readonly static AliasGenerator s_parameterNames = new AliasGenerator("@", 1000);
            private readonly SqlVersion _version;

            internal List<SqlParameter> Parameters { get { return _parameters; } }
            internal Dictionary<EdmMember, SqlParameter> MemberValues { get { return _memberValues; } }
            internal string PropertyAlias { get; set; }

            // generate parameter (name based on parameter ordinal)
            internal SqlParameter CreateParameter(object value, TypeUsage type)
            {
                // Suppress the MaxLength facet in the type usage because
                // SqlClient will silently truncate data when SqlParameter.Size < |SqlParameter.Value|.
                const bool preventTruncation = true;
                SqlParameter parameter = SqlProviderServices.CreateSqlParameter(s_parameterNames.GetName(_parameters.Count), type, ParameterMode.In, value, preventTruncation, _version);

                _parameters.Add(parameter);

                return parameter;
            }

            public override void Visit(DbAndExpression expression)
            {
                VisitBinary(expression, " and ");
            }

            public override void Visit(DbOrExpression expression)
            {
                VisitBinary(expression, " or ");
            }

            public override void Visit(DbComparisonExpression expression)
            {
                Debug.Assert(expression.ExpressionKind == DbExpressionKind.Equals,
                    "only equals comparison expressions are produced in DML command trees in V1");

                VisitBinary(expression, " = ");

                RegisterMemberValue(expression.Left, expression.Right);
            }

            /// <summary>
            /// Call this method to register a property value pair so the translator "remembers"
            /// the values for members of the row being modified. These values can then be used
            /// to form a predicate for server-generation (based on the key of the row)
            /// </summary>
            /// <param name="propertyExpression">DbExpression containing the column reference (property expression).</param>
            /// <param name="value">DbExpression containing the value of the column.</param>
            internal void RegisterMemberValue(DbExpression propertyExpression, DbExpression value)
            {
                if (null != _memberValues)
                {
                    // register the value for this property
                    Debug.Assert(propertyExpression.ExpressionKind == DbExpressionKind.Property,
                        "DML predicates and setters must be of the form property = value");

                    // get name of left property 
                    EdmMember property = ((DbPropertyExpression)propertyExpression).Property;

                    // don't track null values
                    if (value.ExpressionKind != DbExpressionKind.Null)
                    {
                        Debug.Assert(value.ExpressionKind == DbExpressionKind.Constant,
                            "value must either constant or null");
                        // retrieve the last parameter added (which describes the parameter)
                        _memberValues[property] = _parameters[_parameters.Count - 1];
                    }
                }
            }

            public override void Visit(DbIsNullExpression expression)
            {
                expression.Argument.Accept(this);
                _commandText.Append(" is null");
            }

            public override void Visit(DbNotExpression expression)
            {
                _commandText.Append("not (");
                expression.Accept(this);
                _commandText.Append(")");
            }

            public override void Visit(DbConstantExpression expression)
            {
                SqlParameter parameter = CreateParameter(expression.Value, expression.ResultType);
                _commandText.Append(parameter.ParameterName);
            }

            public override void Visit(DbScanExpression expression)
            {
                // we know we won't hit this code unless there is no function defined for this
                // ModificationOperation, so if this EntitySet is using a DefiningQuery, instead
                // of a table, that is an error
                if (expression.Target.DefiningQuery != null)
                {
                    string missingCudElement;
                    if (_commandTree.CommandTreeKind == DbCommandTreeKind.Delete)
                    {
                        missingCudElement = StorageMslConstructs.DeleteFunctionElement;
                    }
                    else if (_commandTree.CommandTreeKind == DbCommandTreeKind.Insert)
                    {
                        missingCudElement = StorageMslConstructs.InsertFunctionElement;
                    }
                    else
                    {
                        Debug.Assert(_commandTree.CommandTreeKind == DbCommandTreeKind.Update, "did you add a new option?");
                        missingCudElement = StorageMslConstructs.UpdateFunctionElement;
                    }
                    throw EntityUtil.Update(System.Data.Entity.Strings.Update_SqlEntitySetWithoutDmlFunctions(expression.Target.Name, missingCudElement, StorageMslConstructs.ModificationFunctionMappingElement), null);
                }

                _commandText.Append(SqlGenerator.GetTargetTSql(expression.Target));
            }

            public override void Visit(DbPropertyExpression expression)
            {
                if (!string.IsNullOrEmpty(this.PropertyAlias))
                {
                    _commandText.Append(this.PropertyAlias);
                    _commandText.Append(".");
                }
                _commandText.Append(GenerateMemberTSql(expression.Property));
            }

            public override void Visit(DbNullExpression expression)
            {
                _commandText.Append("null");
            }

            public override void Visit(DbNewInstanceExpression expression)
            {
                // assumes all arguments are self-describing (no need to use aliases
                // because no renames are ever used in the projection)
                bool first = true;
                foreach (DbExpression argument in expression.Arguments)
                {
                    if (first) { first = false; }
                    else { _commandText.Append(", "); }
                    argument.Accept(this);
                }
            }

            private void VisitBinary(DbBinaryExpression expression, string separator)
            {
                _commandText.Append("(");
                expression.Left.Accept(this);
                _commandText.Append(separator);
                expression.Right.Accept(this);
                _commandText.Append(")");
            }

        }
    }
}

